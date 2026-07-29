using Services.Redis;

namespace Services.Utils;

public static class OrderCodeHelper
{
    // Múi giờ VN
    private static readonly TimeZoneInfo VnTz =
        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    /// <summary>
    /// Lấy số thứ tự trong ngày theo kiosk (atomic INCR + EXPIRE).
    /// Key: order:seq:{ORG}:{STORE}:{KIOSK}:{yyyyMMdd} (giờ VN)
    /// </summary>
    public static async Task<long> NextSequenceAsync(
        IRedisService redis,
        string orgCode,
        string storeCode,
        string kioskId,
        DateTime? nowUtc = null)
    {
        if (redis is null) throw new ArgumentNullException(nameof(redis));

        var nowVn = TimeZoneInfo.ConvertTimeFromUtc(nowUtc ?? DateTime.UtcNow, VnTz);

        var oc = StringHelper.Sanitize(orgCode);
        var sc = StringHelper.Sanitize(storeCode);
        var kc = StringHelper.Sanitize(kioskId);

        var key = $"order:seq:{oc}:{sc}:{kc}:{nowVn:yyyyMMdd}";

        var endOfDay = new DateTime(nowVn.Year, nowVn.Month, nowVn.Day, 23, 59, 59);
        var ttl = endOfDay - nowVn;
        if (ttl.TotalSeconds <= 0) ttl = TimeSpan.FromSeconds(1);

        return await redis.IncrWithExpireAsync(key, ttl); // trả về 1,2,3,...
    }

    /// <summary>
    /// Sinh mã đơn dạng: ORD-ORG-STORE-KIOSK-YYYYMMDD-000123 (giờ VN)
    /// </summary>
    public static async Task<string> GenerateOrderCodeAsync(
        IRedisService redis,
        string orgCode,
        string storeCode,
        string kioskId,
        DateTime? nowUtc = null)
    {
        var nowVn = TimeZoneInfo.ConvertTimeFromUtc(nowUtc ?? DateTime.UtcNow, VnTz);

        try
        {
            var seq = await NextSequenceAsync(redis, orgCode, storeCode, kioskId, nowUtc);
            return
                $"{nowVn:yyMMdd}{StringHelper.Sanitize(orgCode)}{seq:0000}{GuidUtil.ShortenGuid(Guid.NewGuid())}";
        }
        catch (Exception e)
        {
            return
                $"{nowVn:yyMMdd}{StringHelper.Sanitize(orgCode)}{GuidUtil.ShortenGuid(Guid.NewGuid())}";
        }
    }
}