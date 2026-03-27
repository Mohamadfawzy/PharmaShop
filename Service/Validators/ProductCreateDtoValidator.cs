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
        // -------- Required / basic --------
        RuleFor(x => x.StoreId)
            .GreaterThan(0).WithMessage("StoreId is required");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId is required");

        RuleFor(x => x.OuterUnitId)
            .GreaterThan(0).WithMessage("OuterUnitId is required");

        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage("NameAr is required")
            .MaximumLength(250).WithMessage("NameAr must be at most 250 characters");

        RuleFor(x => x.NameEn)
            .MaximumLength(250).WithMessage("NameEn must be at most 250 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.NameEn));

        RuleFor(x => x.InternationalCode)
            .MaximumLength(50).WithMessage("InternationalCode must be at most 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.InternationalCode));

        RuleFor(x => x.SearchKeywords)
            .MaximumLength(500).WithMessage("SearchKeywords must be at most 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.SearchKeywords));


        RuleFor(x => x.PromotionDiscountPercent)
           .GreaterThan(0).When(x => x.HasPromotion);


        // -------- Prices & quantities (matches DB CHECK constraints) --------
        RuleFor(x => x.OuterUnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("OuterUnitPrice must be non-negative");

        RuleFor(x => x.InnerUnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("InnerUnitPrice must be non-negative")
            .When(x => x.InnerUnitPrice.HasValue);

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity must be non-negative");

        // -------- Order limits (matches CK_Products_OrderLimits) --------
        RuleFor(x => x.MinOrderQty)
            .GreaterThan(0).WithMessage("MinOrderQty must be greater than 0");

        RuleFor(x => x.MaxOrderQty)
            .GreaterThanOrEqualTo(x => x.MinOrderQty)
            .WithMessage("MaxOrderQty must be greater than or equal to MinOrderQty")
            .When(x => x.MaxOrderQty.HasValue);

        RuleFor(x => x.MaxPerDayQty)
            .GreaterThan(0).WithMessage("MaxPerDayQty must be greater than 0")
            .When(x => x.MaxPerDayQty.HasValue);

        // -------- Inner rules (matches CK_Products_InnerRules) --------
        RuleFor(x => x)
            .Must(x =>
                (x.InnerUnitId is null && x.InnerPerOuter is null)
                || (x.InnerUnitId is not null && x.InnerPerOuter is not null && x.InnerPerOuter.Value >= 1)
            )
            .WithMessage("InnerUnitId and InnerPerOuter must be provided together, and InnerPerOuter must be >= 1");

        // -------- Promotion percent (matches CK_Products_PromotionPercent) --------
        RuleFor(x => x.PromotionDiscountPercent)
            .InclusiveBetween(0, 100).WithMessage("PromotionDiscountPercent must be between 0 and 100");

        // -------- Promotion dates (matches CK_Products_PromotionDates) --------
        RuleFor(x => x)
            .Must(x =>
                (x.PromotionStartsAt is null && x.PromotionEndsAt is null)
                || (x.PromotionStartsAt is not null && x.PromotionEndsAt is not null && x.PromotionEndsAt > x.PromotionStartsAt)
            )
            .WithMessage("PromotionStartsAt and PromotionEndsAt must be both null or both provided, and PromotionEndsAt must be greater than PromotionStartsAt");

        // -------- منطقيات إضافية مفيدة للـ API (اختيارية ولكن أنصح بها) --------

        // إذا HasPromotion = false الأفضل أن يكون الخصم 0 والتواريخ null
        RuleFor(x => x.PromotionDiscountPercent)
            .Equal(0).WithMessage("PromotionDiscountPercent must be 0 when HasPromotion is false")
            .When(x => x.HasPromotion == false);

        RuleFor(x => x.PromotionStartsAt)
            .Null().WithMessage("PromotionStartsAt must be null when HasPromotion is false")
            .When(x => x.HasPromotion == false);

        RuleFor(x => x.PromotionEndsAt)
            .Null().WithMessage("PromotionEndsAt must be null when HasPromotion is false")
            .When(x => x.HasPromotion == false);

        // إذا HasPromotion = true الأفضل أن تكون التواريخ موجودة (حتى لو DB يسمح بغير ذلك حسب قيودك)
        RuleFor(x => x.PromotionStartsAt)
            .NotNull().WithMessage("PromotionStartsAt is required when HasPromotion is true")
            .When(x => x.HasPromotion == true);

        RuleFor(x => x.PromotionEndsAt)
            .NotNull().WithMessage("PromotionEndsAt is required when HasPromotion is true")
            .When(x => x.HasPromotion == true);

        // Points
        RuleFor(x => x.Points)
            .GreaterThanOrEqualTo(0).WithMessage("Points must be non-negative");
    }
}