namespace Rwd.WF.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string UserId { get; }
    string UserName { get; }
    IEnumerable<string> Permissions { get; }
    bool HasPermission(string permission);
}

