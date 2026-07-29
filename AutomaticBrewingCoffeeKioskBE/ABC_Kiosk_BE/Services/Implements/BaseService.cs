using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using System.Security.Claims;

namespace Services.Implements;

public class BaseService<T>
{
    protected readonly IUnitOfWork _unitOfWork;

    protected readonly ILogger<T> _logger;

    protected readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BaseService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILoggerFactory loggerFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _logger = loggerFactory.CreateLogger<T>();
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Logs a message with the specified log level.
    /// </summary>
    /// <param name="logLevel">The severity level of the log message.</param>
    /// <param name="message">The log message.</param>
    /// <param name="args">Additional arguments for formatting.</param>
    public void LogMessage(LogLevel logLevel, string message, params object[] args)
    {
        if (_logger.IsEnabled(logLevel))
        {
            _logger.Log(logLevel, message, args);
        }
    }

    // <summary>
    /// Retrieves a claim value from the JWT token based on the given claim type.
    /// </summary>
    /// <param name="claimType">The type of claim to retrieve.</param>
    /// <returns>The value of the claim or null if not found.</returns>
    public string? GetClaimValue(string claimType)
    {
        var claimValue = _httpContextAccessor?.HttpContext?.User?.Claims
            .FirstOrDefault(c => c.Type == claimType)?.Value;

        return claimValue;
    }

    /// <summary>
    /// Gets the user ID from the JWT token.
    /// </summary>
    /// <returns>The user ID or null if not present.</returns>
    public string? GetUserIdFromJwt()
    {
        var userId = GetClaimValue(ClaimTypes.NameIdentifier);
        return userId;
    }

    /// <summary>
    /// Gets the user's email address from the JWT token.
    /// </summary>
    /// <returns>The user's email or null if not present.</returns>
    public string? GetUserEmailFromJwt()
    {
        return GetClaimValue(ClaimTypes.Email);
    }

    /// <summary>
    /// Retrieves the list of roles assigned to the user from the JWT token.
    /// </summary>
    /// <returns>A list of user roles or an empty list if none are found.</returns>
    public List<string> GetUserRolesFromJwt()
    {
        var roles = _httpContextAccessor?.HttpContext?.User?.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? new List<string>();
        return roles;
    }

    /// <summary>
    /// Retrieves the user's profile picture URL from the JWT token.
    /// </summary>
    /// <returns>The profile picture URL or null if not available.</returns>
    public string? GetUserProfilePictureFromJwt()
    {
        return GetClaimValue(ClaimTypes.Uri);
    }
}