using Microsoft.AspNetCore.Http;
using MimeMapping;

namespace AutomaticBrewingCoffee.Services.Utils;

public static class FileHelper
{
    public static async Task<byte[]> ToByteArrayAsync(IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    public static string GetExtension(IFormFile file)
    {
        return Path.GetExtension(file.FileName);
    }

    public static string? GetFileExtensionFromBase64(string base64String)
    {
        if (string.IsNullOrEmpty(base64String) || !base64String.Contains(";base64,"))
            return null;

        // Lấy phần MIME type trước `;base64`
        var mimeType = base64String.Split(';')[0].Split(':')[1];

        // Ánh xạ MIME type sang file extension
        return mimeType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/bmp" => ".bmp",
            "image/svg+xml" => ".svg",
            "application/pdf" => ".pdf",
            "application/msword" => ".doc",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
            "application/vnd.ms-excel" => ".xls",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => ".xlsx",
            "application/vnd.ms-powerpoint" => ".ppt",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation" => ".pptx",
            _ => null 
        };
    }
}