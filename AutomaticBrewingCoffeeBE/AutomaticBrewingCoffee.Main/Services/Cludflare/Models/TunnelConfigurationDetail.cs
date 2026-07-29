
using System.Text.Json.Serialization;
namespace Services.Cludflare.Models
{
    public class TunnelConfigurationDetail
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("account_tag")]
        public string AccountTag { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("connections")]
        public List<Connection> Connections { get; set; }

        [JsonPropertyName("conns_active_at")]
        public DateTime? ConnsActiveAt { get; set; }

        [JsonPropertyName("conns_inactive_at")]
        public DateTime? ConnsInactiveAt { get; set; }

        [JsonPropertyName("tun_type")]
        public string TunType { get; set; }
        /* TunType
         "cfd_tunnel"
        "warp_connector"
        "warp"
        "magic"
        "ip_sec"
        "gre"
        "cni"
         */

        [JsonPropertyName("metadata")]
        public Dictionary<string, object> Metadata { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
        /* Status
            "inactive"
            "degraded"
            "healthy"
            "down"
         */

        [JsonPropertyName("remote_config")]
        public bool RemoteConfig { get; set; }

        [JsonPropertyName("credentials_file")]
        public CredentialsFile CredentialsFile { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }
    }

    public class CredentialsFile
    {
        [JsonPropertyName("AccountTag")]
        public string AccountTag { get; set; }

        [JsonPropertyName("TunnelID")]
        public string TunnelID { get; set; }

        [JsonPropertyName("TunnelName")]
        public string TunnelName { get; set; }

        [JsonPropertyName("TunnelSecret")]
        public string TunnelSecret { get; set; }
    }
    public class Connection
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }

        [JsonPropertyName("client_version")]
        public string ClientVersion { get; set; }

        [JsonPropertyName("colo_name")]
        public string ColoName { get; set; }

        [JsonPropertyName("is_pending_reconnect")]
        public string IsPendingReconnect { get; set; }

        [JsonPropertyName("opened_at")]
        public string OpenedAt { get; set; }

        [JsonPropertyName("origin_ip")]
        public string OriginIp { get; set; }
        [JsonPropertyName("uuid")]
        public string UUID { get; set; }


    }
}
