using Services.Storage;

namespace Services.Tests.Local.Storage;

public class MinioObjectStorageServiceTests
{
    [Fact]
    public void RetrievePublicUrl_UsesPathStyleBucketAndExistingObjectPath()
    {
        var service = CreateService();

        var url = service.RetrievePublicUrl(
            "images",
            "drinks/product-1/product-1.png");

        Assert.Equal(
            "http://localhost:9000/images/drinks/product-1/product-1.png",
            url);
    }

    [Fact]
    public void IsObjectStorageResource_RecognizesConfiguredEndpointAndRejectsOtherHosts()
    {
        var service = CreateService();

        Assert.True(service.IsObjectStorageResource(
            "http://localhost:9000/images/drinks/product-1/product-1.png"));
        Assert.False(service.IsObjectStorageResource(
            "https://example.com/images/drinks/product-1/product-1.png"));
    }

    [Fact]
    public void MinioOptions_DefaultToPathStyle()
    {
        var options = new MinioOptions();

        Assert.True(options.UsePathStyle);
    }

    private static MinioObjectStorageService CreateService()
    {
        return new MinioObjectStorageService(new MinioOptions
        {
            Endpoint = "http://localhost:9000",
            AccessKey = "access-key",
            SecretKey = "secret-key",
            PublicEndpoint = "http://localhost:9000"
        });
    }
}
