using FluentValidation;
using Shared.Enums;
using Shared.Models.Dtos.Banners;

namespace Service.Validators;

public sealed class BannerCreateDtoValidator : AbstractValidator<BannerCreateDto>
{
    public BannerCreateDtoValidator()
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
            BannerActionType.None or BannerActionType.ExternalUrl => !value.HasValue,
            BannerActionType.Product or BannerActionType.Offer or BannerActionType.Category or BannerActionType.Brand => value is > 0,
            _ => false
        }).WithMessage("TargetId is required and must be positive for this ActionType, and omitted for None/ExternalUrl.");
        RuleFor(x => x.ParentTargetId).Must((dto, value) => dto.ActionType == BannerActionType.Category
            ? (!value.HasValue || value > 0) : !value.HasValue)
            .WithMessage("ParentTargetId is only allowed for Category.");
        RuleFor(x => x.ActionUrl).Must((dto, value) => dto.ActionType == BannerActionType.ExternalUrl
            ? BeHttpsUrl(value) : string.IsNullOrWhiteSpace(value))
            .WithMessage("ActionUrl is required as an absolute HTTPS URL only for ExternalUrl.");
    }

    internal static bool BeHttpsUrl(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri)
           && uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
           && !string.IsNullOrWhiteSpace(uri.Host);
}
