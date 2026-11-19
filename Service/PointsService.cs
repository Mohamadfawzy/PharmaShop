using Contracts;
using Contracts.IServices;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Dtos.Points;
using Shared.Responses;

namespace Service;

public sealed class PointsService : IPointsService
{
    private const int ConversionRate = 30;
    private const int ExpiryBatchSize = 500;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;

    public PointsService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, INotificationService notifications)
    { _unitOfWork = unitOfWork; _currentUser = currentUser; _notifications = notifications; }

    public async Task<AppResponse<PointsBalanceDto>> GetMyBalanceAsync(CancellationToken ct)
    {
        var customer = await FindCustomerAsync(ct);
        if (customer is null) return AppResponse<PointsBalanceDto>.NotFound("Active customer profile not found");
        var balance = await _unitOfWork.Points.GetBalanceAsync(customer.Id, DateTime.UtcNow, ct);
        return balance is null ? AppResponse<PointsBalanceDto>.NotFound("Customer points account not found")
            : AppResponse<PointsBalanceDto>.Ok(balance);
    }

    public async Task<AppResponse<List<CustomerPointLotDto>>> GetMyLotsAsync(PointsListQueryDto query, CancellationToken ct)
    {
        var customer = await FindCustomerAsync(ct);
        if (customer is null) return AppResponse<List<CustomerPointLotDto>>.NotFound("Active customer profile not found");
        if (query is null) return AppResponse<List<CustomerPointLotDto>>.ValidationError("Query is required");
        if (query.Status.HasValue && (query.Status < 1 || query.Status > 5))
            return AppResponse<List<CustomerPointLotDto>>.ValidationError("Status must be between 1 and 5");
        NormalizePage(query);
        var result = await _unitOfWork.Points.SearchLotsAsync(customer.Id, query.Status, query.Page, query.PageSize, ct);
        return AppResponse<List<CustomerPointLotDto>>.Ok(result.Items,
            PaginationInfo.Create(query.Page, query.PageSize, result.TotalCount));
    }

    public async Task<AppResponse<List<CustomerPointTransactionDto>>> GetMyTransactionsAsync(PointsListQueryDto query, CancellationToken ct)
    {
        var customer = await FindCustomerAsync(ct);
        if (customer is null) return AppResponse<List<CustomerPointTransactionDto>>.NotFound("Active customer profile not found");
        if (query is null) return AppResponse<List<CustomerPointTransactionDto>>.ValidationError("Query is required");
        if (query.TransactionType.HasValue && (query.TransactionType < 1 || query.TransactionType > 8))
            return AppResponse<List<CustomerPointTransactionDto>>.ValidationError("TransactionType must be between 1 and 8");
        if (query.From.HasValue && query.To.HasValue && query.From > query.To)
            return AppResponse<List<CustomerPointTransactionDto>>.ValidationError("From must be earlier than To");
        NormalizePage(query);
        var result = await _unitOfWork.Points.SearchTransactionsAsync(customer.Id, query, ct);
        return AppResponse<List<CustomerPointTransactionDto>>.Ok(result.Items,
            PaginationInfo.Create(query.Page, query.PageSize, result.TotalCount));
    }

    public async Task<AppResponse<List<RedeemablePointsProductDto>>> GetRedeemableProductsAsync(int? storeId, int page, int pageSize, CancellationToken ct)
    {
        if (storeId.HasValue && storeId.Value <= 0) return AppResponse<List<RedeemablePointsProductDto>>.ValidationError("StoreId must be greater than zero");
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await _unitOfWork.Points.SearchRedeemableProductsAsync(storeId, page, pageSize, ct);
        return AppResponse<List<RedeemablePointsProductDto>>.Ok(result.Items,
            PaginationInfo.Create(page, pageSize, result.TotalCount));
    }

    public async Task<AppResponse<ManualPointsAdjustmentResultDto>> AdjustCustomerPointsAsync(int customerId, ManualPointsAdjustmentDto dto, CancellationToken ct)
    {
        if (customerId <= 0) return AppResponse<ManualPointsAdjustmentResultDto>.ValidationError("Invalid customer id");
        if (dto is null || dto.PointsDelta == 0 || dto.PointsDelta == int.MinValue)
            return AppResponse<ManualPointsAdjustmentResultDto>.ValidationError("PointsDelta must be a non-zero integer");
        if (dto.ReferenceId <= 0) return AppResponse<ManualPointsAdjustmentResultDto>.ValidationError("ReferenceId must be a unique positive integer");
        if (string.IsNullOrWhiteSpace(dto.Reason) || dto.Reason.Trim().Length > 500)
            return AppResponse<ManualPointsAdjustmentResultDto>.ValidationError("Reason is required and cannot exceed 500 characters");

        var employee = await FindEmployeeAsync(ct);
        if (employee is null) return AppResponse<ManualPointsAdjustmentResultDto>.Forbidden("An active employee profile is required for point adjustments");
        await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);
        if (await _unitOfWork.Points.AdjustmentReferenceExistsAsync(dto.ReferenceId, ct))
            return AppResponse<ManualPointsAdjustmentResultDto>.Conflict("ReferenceId has already been used for a points adjustment");
        var customer = await _unitOfWork.Customers.FirstOrDefaultAsync(c => c.Id == customerId && c.IsActive, asNoTracking: false, ct: ct);
        if (customer is null) return AppResponse<ManualPointsAdjustmentResultDto>.NotFound("Active customer not found");

        var now = DateTime.UtcNow;
        var delta = dto.PointsDelta;
        CustomerPointLot? grantedLot = null;
        var lots = await _unitOfWork.Points.GetSpendableLotsForUpdateAsync(customerId, now, ct);
        var activeBalance = lots.Sum(l => l.RemainingPoints);
        if (delta > 0)
        {
            grantedLot = new CustomerPointLot
            {
                CustomerId = customer.Id, PointsTotal = delta, RemainingPoints = delta, Status = 2,
                EarnedAt = now, AvailableAt = now, ExpiresAt = now.AddYears(1), CreatedAt = now
            };
            await _unitOfWork.Points.AddLotAsync(grantedLot, ct);
        }
        else
        {
            var amount = -delta;
            if (customer.Points < amount)
                return AppResponse<ManualPointsAdjustmentResultDto>.BusinessRuleViolation("Customer does not have enough available points for this adjustment");
            if (activeBalance < amount)
                return AppResponse<ManualPointsAdjustmentResultDto>.BusinessRuleViolation("Available point lots do not cover the requested adjustment");
            var remaining = amount;
            foreach (var lot in lots)
            {
                if (remaining == 0) break;
                var take = Math.Min(lot.RemainingPoints, remaining);
                lot.RemainingPoints -= take;
                lot.UpdatedAt = now;
                if (lot.RemainingPoints == 0) lot.Status = 3;
                remaining -= take;
            }
        }

        customer.Points += delta;
        customer.UpdatedAt = now;
        var transactionRow = new CustomerPointTransaction
        {
            CustomerId = customerId, Lot = grantedLot, TransactionType = 7, PointsDelta = delta,
            BalanceAfter = activeBalance + delta, ReferenceType = 3, ReferenceId = dto.ReferenceId,
            CreatedByEmployeeId = employee.Id, Notes = dto.Reason.Trim(), CreatedAt = now
        };
        await _unitOfWork.Points.AddTransactionsAsync([transactionRow], ct);
        await _unitOfWork.CompleteAsync(ct);
        await transaction.CommitAsync(ct);
        await _notifications.NotifyCustomerAsync(customerId, "Points", "تحديث رصيد النقاط",
            delta > 0 ? $"تمت إضافة {delta} نقطة إلى رصيدك." : $"تم خصم {Math.Abs(delta)} نقطة من رصيدك.", "Points", transactionRow.Id, ct);
        return AppResponse<ManualPointsAdjustmentResultDto>.Created(new ManualPointsAdjustmentResultDto
        {
            CustomerId = customerId, PointsDelta = delta, BalanceAfter = activeBalance + delta,
            TransactionId = transactionRow.Id, LotId = grantedLot?.Id
        }, $"/api/v1/points/transactions/{transactionRow.Id}", "Points adjusted successfully");
    }

    public async Task<int> ExpireDuePointsAsync(CancellationToken ct)
    {
        var totalExpired = 0;
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);
            var now = DateTime.UtcNow;
            var lots = await _unitOfWork.Points.GetExpiredLotsForUpdateAsync(now, ExpiryBatchSize, ct);
            if (lots.Count == 0) { await transaction.CommitAsync(ct); break; }
            var customerIds = lots.Select(l => l.CustomerId).Distinct().ToList();
            var customers = await _unitOfWork.Customers.FindAsync(c => customerIds.Contains(c.Id), asNoTracking: false, ct: ct);
            var byId = customers.ToDictionary(c => c.Id);
            var expirationByCustomer = lots.GroupBy(l => l.CustomerId).ToDictionary(g => g.Key, g => g.Sum(l => l.RemainingPoints));
            if (expirationByCustomer.Any(x => !byId.ContainsKey(x.Key) || byId[x.Key].Points < x.Value))
                throw new InvalidOperationException("Customer point balance does not match expiring point lots");

            var runningBalances = customers.ToDictionary(c => c.Id, c => c.Points);
            var rows = new List<CustomerPointTransaction>(lots.Count);
            foreach (var lot in lots)
            {
                var points = lot.RemainingPoints;
                lot.RemainingPoints = 0;
                lot.Status = 4;
                lot.UpdatedAt = now;
                var balance = runningBalances[lot.CustomerId] - points;
                runningBalances[lot.CustomerId] = balance;
                rows.Add(new CustomerPointTransaction
                {
                    CustomerId = lot.CustomerId, LotId = lot.Id, TransactionType = 5,
                    PointsDelta = -points, BalanceAfter = balance, ExpiresAt = null,
                    ReferenceType = 4, ReferenceId = lot.Id,
                    Notes = "Point lot expired", CreatedAt = now
                });
            }
            foreach (var customer in customers)
            { customer.Points = runningBalances[customer.Id]; customer.UpdatedAt = now; }
            await _unitOfWork.Points.AddTransactionsAsync(rows, ct);
            await _unitOfWork.CompleteAsync(ct);
            await transaction.CommitAsync(ct);
            totalExpired += lots.Count;
        }
        return totalExpired;
    }

    public async Task RestoreRedeemedPointsAsync(Order order, DateTime refundedAtUtc, CancellationToken ct)
    {
        if (order.RedeemedPoints <= 0) return;
        var customer = await _unitOfWork.Customers.FirstOrDefaultAsync(c => c.Id == order.CustomerId, asNoTracking: false, ct: ct)
            ?? throw new InvalidOperationException("Order customer not found while restoring redeemed points");
        var redemptions = await _unitOfWork.Points.GetOrderRedemptionsAsync(order.Id, order.CustomerId, ct);
        var runningBalance = customer.Points;
        var rows = new List<CustomerPointTransaction>();
        foreach (var redemption in redemptions)
        {
            if (await _unitOfWork.Points.HasRefundForRedemptionAsync(redemption.Id, ct)) continue;
            var points = -redemption.PointsDelta;
            if (points <= 0) continue;
            var lot = new CustomerPointLot
            {
                CustomerId = order.CustomerId, OrderId = order.Id, OrderItemId = redemption.OrderItemId,
                PointsTotal = points, RemainingPoints = points, Status = 2, EarnedAt = refundedAtUtc,
                AvailableAt = refundedAtUtc, ExpiresAt = refundedAtUtc.AddDays(90), CreatedAt = refundedAtUtc
            };
            await _unitOfWork.Points.AddLotAsync(lot, ct);
            customer.Points += points;
            runningBalance += points;
            rows.Add(new CustomerPointTransaction
            {
                CustomerId = order.CustomerId, Lot = lot, OrderId = order.Id, OrderItemId = redemption.OrderItemId,
                TransactionType = 4, PointsDelta = points, BalanceAfter = runningBalance,
                ReferenceType = 2, SourceTransactionId = redemption.Id,
                Notes = "Redeemed points refunded; expire 90 days after refund", CreatedAt = refundedAtUtc
            });
        }
        customer.UpdatedAt = refundedAtUtc;
        if (rows.Count > 0) await _unitOfWork.Points.AddTransactionsAsync(rows, ct);
    }

    public async Task<(int RedeemedRefunded, int EarnedReversed)> ApplyApprovedReturnAsync(
        Order order, ReturnRequest request, IReadOnlyList<ReturnRequestItem> priorApprovedItems, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var customer = await _unitOfWork.Customers.FirstOrDefaultAsync(c => c.Id == order.CustomerId, asNoTracking: false, ct: ct)
            ?? throw new InvalidOperationException("Return order customer not found");
        var totalLineValue = order.OrderItems.Sum(i => i.LineTotal);
        var priorReturnValue = priorApprovedItems.Sum(i => ReturnLineValue(i.OrderItem, i.Quantity));
        var currentReturnValue = request.Items.Sum(i => ReturnLineValue(i.OrderItem, i.Quantity));

        var redemptions = await _unitOfWork.Points.GetOrderRedemptionsAsync(order.Id, order.CustomerId, ct);
        var totalRedeemed = redemptions.Sum(t => -t.PointsDelta);
        var previouslyRefunded = await _unitOfWork.Points.GetReturnPointAdjustmentTotalAsync(order.Id, 8, ct);
        var targetRefunded = totalLineValue <= 0 ? 0 : Math.Min(totalRedeemed,
            (int)Math.Floor(totalRedeemed * (priorReturnValue + currentReturnValue) / totalLineValue));
        var redeemedRefund = Math.Max(0, targetRefunded - previouslyRefunded);

        var earnedLot = await _unitOfWork.Points.GetOrderEarnLotAsync(order.Id, ct);
        var earnedToReverse = 0;
        foreach (var orderItem in order.OrderItems)
        {
            if (orderItem.Quantity <= 0 || orderItem.EarnedPoints <= 0) continue;
            var priorQty = priorApprovedItems.Where(i => i.OrderItemId == orderItem.Id).Sum(i => i.Quantity);
            var thisQty = request.Items.Where(i => i.OrderItemId == orderItem.Id).Sum(i => i.Quantity);
            var target = (int)Math.Floor(orderItem.EarnedPoints * Math.Min(orderItem.Quantity, priorQty + thisQty) / orderItem.Quantity);
            var reversed = -await _unitOfWork.Points.GetEarnedPointsReversedAsync(orderItem.Id, ct);
            earnedToReverse += Math.Max(0, target - reversed);
        }

        var earnedReversed = Math.Min(earnedToReverse, earnedLot?.RemainingPoints ?? 0);
        if (earnedReversed > 0 && earnedLot is not null)
        {
            if (customer.Points < earnedReversed)
                throw new InvalidOperationException("Customer points balance is inconsistent with the available order points lot");
            earnedLot.RemainingPoints -= earnedReversed;
            earnedLot.Status = earnedLot.RemainingPoints == 0 ? (byte)3 : earnedLot.Status;
            earnedLot.UpdatedAt = now;
            customer.Points -= earnedReversed;
            await _unitOfWork.Points.AddTransactionsAsync([new CustomerPointTransaction
            {
                CustomerId = customer.Id, LotId = earnedLot.Id, OrderId = order.Id,
                TransactionType = 6, PointsDelta = -earnedReversed, BalanceAfter = customer.Points,
                ReferenceType = 5, ReferenceId = request.Id,
                Notes = $"Earned points reversed for approved return #{request.Id}", CreatedAt = now
            }], ct);
        }

        if (redeemedRefund > 0)
        {
            var expiry = now.AddDays(90);
            var lot = new CustomerPointLot
            {
                CustomerId = customer.Id, OrderId = order.Id, PointsTotal = redeemedRefund,
                RemainingPoints = redeemedRefund, Status = 2, EarnedAt = now, AvailableAt = now,
                ExpiresAt = expiry, CreatedAt = now
            };
            await _unitOfWork.Points.AddLotAsync(lot, ct);
            customer.Points += redeemedRefund;
            await _unitOfWork.Points.AddTransactionsAsync([new CustomerPointTransaction
            {
                CustomerId = customer.Id, Lot = lot, OrderId = order.Id, TransactionType = 8,
                PointsDelta = redeemedRefund, BalanceAfter = customer.Points, PointsPerEGP = ConversionRate,
                AmountEGP = Math.Round(redeemedRefund / (decimal)ConversionRate, 2), ExpiresAt = expiry,
                ReferenceType = 5, ReferenceId = request.Id,
                Notes = $"Redeemed points refunded for approved return #{request.Id}; expires 90 days after refund", CreatedAt = now
            }], ct);
        }

        customer.UpdatedAt = now;
        return (redeemedRefund, earnedReversed);
    }

    private static decimal ReturnLineValue(OrderItem item, decimal quantity)
        => item.Quantity <= 0 ? 0 : decimal.Round(item.LineTotal * quantity / item.Quantity, 2, MidpointRounding.AwayFromZero);

    private Task<Customer?> FindCustomerAsync(CancellationToken ct)
    {
        if (!int.TryParse(_currentUser.UserId, out var userId) || userId <= 0) return Task.FromResult<Customer?>(null);
        return _unitOfWork.Customers.FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive, ct: ct);
    }

    private Task<Employee?> FindEmployeeAsync(CancellationToken ct)
    {
        if (!int.TryParse(_currentUser.UserId, out var userId) || userId <= 0) return Task.FromResult<Employee?>(null);
        return _unitOfWork.Employees.FirstOrDefaultAsync(e => e.UserId == userId && e.IsActive && e.DeletedAt == null, ct: ct);
    }

    private static void NormalizePage(PointsListQueryDto query)
    { query.Page = Math.Max(1, query.Page); query.PageSize = Math.Clamp(query.PageSize, 1, 100); }
}
