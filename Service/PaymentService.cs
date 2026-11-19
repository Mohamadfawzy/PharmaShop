using Contracts;
using Contracts.IServices;
using Entities.Models;
using Shared.Constants;
using Shared.Models.Dtos.Payments;
using Shared.Responses;

namespace Service;

public sealed class PaymentService : IPaymentService
{
    private const byte Pending = 1, Succeeded = 2, Failed = 3, Cancelled = 4;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;

    public PaymentService(IUnitOfWork uow, ICurrentUserService currentUser, INotificationService notifications)
    { _uow = uow; _currentUser = currentUser; _notifications = notifications; }

    public async Task<AppResponse<List<PaymentAttemptDto>>> GetOrderPaymentsAsync(int orderId, CancellationToken ct)
    {
        var order = await _uow.Orders.GetOrderForStatusUpdateAsync(orderId, ct);
        if (order is null) return AppResponse<List<PaymentAttemptDto>>.NotFound("Order not found");
        if (!await CanManageOrderAsync(order, ct)) return AppResponse<List<PaymentAttemptDto>>.Forbidden("You cannot manage payments for another pharmacy");
        return AppResponse<List<PaymentAttemptDto>>.Ok(await GetDtosAsync(orderId, ct));
    }

    public async Task<AppResponse<PaymentAttemptDto>> CreateAttemptAsync(int orderId, CreatePaymentAttemptDto dto, CancellationToken ct)
    {
        if (orderId <= 0 || dto is null || string.IsNullOrWhiteSpace(dto.Provider) || dto.Provider.Trim().Length > 50)
            return AppResponse<PaymentAttemptDto>.ValidationError("OrderId and a Provider name (max 50 characters) are required");
        if (!int.TryParse(_currentUser.UserId, out var userId)) return AppResponse<PaymentAttemptDto>.Unauthorized("User identity is invalid");
        await using var tx = await _uow.BeginTransactionAsync(ct);
        await _uow.Orders.LockOrderForStatusUpdateAsync(orderId, ct);
        var order = await _uow.Orders.GetOrderForStatusUpdateAsync(orderId, ct);
        if (order is null) return AppResponse<PaymentAttemptDto>.NotFound("Order not found");
        if (!await CanManageOrderAsync(order, ct)) return AppResponse<PaymentAttemptDto>.Forbidden("You cannot manage payments for another pharmacy");
        if (order.PaymentStatus != 1 || order.Status is 6 or 7 || order.GrandTotal <= 0)
            return AppResponse<PaymentAttemptDto>.BusinessRuleViolation("A payment attempt can only be recorded for an unpaid order that is not cancelled or returned");
        var attempt = new PaymentAttempt
        {
            OrderId = order.Id, Provider = dto.Provider.Trim(), Amount = order.GrandTotal,
            Status = Pending, CreatedByUserId = userId, CreatedAt = DateTime.UtcNow
        };
        await _uow.PaymentAttempts.AddAsync(attempt, ct);
        await _uow.CompleteAsync(ct); await tx.CommitAsync(ct);
        return AppResponse<PaymentAttemptDto>.Created(Map(attempt), $"/api/v1/admin/payment-attempts/{attempt.Id}", "Payment attempt recorded");
    }

    public async Task<AppResponse<PaymentAttemptDto>> UpdateAttemptAsync(int attemptId, UpdatePaymentAttemptDto dto, CancellationToken ct)
    {
        if (attemptId <= 0 || dto is null || dto.Status is < 2 or > 4 || dto.ExternalReference?.Trim().Length > 200 || dto.FailureReason?.Trim().Length > 500)
            return AppResponse<PaymentAttemptDto>.ValidationError("Status must be 2 (Succeeded), 3 (Failed), or 4 (Cancelled); optional text fields are limited to 200/500 characters");
        if (!int.TryParse(_currentUser.UserId, out var userId)) return AppResponse<PaymentAttemptDto>.Unauthorized("User identity is invalid");
        var attempt = await _uow.PaymentAttempts.GetByIdNoTrackingAsync(attemptId, ct);
        if (attempt is null) return AppResponse<PaymentAttemptDto>.NotFound("Payment attempt not found");
        await using var tx = await _uow.BeginTransactionAsync(ct);
        await _uow.Orders.LockOrderForStatusUpdateAsync(attempt.OrderId, ct);
        var order = await _uow.Orders.GetOrderForStatusUpdateAsync(attempt.OrderId, ct);
        if (order is null) return AppResponse<PaymentAttemptDto>.NotFound("Order not found");
        if (!await CanManageOrderAsync(order, ct)) return AppResponse<PaymentAttemptDto>.Forbidden("You cannot manage payments for another pharmacy");
        attempt = await _uow.PaymentAttempts.FirstOrDefaultAsync(p => p.Id == attemptId, asNoTracking: true, ct: ct);
        if (attempt is null) return AppResponse<PaymentAttemptDto>.NotFound("Payment attempt not found");
        if (attempt.Status != Pending) return AppResponse<PaymentAttemptDto>.Conflict("Only a pending payment attempt can be updated");
        if (dto.Status == Succeeded && (order.Status is 6 or 7 || order.PaymentStatus != 1))
            return AppResponse<PaymentAttemptDto>.BusinessRuleViolation("This order cannot accept a successful payment in its current state");
        if (!string.IsNullOrWhiteSpace(dto.ExternalReference))
        {
            var reference = dto.ExternalReference.Trim();
            if (await _uow.PaymentAttempts.AnyAsync(p => p.Id != attempt.Id && p.Provider == attempt.Provider && p.ExternalReference == reference, ct))
                return AppResponse<PaymentAttemptDto>.Conflict("ExternalReference has already been used by this provider");
            attempt.ExternalReference = reference;
        }
        attempt.Status = dto.Status; attempt.FailureReason = dto.FailureReason?.Trim(); attempt.UpdatedAt = DateTime.UtcNow;
        _uow.PaymentAttempts.Update(attempt);
        if (dto.Status == Succeeded) { order.PaymentStatus = 2; _uow.Orders.UpdateOrder(order); }
        await _uow.CompleteAsync(ct); await tx.CommitAsync(ct);
        if (dto.Status == Succeeded)
            await _notifications.NotifyCustomerAsync(order.CustomerId, "Order", "تم تأكيد الدفع", $"تم تأكيد دفع الطلب رقم {order.OrderNumber}.", "Order", order.Id, ct);
        attempt = await _uow.PaymentAttempts.GetByIdAsync(attemptId, ct) ?? attempt;
        return AppResponse<PaymentAttemptDto>.Ok(Map(attempt), "Payment attempt updated");
    }

