using Services.Supabase;

namespace Services.Local;

public sealed class DisabledSupabaseStorageService : ISupabaseStorageService
{
    public Task<string> UploadFile(byte[] fileByte, string filePath, string bucketName, bool replace)
    {
        throw CreateDisabledException();
    }

    public string RetrievePublicUrl(string bucketName, string filePath)
    {
        throw CreateDisabledException();
    }

    public Task<byte[]> DownloadFile(string bucketName, string pathOrUrl)
    {
        throw CreateDisabledException();
    }

    public bool IsSupabaseResource(string imageUrl)
    {
        return false;
    }

    private static InvalidOperationException CreateDisabledException()
    {
        return new InvalidOperationException(
            "Supabase storage is disabled in local mode. Local object storage is configured separately.");
    }
}
