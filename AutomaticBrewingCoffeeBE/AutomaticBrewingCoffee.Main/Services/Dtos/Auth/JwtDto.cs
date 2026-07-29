namespace Services.Dtos.Auth;

public class JwtDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}