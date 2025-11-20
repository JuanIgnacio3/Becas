using System.Collections.Concurrent;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace Becas.Identity;

/// <summary>
/// Simple in-memory user store used only for classroom demos.
/// </summary>
public class InMemoryUserStore : IQueryableUserStore<IdentityUser>, IUserPasswordStore<IdentityUser>, IUserRoleStore<IdentityUser>
{
    private readonly ConcurrentDictionary<string, IdentityUser> _usersById = new();
    private readonly ConcurrentDictionary<string, string> _userIdByNormalizedName = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _rolesByUserId = new();

    public IQueryable<IdentityUser> Users => _usersById.Values.AsQueryable();

    public Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        user.Id ??= Guid.NewGuid().ToString();
        user.PhoneNumber ??= Guid.NewGuid().ToString();
        user.Email = "email@email.com";
        
        var storedUser = Clone(user)!;
        if (!_usersById.TryAdd(user.Id, storedUser))
        {
            return Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "No se pudo crear el usuario en memoria." }));
        }

        IndexNormalizedName(storedUser.Id, storedUser.NormalizedUserName);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        var removed = _usersById.TryRemove(user.Id, out var removedUser);
        if (removedUser?.NormalizedUserName is not null)
        {
            _userIdByNormalizedName.TryRemove(removedUser.NormalizedUserName, out _);
        }
        _rolesByUserId.TryRemove(user.Id, out _);
        return Task.FromResult(removed ? IdentityResult.Success : IdentityResult.Failed(new IdentityError { Description = "Usuario no encontrado en memoria." }));
    }

    public void Dispose()
    {
        // Nothing to dispose
    }

    public Task<IdentityUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        _usersById.TryGetValue(userId, out var user);
        return Task.FromResult(Clone(user));
    }

    public Task<IdentityUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        IdentityUser? user = null;

        if (!string.IsNullOrWhiteSpace(normalizedUserName) && _userIdByNormalizedName.TryGetValue(normalizedUserName, out var userId))
        {
            _usersById.TryGetValue(userId, out user);
        }

        user ??= _usersById.Values.FirstOrDefault(u =>
            string.Equals(u.NormalizedUserName, normalizedUserName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(u.UserName, normalizedUserName, StringComparison.OrdinalIgnoreCase));

        if (user?.NormalizedUserName is not null)
        {
            _userIdByNormalizedName[user.NormalizedUserName] = user.Id;
        }

        return Task.FromResult(Clone(user));
    }

    public Task<string?> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.Id);
    }

    public Task<string?> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.UserName);
    }

    public Task SetNormalizedUserNameAsync(IdentityUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        var stored = GetTrackedUser(user);
        var previous = stored.NormalizedUserName;
        stored.NormalizedUserName = normalizedName;
        UpdateNormalizedNameIndex(stored.Id, previous, normalizedName);
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(IdentityUser user, string? userName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        var stored = GetTrackedUser(user);
        stored.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        _usersById.TryGetValue(user.Id, out var existing);
        var updated = Clone(user)!;
        _usersById[user.Id] = updated;
        UpdateNormalizedNameIndex(user.Id, existing?.NormalizedUserName, updated.NormalizedUserName);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task SetPasswordHashAsync(IdentityUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        var stored = GetTrackedUser(user);
        stored.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.PasswordHash is not null);
    }

    public Task AddToRoleAsync(IdentityUser user, string roleName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);
        var set = _rolesByUserId.GetOrAdd(user.Id, _ => new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase));
        set[roleName] = 0;
        return Task.CompletedTask;
    }

    public Task RemoveFromRoleAsync(IdentityUser user, string roleName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (_rolesByUserId.TryGetValue(user.Id, out var set))
        {
            set.TryRemove(roleName, out _);
        }
        return Task.CompletedTask;
    }

    public Task<IList<string>> GetRolesAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        IList<string> roles = Array.Empty<string>();
        if (_rolesByUserId.TryGetValue(user.Id, out var set))
        {
            roles = set.Keys.ToList();
        }
        return Task.FromResult(roles);
    }

    public Task<bool> IsInRoleAsync(IdentityUser user, string roleName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (_rolesByUserId.TryGetValue(user.Id, out var set))
        {
            return Task.FromResult(set.ContainsKey(roleName));
        }
        return Task.FromResult(false);
    }

    public Task<IList<IdentityUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);
        var result = new List<IdentityUser>();
        foreach (var kvp in _rolesByUserId)
        {
            if (kvp.Value.ContainsKey(roleName) && _usersById.TryGetValue(kvp.Key, out var user))
            {
                result.Add(Clone(user));
            }
        }
        return Task.FromResult<IList<IdentityUser>>(result);
    }

    private IdentityUser GetTrackedUser(IdentityUser user)
    {
        if (!_usersById.TryGetValue(user.Id, out var stored))
        {
            stored = Clone(user)!;
            _usersById[user.Id] = stored;
            IndexNormalizedName(stored.Id, stored.NormalizedUserName);
        }
        return stored;
    }

    private void IndexNormalizedName(string userId, string? normalizedUserName)
    {
        if (!string.IsNullOrWhiteSpace(normalizedUserName))
        {
            _userIdByNormalizedName[normalizedUserName] = userId;
        }
    }

    private void UpdateNormalizedNameIndex(string userId, string? previous, string? current)
    {
        if (!string.IsNullOrWhiteSpace(previous))
        {
            _userIdByNormalizedName.TryRemove(previous, out _);
        }

        IndexNormalizedName(userId, current);
    }

    private static IdentityUser? Clone(IdentityUser? user)
    {
        if (user is null)
        {
            return null;
        }

        return new IdentityUser
        {
            Id = user.Id,
            UserName = user.UserName,
            NormalizedUserName = user.NormalizedUserName,
            Email = user.Email,
            NormalizedEmail = user.NormalizedEmail,
            EmailConfirmed = user.EmailConfirmed,
            PasswordHash = user.PasswordHash,
            SecurityStamp = user.SecurityStamp,
            ConcurrencyStamp = user.ConcurrencyStamp,
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            LockoutEnd = user.LockoutEnd,
            LockoutEnabled = user.LockoutEnabled,
            AccessFailedCount = user.AccessFailedCount
        };
    }
}
