using FluentValidation;
using Shared.Models.Dtos.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Validators;


public sealed class ReceiveStockDtoValidator : AbstractValidator<ReceiveStockDto>
{
    public ReceiveStockDtoValidator()
    {
        RuleFor(x => x.StoreId)
            .GreaterThan(0);

        RuleFor(x => x.ProductUnitId)
            .GreaterThan(0);

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .LessThanOrEqualTo(1_000_000);

        RuleFor(x => x.BatchNumber)
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.CostPrice.HasValue);

        RuleFor(x => x.Reason)
            .MaximumLength(500);

        // ExpirationDate:
        // - لا نجبرها هنا لأن هذا يعتمد على product.HasExpiry (DB)
        // - لكن لو موجودة نتأكد أنها ليست في الماضي
        RuleFor(x => x.ExpirationDate)
            .Must(d => d == null || d.Value.Date >= DateTime.UtcNow.Date)
            .WithMessage("ExpirationDate cannot be in the past.");
    }
}