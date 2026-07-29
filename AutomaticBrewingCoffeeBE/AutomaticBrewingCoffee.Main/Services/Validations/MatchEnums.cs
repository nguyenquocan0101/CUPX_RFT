using System.ComponentModel.DataAnnotations;

namespace Services.Validations;

public class MatchEnumsAttribute : ValidationAttribute
{
    private readonly Type _enumType;

    public MatchEnumsAttribute(Type enumType)
    {
        if (!enumType.IsEnum)
            throw new ArgumentException("Type must be an enum.");

        _enumType = enumType;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not IEnumerable<string> list)
            return new ValidationResult("Value must be a list of strings.");

        foreach (var item in list)
        {
            if (!Enum.IsDefined(_enumType, item))
            {
                return new ValidationResult($"Value '{item}' is not valid for enum {_enumType.Name}.");
            }
        }

        return ValidationResult.Success;
    }
}