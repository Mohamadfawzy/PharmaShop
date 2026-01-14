using FluentValidation;
using Shared.Models.Dtos.Product.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Validators;

public sealed class ProductUnitCreateDtoValidator : AbstractValidator<ProductUnitCreateDto>
{
    public ProductUnitCreateDtoValidator()
    {
        RuleFor(x => x.UnitId)
            .GreaterThan(0);

        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .Length(3);

        RuleFor(x => x.ListPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.CostPrice.HasValue);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0);

        // Hierarchy rules
        RuleFor(x => x.UnitsPerParent)
            .NotNull()
            .GreaterThan(0)
            .When(x => x.ParentProductUnitId.HasValue)
            .WithMessage("UnitsPerParent is required and must be > 0 when ParentProductUnitId is provided.");

        RuleFor(x => x.UnitsPerParent)
            .Null()
            .When(x => !x.ParentProductUnitId.HasValue);

        // Base content rules
        RuleFor(x => x.BaseQuantity)
            .NotNull()
            .GreaterThan(0)
            .When(x => x.BaseUnitId.HasValue)
            .WithMessage("BaseQuantity is required and must be > 0 when BaseUnitId is provided.");

        RuleFor(x => x.BaseQuantity)
            .Null()
            .When(x => !x.BaseUnitId.HasValue);

        RuleFor(x => x.UnitCode)
            .MaximumLength(50);

        RuleFor(x => x.SKU)
            .MaximumLength(50);

        RuleFor(x => x.Reason)
            .MaximumLength(500);
    }
}
