using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Validations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class GreaterThanAttribute : ValidationAttribute
    {
        private readonly double _minValue;

        public GreaterThanAttribute(double minValue)
        {
            _minValue = minValue;
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return true;

            try
            {
                var convertedValue = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                return convertedValue > _minValue;
            }
            catch
            {
                return false;
            }
        }

        public override string FormatErrorMessage(string name)
        {
            return string.IsNullOrEmpty(ErrorMessage)
                ? $"{name} must greater than {_minValue}."
                : base.FormatErrorMessage(name);
        }
    }
}
