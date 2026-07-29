using System.Text.Json;
using System.Text;
using CloudFlare.Client;
using Microsoft.Extensions.Options;
using Services.Cludflare.Models;
using Services.Cludflare.OptionConfig;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using CloudFlare.Client.Api.Zones.DnsRecord;
using CloudFlare.Client.Enumerators;

namespace Services.Cludflare
{
    public class CloudflareApi
    {
        private readonly HttpClient _httpClient;
        private readonly CloudflareInfo _cloudflareInfo;
        private readonly CloudFlareClient _cloudFlareClient;
        private readonly string _cfBaseUrl;
        private readonly CloudFlare.Client.Api.Accounts.Account _adminAccount;
        private readonly CloudFlare.Client.Api.Zones.Zone _zone;

        private const string tunnelType = "cloudflare";
        private const string contentPart = "cfargotunnel.com";
        private string _zoneName;

        public string ZoneDomain
        {
            get { return _zoneName; }
        }

        public CloudflareApi(HttpClient httpClient, IOptions<CloudflareInfo> options)
        {
            _httpClient = httpClient;
            _cloudflareInfo = options.Value;
            _httpClient.DefaultRequestHeaders.Add("X-Auth-Email", _cloudflareInfo.Email);
            _httpClient.DefaultRequestHeaders.Add("X-Auth-Key", _cloudflareInfo.ApiKey);

            _cloudFlareClient = new CloudFlareClient(_cloudflareInfo.Email, _cloudflareInfo.ApiKey);
            _cfBaseUrl = _cloudflareInfo.BaseUrl;
            _zone = GetZoneAsync().Result;
            _zoneName = _zone.Name;
            _adminAccount = GetAccountAsync().Result;
        }

        private async Task<CloudFlare.Client.Api.Zones.Zone> GetZoneAsync()
        {
            try
            {
                var zoneRs = await _cloudFlareClient.Zones.GetAsync();
                return zoneRs.Result[0]; //scenario explain: 1 zone is used in project
            }
            catch (Exception)
            {
                return new CloudFlare.Client.Api.Zones.Zone();
            }
        }
        private async Task<CloudFlare.Client.Api.Accounts.Account> GetAccountAsync()
        {
            try
            {
                var accountRs = await _cloudFlareClient.Accounts.GetAsync();
                return accountRs.Result[0]; //scenario explain: 1 account is used in project
            }
            catch (Exception)
            {
                return new CloudFlare.Client.Api.Accounts.Account();
            }
        }

        #region DNS Record
        public async Task<DnsRecord?> GetDnsRecordAsync(string dnsRecordId, string name)
        {
            var filter = new DnsRecordFilter
            {
                Type = DnsRecordType.Cname,
            };
            var response = await _cloudFlareClient.Zones.DnsRecords.GetAsync(dnsRecordId, filter);
            return response.Result.FirstOrDefault(d => d.Equals(name));
        }

        public async Task<DnsRecord> CreateDnsRecordAsync(string name, string tunnelId)
        {
            var newRecord = new NewDnsRecord
            {
                Name = name,
                Content = $"{tunnelId}.{contentPart}",
                Proxied = true,
                Ttl = 1,
                Type = DnsRecordType.Cname,
            };
            var dnsAdded = (await _cloudFlareClient.Zones.DnsRecords.AddAsync(_zone.Id, newRecord)).Result;
            return dnsAdded;
        }

        public async Task<bool> DeleteDnsRecordAsync(string dnsRecordId)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Auth-Email", _cloudflareInfo.Email);
            client.DefaultRequestHeaders.Add("X-Auth-Key", _cloudflareInfo.ApiKey);

            var url = $"https://api.cloudflare.com/client/v4/zones/{_zone.Id}/dns_records/{dnsRecordId}";
            var response = await client.DeleteAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (root.TryGetProperty("success", out var successProp) && successProp.GetBoolean())
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// return record or nothing by hostname of tunnel -> whatever result, it is true because dns record does not exist
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public async Task<DnsRecord?> GetDNSRecordByTunnelHostname(string hostName)
        {
            var dnsRecords = (await _cloudFlareClient.Zones.DnsRecords.GetAsync(zoneId: _zone.Id)).Result; 
            var targetRecord = dnsRecords.FirstOrDefault(d => d.Name.Split(".")[0].Equals(hostName) && d.Type.Equals(DnsRecordType.Cname));
            if (targetRecord == null) return null;

            return targetRecord;
        }
        #endregion

