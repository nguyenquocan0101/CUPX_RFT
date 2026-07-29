using Microsoft.AspNetCore.Http;
using Services.Validations;

namespace Services.Dtos.Product;

public class UploadProductImageDto
{
    [FileSize(5)]
    [AllowedExtensions([".jpg", ".png", ".jpeg"])]
    public required IFormFile File { get; set; }
}