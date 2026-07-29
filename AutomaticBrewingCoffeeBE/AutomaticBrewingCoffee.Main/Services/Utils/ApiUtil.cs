using System.Text;
using Newtonsoft.Json;

namespace AutomaticBrewingCoffee.Services.Utils;

public class ApiUtil
{
    // GET Request
    public static async Task<HttpResponseMessage> GetAsync(string url, Dictionary<string, string>? headers = null,
        string? token = null, Dictionary<string, string?>? queryParams = null)
    {
        if (queryParams != null && queryParams.Any())
        {
            var query = string.Join("&", queryParams
                .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Key) && kvp.Value != null)
                .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

            url = $"{url}?{query}";
        }

        using var httpClient = new HttpClient();

        // Add headers if provided
        if (headers != null)
        {
            foreach (var header in headers)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        // Add authorization token if provided
        if (!string.IsNullOrEmpty(token))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return await httpClient.GetAsync(url);
    }

    // POST Request
    public static async Task<HttpResponseMessage> PostAsync(string url, object? data = null,
        Dictionary<string, string>? headers = null, string? token = null,
        Dictionary<string, string?>? queryParams = null)
    {
        if (queryParams != null && queryParams.Any())
        {
            var query = string.Join("&",
                queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            url = $"{url}?{query}";
        }

        var json = data != null ? System.Text.Json.JsonSerializer.Serialize(data) : string.Empty;
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpClient = new HttpClient();

        // Add headers if provided
        if (headers != null)
        {
            foreach (var header in headers)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        // Add authorization token if provided
        if (!string.IsNullOrEmpty(token))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return await httpClient.PostAsync(url, content);
    }

    // PUT Request
    public static async Task<HttpResponseMessage> PutAsync(string url, object? data = null,
        Dictionary<string, string>? headers = null, string? token = null,
        Dictionary<string, string?>? queryParams = null)
    {
        if (queryParams != null && queryParams.Any())
        {
            var query = string.Join("&",
                queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            url = $"{url}?{query}";
        }

        var json = data != null ? System.Text.Json.JsonSerializer.Serialize(data) : string.Empty;
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpClient = new HttpClient();

        // Add headers if provided
        if (headers != null)
        {
            foreach (var header in headers)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        // Add authorization token if provided
        if (!string.IsNullOrEmpty(token))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return await httpClient.PutAsync(url, content);
    }

    // DELETE Request
    public static async Task<HttpResponseMessage> DeleteAsync(string url, Dictionary<string, string>? headers = null,
        string? token = null, Dictionary<string, string?>? queryParams = null)
    {
        if (queryParams != null && queryParams.Any())
        {
            var query = string.Join("&",
                queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            url = $"{url}?{query}";
        }

        using var httpClient = new HttpClient();

        // Add headers if provided
        if (headers != null)
        {
            foreach (var header in headers)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        // Add authorization token if provided
        if (!string.IsNullOrEmpty(token))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return await httpClient.DeleteAsync(url);
    }

    //Delete with body
    public static async Task<HttpResponseMessage> DeleteAsync(string url, object data,
        Dictionary<string, string>? headers = null, string? token = null,
        Dictionary<string, string?>? queryParams = null)
    {
        // Add query parameters if present
        if (queryParams != null && queryParams.Any())
        {
            var query = string.Join("&",
                queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            url = $"{url}?{query}";
        }

        // Serialize the data object into JSON if provided
        var json = data != null ? System.Text.Json.JsonSerializer.Serialize(data) : string.Empty;
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Create the HttpClient
        using var httpClient = new HttpClient();

        // Add headers if provided
        if (headers != null)
        {
            foreach (var header in headers)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        // Add authorization token if provided
        if (!string.IsNullOrEmpty(token))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri(url),
            Content = content // Attach the body content
        };
        return await httpClient.SendAsync(request);
    }


    //Handle Response
    public static T HandleResponse<T>(HttpResponseMessage response) where T : new()
    {
        try
        {
            response.EnsureSuccessStatusCode();

            var content = response.Content.ReadAsStringAsync().Result;
            if (string.IsNullOrEmpty(content))
            {
                throw new InvalidOperationException("Response content is empty.");
            }

            return JsonConvert.DeserializeObject<T>(content) ?? new T();
        }
        // catch (TaskCanceledException ex)
        // {
        //     Console.WriteLine("Task canceled: " + ex.Messages);
        //     throw;
        // }
        // catch (OperationCanceledException ex)
        // {
        //     Console.WriteLine("Operation canceled: " + ex.Messages);
        //     throw;
        // }
        // catch (JsonSerializationException ex)
        // {
        //     Console.WriteLine("JSON deserialization error: " + ex.Messages);
        //     throw;
        // }
        catch (Exception ex)
        {
            Console.WriteLine("Error handling response: " + ex.Message);
            throw;
        }
    }
}