using Services.Dtos.Auth;
using Services.Firebase;

namespace Services.Local;

public sealed class DisabledFirebaseAuthService : IFirebaseAuthService
{
    public Task<FirebaseLoginResponse> LoginByEmailPassword(string email, string password)
    {
        throw new InvalidOperationException(
            "Firebase authentication is disabled in local mode. Use the email/password login endpoint.");
    }
}
