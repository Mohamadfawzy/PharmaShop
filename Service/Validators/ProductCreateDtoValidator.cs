using FluentValidation;
using Shared.Models.Dtos.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Validators;


public sealed class ProductCreateDtoValidator : AbstractValidator<ProductCreateDto>
{
    public ProductCreateDtoValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(250).WithMessage("Name must be at most 250 characters");

        RuleFor(x => x.NameEn)
            .NotEmpty().WithMessage("NameEn is required")
            .MaximumLength(250).WithMessage("NameEn must be at most 250 characters");

        RuleFor(x => x.VatRate)
            .InclusiveBetween(0m, 100m).WithMessage("VatRate must be between 0 and 100");

        // CK_Products_Age
        When(x => !x.AgeRestricted, () =>
        {
            RuleFor(x => x.MinAge)
                .Null().WithMessage("MinAge must be null when AgeRestricted is false");
        });

        When(x => x.AgeRestricted, () =>
        {
            RuleFor(x => x.MinAge)
                .NotNull().WithMessage("MinAge is required when AgeRestricted is true")
                .GreaterThanOrEqualTo(0).WithMessage("MinAge must be >= 0");
        });

        // CK_Products_OrderLimits
        RuleFor(x => x.MinOrderQty)
            .GreaterThan(0).WithMessage("MinOrderQty must be > 0");

        RuleFor(x => x.MaxOrderQty)
            .GreaterThanOrEqualTo(x => x.MinOrderQty)
            .When(x => x.MaxOrderQty.HasValue)
            .WithMessage("MaxOrderQty must be >= MinOrderQty");

        RuleFor(x => x.MaxPerDayQty)
            .GreaterThan(0)
            .When(x => x.MaxPerDayQty.HasValue)
            .WithMessage("MaxPerDayQty must be > 0");

        // CK_Products_ReturnWindow
        When(x => !x.IsReturnable, () =>
        {
            RuleFor(x => x.ReturnWindowDays)
                .Null().WithMessage("ReturnWindowDays must be null when IsReturnable is false");
        });

        When(x => x.IsReturnable, () =>
        {
            RuleFor(x => x.ReturnWindowDays)
                .GreaterThan(0)
                .When(x => x.ReturnWindowDays.HasValue)
                .WithMessage("ReturnWindowDays must be > 0 when provided");
        });

        // CK_Products_SplitRules
        When(x => !x.AllowSplitSale, () =>
        {
            RuleFor(x => x.SplitLevel)
                .Null().WithMessage("SplitLevel must be null when AllowSplitSale is false");
        });

        When(x => x.AllowSplitSale, () =>
        {
            RuleFor(x => x.SplitLevel)
                .NotNull().WithMessage("SplitLevel is required when AllowSplitSale is true")
                .Must(sl => sl is 1 or 2).WithMessage("SplitLevel must be 1 or 2 when AllowSplitSale is true");
        });

        // CK_Products_Dimensions
        RuleFor(x => x.WeightGrams)
            .GreaterThanOrEqualTo(0)
            .When(x => x.WeightGrams.HasValue)
            .WithMessage("WeightGrams must be >= 0");

        RuleFor(x => x.LengthMm)
            .GreaterThanOrEqualTo(0)
            .When(x => x.LengthMm.HasValue)
            .WithMessage("LengthMm must be >= 0");

        RuleFor(x => x.WidthMm)
            .GreaterThanOrEqualTo(0)
            .When(x => x.WidthMm.HasValue)
            .WithMessage("WidthMm must be >= 0");

        RuleFor(x => x.HeightMm)
            .GreaterThanOrEqualTo(0)
            .When(x => x.HeightMm.HasValue)
            .WithMessage("HeightMm must be >= 0");
    }
}