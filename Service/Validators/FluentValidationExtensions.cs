using FluentValidation.Results;
namespace Service.Validators;
internal static class FluentValidationExtensions
{
    internal static Dictionary<string, string[]> ToFieldErrorsDictionary(this ValidationResult result)
    {
        return result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).Distinct().ToArray(),
                StringComparer.OrdinalIgnoreCase);
    }
}