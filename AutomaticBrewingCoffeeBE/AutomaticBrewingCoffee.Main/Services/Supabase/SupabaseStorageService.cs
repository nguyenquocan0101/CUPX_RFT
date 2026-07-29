using Microsoft.Extensions.Options;
using Services.Supabase.Base;
using Supabase.Gotrue;
using Supabase.Interfaces;
using Supabase.Realtime;
using Supabase.Storage;
using FileOptions = Supabase.Storage.FileOptions;

namespace Services.Supabase;

public class SupabaseStorageService : ISupabaseStorageService
{
    private readonly ISupabaseClient<User, Session, RealtimeSocket, RealtimeChannel, Bucket, FileObject>
        _supabaseClient;

    private readonly SupabaseConfigure _supabaseConfigure;

    public SupabaseStorageService(
        ISupabaseClient<User, Session, RealtimeSocket, RealtimeChannel, Bucket, FileObject> supabaseClient,
        IOptions<SupabaseConfigure> options)
    {
        _supabaseClient = supabaseClient;
        _supabaseConfigure = options.Value;
    }

    public async Task<string> UploadFile(byte[] fileByte, string filePath, string bucketName, bool replace)
    {
        return await _supabaseClient.Storage.From(bucketName)
            .Upload
            (
                fileByte,
                filePath,
                new FileOptions()
                {
                    Upsert = replace,
                    CacheControl = "3600",
                    ContentType = "image",
                }
            );
    }

    public string RetrievePublicUrl(string bucketName, string filePath)
    {
        return _supabaseClient.Storage.From(bucketName).GetPublicUrl(filePath);
    }

    public async Task<byte[]> DownloadFile(string bucketName, string pathOrUrl)
    {
        var filePath = pathOrUrl;


        var publicPrefix = $"/object/public/{bucketName}/";

        if (filePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(filePath);

            var idx = uri.AbsolutePath.IndexOf(publicPrefix, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                filePath = uri.AbsolutePath.Substring(idx + publicPrefix.Length);
            }
            else
            {
                throw new ArgumentException("URL không hợp lệ hoặc không thuộc bucket đã chọn.");
            }
        }

        return await _supabaseClient.Storage.From(bucketName).Download(filePath, null);
    }

    public bool IsSupabaseResource(string imageUrl)
    {
        return imageUrl.Contains(_supabaseConfigure.Url);
    }
}