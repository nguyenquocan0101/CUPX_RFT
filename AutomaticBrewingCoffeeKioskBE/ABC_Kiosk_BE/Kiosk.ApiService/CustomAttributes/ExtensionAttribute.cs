using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kiosk.ApiService.CustomAttributes
{
    public class ExtensionAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public ExtensionAttribute(params string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is List<IFormFile> files)
                foreach (var file in files)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (!_extensions.Contains(extension))
                    {
                        return new ValidationResult(
                            $"File {file.FileName} has an invalid extension. Allowed extensions are: {string.Join(", ", _extensions)}.");
                    }
                }

            return ValidationResult.Success;
        }
    }
}
