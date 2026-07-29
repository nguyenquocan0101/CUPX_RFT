using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using AutomaticBrewingCoffee.Repository.Interfaces;
using AutoMapper;

namespace Services.Implements;

public class BaseService<T>
{
    protected readonly IUnitOfWork _unitOfWork;

    protected readonly ILogger<T> _logger;

    protected readonly IMapper _mapper;

    //protected readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private IUnitOfWork unitOfWork;
    private ILoggerFactory loggerFactory;
    private IHttpContextAccessor httpContextAccessor;

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
    protected void LogMessage(LogLevel logLevel, string message, params object[] args)
    {
        if (_logger.IsEnabled(logLevel))
        {
            _logger.Log(logLevel, message, args);
        }
    }
    
    private string? GetClaimValue(string claimType)
    {
        var claimValue = _httpContextAccessor?.HttpContext?.User?.Claims
            .FirstOrDefault(c => c.Type == claimType)?.Value;

        return claimValue;
    }

    protected string? GetAccountIdFromJwt()
    {
        var accountId = GetClaimValue("accountId");
        return accountId;
    }

    protected string? GetReferenceIdFromJwt()
    {
        var accountId = GetClaimValue("organizationId");
        return accountId;
    }

    protected string? GetKioskIdFromJwt()
    {
        var accountId = GetClaimValue("kioskId");
        return accountId;
    }

    /// <summary>
    /// Retrieves the list of roles assigned to the user from the JWT token.
    /// </summary>
    /// <returns>A list of user roles or an empty list if none are found.</returns>
    public List<string> GetAccountRolesFromJwt()
    {
        var roles = _httpContextAccessor?.HttpContext?.User?.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role" || c.Type == "roles")
            .Select(c => c.Value)
            .ToList() ?? new List<string>();
        return roles;
    }
}