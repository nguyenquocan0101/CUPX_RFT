namespace Services.Supabase;

public interface ISupabaseStorageService
{
    Task<string> UploadFile(byte[] fileByte, string filePath, string bucketName, bool replace);
    string RetrievePublicUrl(string bucketName, string filePath);
    public Task<byte[]> DownloadFile(string bucketName, string pathOrUrl);
    public bool IsSupabaseResource(string imageUrl);
}