using FluentValidation;
using Shared.Models.Dtos.Banners;

namespace Service.Validators;

public sealed class BannerUpdateDtoValidator : AbstractValidator<BannerUpdateDto>
{
    public BannerUpdateDtoValidator()
    {
        Include(new BannerCreateDtoValidatorAdapter());
    }

    private sealed class BannerCreateDtoValidatorAdapter : AbstractValidator<BannerUpdateDto>
    {
        public BannerCreateDtoValidatorAdapter()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Placement).IsInEnum();
            RuleFor(x => x.ActionType).IsInEnum();
            RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
            RuleFor(x => x.AltText).MaximumLength(200);
            RuleFor(x => x.EndsAt).Must((dto, endsAt) => !endsAt.HasValue || !dto.StartsAt.HasValue || endsAt > dto.StartsAt)
                .WithMessage("EndsAt must be later than StartsAt.");
            RuleFor(x => x.TargetId).Must((dto, value) => dto.ActionType switch
            {
                Shared.Enums.BannerActionType.None or Shared.Enums.BannerActionType.ExternalUrl => !value.HasValue,
                Shared.Enums.BannerActionType.Product or Shared.Enums.BannerActionType.Offer or Shared.Enums.BannerActionType.Category or Shared.Enums.BannerActionType.Brand => value is > 0,
                _ => false
            }).WithMessage("TargetId is required and must be positive for this ActionType, and omitted for None/ExternalUrl.");
            RuleFor(x => x.ParentTargetId).Must((dto, value) => dto.ActionType == Shared.Enums.BannerActionType.Category
                ? (!value.HasValue || value > 0) : !value.HasValue)
                .WithMessage("ParentTargetId is only allowed for Category.");
            RuleFor(x => x.ActionUrl).Must((dto, value) => dto.ActionType == Shared.Enums.BannerActionType.ExternalUrl
                ? BannerCreateDtoValidator.BeHttpsUrl(value) : string.IsNullOrWhiteSpace(value))
                .WithMessage("ActionUrl is required as an absolute HTTPS URL only for ExternalUrl.");
        }
    }
}