    public async Task<AppResponse<PaymentRefundDto>> CreateRefundAsync(int orderId, CreatePaymentRefundDto dto, CancellationToken ct)
    {
        if (orderId <= 0 || dto is null || dto.PaymentAttemptId <= 0 || dto.Amount <= 0 || string.IsNullOrWhiteSpace(dto.Reason) || dto.Reason.Trim().Length > 500)
            return AppResponse<PaymentRefundDto>.ValidationError("PaymentAttemptId, a positive Amount, and Reason (max 500 characters) are required");
        if (!int.TryParse(_currentUser.UserId, out var userId)) return AppResponse<PaymentRefundDto>.Unauthorized("User identity is invalid");
        await using var tx = await _uow.BeginTransactionAsync(ct);
        await _uow.Orders.LockOrderForStatusUpdateAsync(orderId, ct);
        var order = await _uow.Orders.GetOrderForStatusUpdateAsync(orderId, ct);
        if (order is null) return AppResponse<PaymentRefundDto>.NotFound("Order not found");
        if (!await CanManageOrderAsync(order, ct)) return AppResponse<PaymentRefundDto>.Forbidden("You cannot manage payments for another pharmacy");
        if (dto.Amount < 0.01m) return AppResponse<PaymentRefundDto>.ValidationError("Amount must be at least 0.01");
        var attempt = await _uow.PaymentAttempts.FirstOrDefaultAsync(p => p.Id == dto.PaymentAttemptId && p.OrderId == orderId && p.Status == Succeeded, asNoTracking: true, ct: ct);
        if (attempt is null) return AppResponse<PaymentRefundDto>.BusinessRuleViolation("A successful payment attempt for this order is required");
        var refunds = await _uow.PaymentRefunds.FindAsync(r => r.PaymentAttemptId == attempt.Id && r.Status != Failed, ct: ct);
        if (refunds.Sum(r => r.Amount) + dto.Amount > attempt.Amount)
            return AppResponse<PaymentRefundDto>.BusinessRuleViolation("Refund amount exceeds the remaining captured amount");
        var refund = new PaymentRefund
        {
            PaymentAttemptId = attempt.Id, Amount = decimal.Round(dto.Amount, 2), Status = 1,
            Reason = dto.Reason.Trim(), RequestedByUserId = userId, CreatedAt = DateTime.UtcNow
        };
        await _uow.PaymentRefunds.AddAsync(refund, ct);
        await _uow.CompleteAsync(ct); await tx.CommitAsync(ct);
        return AppResponse<PaymentRefundDto>.Created(Map(refund), $"/api/v1/admin/payment-refunds/{refund.Id}", "Refund request recorded");
    }

