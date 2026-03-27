using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Shared.Models.Dtos.Tag;

namespace Service.Validators;


public sealed class TagCreateDtoValidator : AbstractValidator<TagCreateDto>
{
    public TagCreateDtoValidator()
    {
        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(80).WithMessage("Name must be at most 80 characters");

        RuleFor(x => x.NameEn)
            .MaximumLength(80).WithMessage("NameEn must be at most 80 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.NameEn));
    }
}