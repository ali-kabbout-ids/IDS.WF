using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Rwd.WF.Application.Common.Interfaces;

namespace Rwd.WF.Infrastructure.Identity;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly ClaimsPrincipal? _user = httpContextAccessor.HttpContext?.User;

    public string UserId => _user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
    public string UserName => _user?.FindFirstValue(ClaimTypes.Name) ?? "system";

    public IEnumerable<string> Permissions =>
        _user?.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value) ?? [];

    public bool HasPermission(string permission) => Permissions.Contains(permission);
}

