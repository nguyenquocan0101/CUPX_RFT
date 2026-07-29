using System.Text;
using System.Text.Json;
using FirebaseAdmin.Auth;
using Services.Dtos.Auth;

namespace Services.Firebase;

public class FirebaseAuthService : IFirebaseAuthService
{
    private readonly FirebaseAuth _firebaseAuth;
    private readonly FirebaseOptions _firebaseOptions;
    private readonly IHttpClientFactory _httpClientFactory;

    public FirebaseAuthService(FirebaseAuth firebaseAuth, FirebaseOptions firebaseOptions,
        IHttpClientFactory httpClientFactory)
    {
        _firebaseAuth = firebaseAuth;
        _firebaseOptions = firebaseOptions;
        _httpClientFactory = httpClientFactory;
    }



    public async Task<FirebaseLoginResponse> LoginByEmailPassword(string email, string password)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var firebaseAuthUrl =
            $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_firebaseOptions.ApiKey}";

        var payload = new
        {
            email,
            password,
            returnSecureToken = true
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            var response = await httpClient.PostAsync(firebaseAuthUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.StatusCode.ToString());
            }

            var responseStream = await response.Content.ReadAsStreamAsync();
            var loginResponse = await JsonSerializer.DeserializeAsync<FirebaseLoginResponse>(responseStream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            return loginResponse;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return new();
    }
}