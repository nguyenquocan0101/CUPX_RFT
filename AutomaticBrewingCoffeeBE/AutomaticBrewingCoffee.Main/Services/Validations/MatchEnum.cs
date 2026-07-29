using System.ComponentModel.DataAnnotations;

namespace Services.Validations;

public class MatchEnum : ValidationAttribute
{
    private readonly Type _enumType;

    public MatchEnum(Type enumType)
    {
        _enumType = enumType;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (!_enumType.IsEnum)
            throw new ArgumentException("Provided type is not an enum.");

        string stringValue = value.ToString();

        bool isValid = Enum.IsDefined(_enumType, stringValue);
        if (isValid)
            return ValidationResult.Success;

        return new ValidationResult($"'{stringValue}' is not a valid value for {_enumType.Name}.");
    }
}