using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Services.Validations
{
    public class MatchBase64 : ValidationAttribute
    {
        private static readonly Regex _base64Regex =
            new Regex(@"^data:image\/(png|jpg|jpeg|gif);base64,[A-Za-z0-9+/=]+\s*$", RegexOptions.Compiled);

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                // Cho phép null hoặc chuỗi rỗng (nếu muốn bắt buộc thì bỏ dòng này đi)
                return ValidationResult.Success;
            }

            string base64String = value.ToString()!;

            // Kiểm tra chuỗi có đúng format của base64 image hay không
            if (!_base64Regex.IsMatch(base64String))
            {
                return new ValidationResult("The provided string is not a valid Base64 encoded image.");
            }

            // Kiểm tra giải mã có lỗi không
            try
            {
                var base64Data = base64String.Split(',')[1];
                Convert.FromBase64String(base64Data);
            }
            catch (FormatException)
            {
                return new ValidationResult("The provided string is not a valid Base64 encoded image.");
            }

            return ValidationResult.Success;
        }
    }
}