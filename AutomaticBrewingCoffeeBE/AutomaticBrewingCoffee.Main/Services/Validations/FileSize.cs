using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Services.Validations;

public class FileSize : ValidationAttribute
{
    private readonly int _maxFileSize;

    public FileSize(int maxFileSize)
    {
        _maxFileSize = maxFileSize;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var file = value as IFormFile;

        if (file != null)
        {
            if (file.Length > (_maxFileSize * 1024 * 1024))
            {
                return new ValidationResult($"Maximum allowed file size is {_maxFileSize} MB.");
            }
        }

        return ValidationResult.Success;
    }
}