using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace AutomaticBrewingCoffee.API.Attribute;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

public sealed class AuthorizesAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public AuthorizesAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        // Nếu chưa đăng nhập
        if (!user.Identity!.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Lấy tất cả các claims Role của user
        var userRoles = user.Claims.Where(c => c.Type == "role" || c.Type == "roles" || c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        // Kiểm tra user có ít nhất một role hợp lệ
        if (!_roles.IsNullOrEmpty())
        {
            bool isInRole = _roles.Any(role => userRoles.Contains(role));
            if (!isInRole)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}