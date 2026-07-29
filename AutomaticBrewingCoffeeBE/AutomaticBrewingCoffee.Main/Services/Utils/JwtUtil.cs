using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutomaticBrewingCoffee.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Services.Utils;

public class JwtUtil
{
    public static string GenerateAccessToken(Account account, IConfiguration config)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var secretKeyBytes = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);
        var tokenDescription = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, account.AccountId),
                new Claim("accountId", account.AccountId),
                new Claim("email", account.Email),
                new Claim("fullname", account.FullName ?? ""),
                new Claim("role", account.RoleName),
                new Claim("organizationId", account.OrganizationId ?? ""),
            }),
            Expires = DateTime.UtcNow.AddDays(1),
            Issuer = config["Jwt:Issuer"],
            Audience = config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes),
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = jwtTokenHandler.WriteToken(jwtTokenHandler.CreateToken(tokenDescription));
        return token;
    }

    public static string GenerateRefreshToken(Account account, IConfiguration config)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var secretKeyBytes = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);
        var tokenDescription = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, account.AccountId),
                new Claim("accountId", account.AccountId),
                new Claim("email", account.Email),
                new Claim("fullname", account.FullName ?? ""),
                new Claim("role", account.RoleName),
                new Claim("organizationId", account.OrganizationId ?? "")
            ]),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes),
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = jwtTokenHandler.WriteToken(jwtTokenHandler.CreateToken(tokenDescription));
        return token;
    }

    public static ClaimsPrincipal? GetPrincipalFromToken(string token, IConfiguration config)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var secretKeyBytes = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            if (validatedToken is JwtSecurityToken jwtToken &&
                jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return principal;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}