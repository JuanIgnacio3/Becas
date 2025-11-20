using System.Collections.Concurrent;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace Becas.Identity;

/// <summary>
/// Simple in-memory role store for Identity demos.
/// </summary>
public class InMemoryRoleStore : IQueryableRoleStore<IdentityRole>
{
    private readonly ConcurrentDictionary<string, IdentityRole> _rolesById = new();

    public IQueryable<IdentityRole> Roles => _rolesById.Values.AsQueryable();

    public Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(role);
        role.Id ??= Guid.NewGuid().ToString();
       
        var stored = Clone(role)!;
        if (!_rolesById.TryAdd(role.Id, stored))
        {
            return Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "No se pudo crear el rol en memoria." }));
        }
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(role);
        var removed = _rolesById.TryRemove(role.Id, out _);
        return Task.FromResult(removed ? IdentityResult.Success : IdentityResult.Failed(new IdentityError { Description = "Rol no encontrado en memoria." }));
    }

    public void Dispose()
    {
        // Nothing to dispose
    }

    public Task<IdentityRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        _rolesById.TryGetValue(roleId, out var role);
        return Task.FromResult(Clone(role));
    }

    public Task<IdentityRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        var role = _rolesById.Values.FirstOrDefault(r => string.Equals(r.NormalizedName, normalizedRoleName, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(Clone(role));
    }

    public Task<string?> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(role);
        return Task.FromResult(role.NormalizedName);
    }

    public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(role);
        return Task.FromResult(role.Id);
    }

    public Task<string?> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(role);
        return Task.FromResult(role.Name);
    }

    public Task SetNormalizedRoleNameAsync(IdentityRole role, string? normalizedName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(role);
        var stored = GetTrackedRole(role);
        stored.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetRoleNameAsync(IdentityRole role, string? roleName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(role);
        var stored = GetTrackedRole(role);
        stored.Name = roleName;
        return Task.CompletedTask;
    }

    public Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(role);
        _rolesById[role.Id] = Clone(role)!;
        return Task.FromResult(IdentityResult.Success);
    }

    private IdentityRole GetTrackedRole(IdentityRole role)
    {
        if (!_rolesById.TryGetValue(role.Id, out var stored))
        {
            stored = Clone(role)!;
            _rolesById[role.Id] = stored;
        }
        return stored;
    }

    private static IdentityRole? Clone(IdentityRole? role)
    {
        if (role is null)
        {
            return null;
        }

        return new IdentityRole
        {
            Id = role.Id,
            Name = role.Name,
            NormalizedName = role.NormalizedName,
            ConcurrencyStamp = role.ConcurrencyStamp
        };
    }
}