    public async Task<AppResponse<PaymentRefundDto>> UpdateRefundAsync(int refundId, UpdatePaymentRefundDto dto, CancellationToken ct)
    {
        if (refundId <= 0 || dto is null || dto.Status is < 2 or > 4 || dto.ExternalReference?.Trim().Length > 200)
            return AppResponse<PaymentRefundDto>.ValidationError("Status must be 2 (Processing), 3 (Succeeded), or 4 (Failed); ExternalReference is limited to 200 characters");
        if (!int.TryParse(_currentUser.UserId, out var userId)) return AppResponse<PaymentRefundDto>.Unauthorized("User identity is invalid");
        var refund = await _uow.PaymentRefunds.GetByIdNoTrackingAsync(refundId, ct);
        if (refund is null) return AppResponse<PaymentRefundDto>.NotFound("Refund request not found");
        var attempt = await _uow.PaymentAttempts.GetByIdAsync(refund.PaymentAttemptId, ct);
        if (attempt is null) return AppResponse<PaymentRefundDto>.NotFound("Payment attempt not found");
        await using var tx = await _uow.BeginTransactionAsync(ct);
        await _uow.Orders.LockOrderForStatusUpdateAsync(attempt.OrderId, ct);
        var order = await _uow.Orders.GetOrderForStatusUpdateAsync(attempt.OrderId, ct);
        if (order is null) return AppResponse<PaymentRefundDto>.NotFound("Order not found");
        if (!await CanManageOrderAsync(order, ct)) return AppResponse<PaymentRefundDto>.Forbidden("You cannot manage payments for another pharmacy");
        refund = await _uow.PaymentRefunds.FirstOrDefaultAsync(r => r.Id == refundId, asNoTracking: true, ct: ct);
        if (refund is null) return AppResponse<PaymentRefundDto>.NotFound("Refund request not found");
        var allowed = refund.Status == 1 ? dto.Status is 2 or 4 : refund.Status == 2 && dto.Status is 3 or 4;
        if (!allowed) return AppResponse<PaymentRefundDto>.Conflict("Invalid refund state transition");
        refund.Status = dto.Status; refund.ProcessedByUserId = userId;
        if (!string.IsNullOrWhiteSpace(dto.ExternalReference))
        {
            var reference = dto.ExternalReference.Trim();
            if (await _uow.PaymentRefunds.AnyAsync(r => r.Id != refund.Id && r.ExternalReference == reference, ct))
                return AppResponse<PaymentRefundDto>.Conflict("ExternalReference has already been used by another refund");
            refund.ExternalReference = reference;
        }
        if (dto.Status is 3 or 4) refund.ProcessedAt = DateTime.UtcNow;
        _uow.PaymentRefunds.Update(refund);
        if (dto.Status == 3)
        {
            var allRefunds = await _uow.PaymentRefunds.FindAsync(r => r.PaymentAttemptId == attempt.Id && r.Status == 3, ct: ct);
            var refundedTotal = allRefunds.Sum(r => r.Amount) + refund.Amount;
            if (refundedTotal >= order.GrandTotal) { order.PaymentStatus = 3; _uow.Orders.UpdateOrder(order); }
        }
        await _uow.CompleteAsync(ct); await tx.CommitAsync(ct);
        if (dto.Status == 3)
            await _notifications.NotifyCustomerAsync(order.CustomerId, "Order", "تم تنفيذ استرداد", $"تم تسجيل استرداد بقيمة {refund.Amount:0.00} للطلب رقم {order.OrderNumber}.", "Order", order.Id, ct);
        return AppResponse<PaymentRefundDto>.Ok(Map(refund), "Refund request updated");
    }

    private async Task<List<PaymentAttemptDto>> GetDtosAsync(int orderId, CancellationToken ct)
    {
        var attempts = await _uow.PaymentAttempts.GetAllAsync(criteria: p => p.OrderId == orderId,
            orderBy: q => q.OrderByDescending(p => p.CreatedAt), asNoTracking: true, ct: ct);
        var result = new List<PaymentAttemptDto>(attempts.Count);
        foreach (var attempt in attempts)
        {
            var dto = Map(attempt);
            dto.Refunds = (await _uow.PaymentRefunds.GetAllAsync(criteria: r => r.PaymentAttemptId == attempt.Id,
                orderBy: q => q.OrderBy(r => r.CreatedAt), asNoTracking: true, ct: ct)).Select(Map).ToList();
            result.Add(dto);
        }
        return result;
    }

    private async Task<bool> CanManageOrderAsync(Order order, CancellationToken ct)
    {
        if (_currentUser.IsInRole(AppRoles.Admin)) return true;
        if (!int.TryParse(_currentUser.UserId, out var userId)) return false;
        var employee = await _uow.Employees.FirstOrDefaultAsync(e => e.UserId == userId && e.IsActive && e.DeletedAt == null, ct: ct);
        return employee is not null && await _uow.Orders.GetStorePharmacyIdAsync(order.StoreId, ct) == employee.PharmacyId;
    }

    private static PaymentAttemptDto Map(PaymentAttempt p) => new()
    { Id = p.Id, OrderId = p.OrderId, Provider = p.Provider, ExternalReference = p.ExternalReference, Amount = p.Amount, Status = p.Status, FailureReason = p.FailureReason, CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt };
    private static PaymentRefundDto Map(PaymentRefund r) => new()
    { Id = r.Id, PaymentAttemptId = r.PaymentAttemptId, Amount = r.Amount, Status = r.Status, ExternalReference = r.ExternalReference, Reason = r.Reason, RequestedByUserId = r.RequestedByUserId, ProcessedByUserId = r.ProcessedByUserId, CreatedAt = r.CreatedAt, ProcessedAt = r.ProcessedAt };
}
