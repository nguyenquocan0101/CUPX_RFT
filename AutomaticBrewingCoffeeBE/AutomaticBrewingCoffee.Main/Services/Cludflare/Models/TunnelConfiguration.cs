using System.Text.Json.Serialization;

namespace Services.Cludflare.Models
{
    public class TunnelAccess
    {
        [JsonPropertyName("audTag")]
        public List<string> AudTag { get; set; }

        [JsonPropertyName("teamName")]
        public string TeamName { get; set; }

        [JsonPropertyName("required")]
        public bool Required { get; set; }
    }

    public class OriginRequest
    {
        [JsonPropertyName("access")]
        public TunnelAccess Access { get; set; }

        [JsonPropertyName("caPool")]
        public string CaPool { get; set; }

        [JsonPropertyName("connectTimeout")]
        public int ConnectTimeout { get; set; }

        [JsonPropertyName("disableChunkedEncoding")]
        public bool DisableChunkedEncoding { get; set; }

        [JsonPropertyName("http2Origin")]
        public bool Http2Origin { get; set; }

        [JsonPropertyName("httpHostHeader")]
        public string HttpHostHeader { get; set; }

        [JsonPropertyName("keepAliveConnections")]
        public int KeepAliveConnections { get; set; }

        [JsonPropertyName("keepAliveTimeout")]
        public int KeepAliveTimeout { get; set; }

        [JsonPropertyName("noHappyEyeballs")]
        public bool NoHappyEyeballs { get; set; }

        [JsonPropertyName("noTLSVerify")]
        public bool NoTLSVerify { get; set; }

        [JsonPropertyName("originServerName")]
        public string OriginServerName { get; set; }

        [JsonPropertyName("proxyType")]
        public string ProxyType { get; set; }

        [JsonPropertyName("tcpKeepAlive")]
        public int TcpKeepAlive { get; set; }

        [JsonPropertyName("tlsTimeout")]
        public int TlsTimeout { get; set; }
    }

    public class IngressRule
    {
        [JsonPropertyName("hostname")]
        public string Hostname { get; set; }

        [JsonPropertyName("service")]
        public string Service { get; set; }

        [JsonPropertyName("originRequest")]
        public OriginRequest OriginRequest { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }
    }

    public class WarpRouting
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
    }

    public class TunnelConfigData
    {
        [JsonPropertyName("ingress")]
        public List<IngressRule> Ingress { get; set; }

        [JsonPropertyName("warp-routing")]
        public WarpRouting WarpRouting { get; set; }
    }

    public class TunnelConfiguration
    {
        [JsonPropertyName("tunnel_id")]
        public string TunnelId { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("config")]
        public TunnelConfigData Config { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }   
}