        #region Tunnel
        public async Task<string?> GetTunnelTokenAsync(string tunnelId)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Auth-Email", _cloudflareInfo.Email);
            client.DefaultRequestHeaders.Add("X-Auth-Key", _cloudflareInfo.ApiKey);

            var url = $"https://api.cloudflare.com/client/v4/accounts/{_adminAccount.Id}/cfd_tunnel/{tunnelId}/token";
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to fetch tunnel info.");
            }

            using var doc = JsonDocument.Parse(content);
            var resultElement = doc.RootElement.GetProperty("result");

            var result = JsonSerializer.Deserialize<string>(resultElement.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result;
        }
        public async Task<CloudflareTunnel?> GetTunnelInfoAsync(string tunnelId)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Auth-Email", _cloudflareInfo.Email);
            client.DefaultRequestHeaders.Add("X-Auth-Key", _cloudflareInfo.ApiKey);

            var url = $"https://api.cloudflare.com/client/v4/accounts/{_adminAccount.Id}/cfd_tunnel/{tunnelId}";
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to fetch tunnel info.");
            }

            using var doc = JsonDocument.Parse(content);
            var resultElement = doc.RootElement.GetProperty("result");

            var result = JsonSerializer.Deserialize<CloudflareTunnel>(resultElement.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result;
        }
        public async Task<List<CloudflareTunnel>> GetTunnelsAsync()
        {
            var url = _cfBaseUrl + $"accounts/{_adminAccount.Id}/cfd_tunnel";
            var response = await _httpClient.GetAsync(url);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to fetch tunnels: {response.StatusCode}, {responseBody}");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            using var doc = JsonDocument.Parse(responseBody);

            var resultElement = doc.RootElement.GetProperty("result");

            var result = JsonSerializer.Deserialize<List<CloudflareTunnel>>(resultElement.GetRawText(), options);

            return result!;
        }
        public async Task<TunnelConfigurationDetail> CreateTunnelAsync(string tunnelName)
        {
            var url = _cfBaseUrl + $"accounts/{_adminAccount.Id}/cfd_tunnel";
            var body = JsonSerializer.Serialize(new { name = tunnelName, config_src = tunnelType });

            var response = await _httpClient.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json"));
            var respBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to create tunnel");
            }

            using var doc = JsonDocument.Parse(respBody);
            var resultElement = doc.RootElement.GetProperty("result");

            var result = JsonSerializer.Deserialize<TunnelConfigurationDetail>(resultElement.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result!;
        }
        //new api version from cloudflare - using apiToken
        public async Task<bool> DeleteTunnelAsync(string tunnelId)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _cloudflareInfo.ApiToken);

            var url = _cfBaseUrl + $"accounts/{_adminAccount.Id}/cfd_tunnel/{tunnelId}";
            var response = await client.DeleteAsync(url);
            var respBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region Tunnel Configuration
        public async Task<TunnelConfiguration?> GetTunnelConfigurationAsync(string tunnelId)
        {
            var url = _cfBaseUrl + $"accounts/{_adminAccount.Id}/cfd_tunnel/{tunnelId}/configurations";
            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("result", out var result))
            {
                return null;
            }
            var rsJson = result.GetRawText();
            var config = JsonSerializer.Deserialize<TunnelConfiguration>(rsJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return config;
        }

        public async Task<TunnelConfiguration?> UpdateTunnelConfigurationAsync(
            string tunnelId,
            string hostname,
            string localServer = null,
            OriginRequest originRequest = null,
            string path = null)
        {
            var apiUrl = _cfBaseUrl + $"accounts/{_adminAccount.Id}/cfd_tunnel/{tunnelId}/configurations";

            var ingressRules = new List<IngressRule>
            {
                new IngressRule
                {
                    Hostname = hostname,
                    Service = localServer ?? _cloudflareInfo.DefaultServer,
                    OriginRequest = originRequest,
                    Path = path
                },
                new IngressRule
                {
                    Service = "http_status:404"
                }
            };

            var tunnelConfig = new
            {
                config = new TunnelConfigData
                {
                    Ingress = ingressRules
                }
            };

            var json = JsonSerializer.Serialize(tunnelConfig, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(apiUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            if (!root.TryGetProperty("result", out var result))
            {
                throw new Exception("Failed to get tunnel configuration");
            }
            var rsJson = result.GetRawText();
            var config = JsonSerializer.Deserialize<TunnelConfiguration>(rsJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return config;
        }
        #endregion
    }
}


