using Services.Dtos.Auth;

namespace Services.Firebase;

public interface IFirebaseAuthService
{
    Task<FirebaseLoginResponse> LoginByEmailPassword(string email, string password);
}