namespace Rivo.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }

    string? Email { get; }

    string? IpAddress { get; }

    bool HasPermission(string permission);
}
