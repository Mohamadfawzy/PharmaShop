using FluentValidation;
using Shared.Models.Dtos.Promotion;

namespace Service.Validators;

public sealed class PromotionUpdateDtoValidator : AbstractValidator<PromotionUpdateDto>
{
    public PromotionUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Name must be at most 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Notes)
            .MaximumLength(250).WithMessage("Notes must be at most 250 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));

        RuleFor(x => x.DiscountPercent)
            .InclusiveBetween(0, 100).WithMessage("DiscountPercent must be between 0 and 100")
            .When(x => x.DiscountPercent.HasValue);

        RuleFor(x => x)
            .Must(x =>
                (x.StartAt is null && x.EndAt is null) ||
                (x.StartAt is not null && x.EndAt is not null && x.EndAt > x.StartAt)
            )
            .WithMessage("StartAt and EndAt must be both null or both provided, and EndAt must be greater than StartAt");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("TotalAmount must be > 0")
            .When(x => x.TotalAmount.HasValue);

        RuleFor(x => x.BasicAmount)
            .GreaterThan(0).WithMessage("BasicAmount must be > 0")
            .When(x => x.BasicAmount.HasValue);

        RuleFor(x => x.OfferAmount)
            .GreaterThan(0).WithMessage("OfferAmount must be > 0")
            .When(x => x.OfferAmount.HasValue);

        RuleFor(x => x)
            .Must(x =>
            {
                if (!x.TotalAmount.HasValue || !x.BasicAmount.HasValue || !x.OfferAmount.HasValue)
                    return true;

                return x.TotalAmount.Value >= (x.BasicAmount.Value + x.OfferAmount.Value);
            })
            .WithMessage("TotalAmount must be >= BasicAmount + OfferAmount")
            .When(x => x.TotalAmount.HasValue && x.BasicAmount.HasValue && x.OfferAmount.HasValue);
    }
}
