using Microsoft.AspNetCore.Authorization;
using Rwd.WF.Application.Common.Interfaces;

namespace Rwd.WF.API.Authorization;

public record PermissionRequirement(string Permission) : IAuthorizationRequirement;

public class PermissionAuthorizationHandler(ICurrentUserService currentUser)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (currentUser.HasPermission(requirement.Permission))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

public static class PolicyNames
{
    public const string LookupRead = nameof(Permissions.LookupRead);
    public const string LookupCreate = nameof(Permissions.LookupCreate);
    public const string LookupUpdate = nameof(Permissions.LookupUpdate);
    public const string LookupDelete = nameof(Permissions.LookupDelete);
}

