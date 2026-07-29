using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Services.Validations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class PhoneVNAttribute : ValidationAttribute
{
    private const string Pattern = @"^(?:\+84|0)(3[2-9]|5[689]|7[06-9]|8[1-689]|9[0-46-9])[0-9]{7}$";

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success; // Cho phép null, nếu muốn bắt buộc thì thêm [Required]
        }

        var phone = value.ToString();
        if (string.IsNullOrWhiteSpace(phone))
        {
            return ValidationResult.Success;
        }

        if (Regex.IsMatch(phone, Pattern))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} không phải là số điện thoại hợp lệ.");
    }
}