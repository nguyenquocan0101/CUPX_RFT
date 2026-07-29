using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kiosk.ApiService.CustomAttributes
{
    public class EnumAttribute : ValidationAttribute
    {
        private readonly Type _enumType;

        public EnumAttribute(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException("T must be an enum type");
            _enumType = enumType;
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null && !Enum.IsDefined(_enumType, value))
            {
                return new ValidationResult($"Invalid value for {validationContext.DisplayName}");
            }
            return ValidationResult.Success;
        }
    }
}
