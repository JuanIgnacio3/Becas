using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace Becas.Identity;

/// <summary>
/// Seeds demo users and roles so the instructor can show how Identity works without a database.
/// </summary>
public class DummyIdentitySeeder
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<DummyIdentitySeeder> _logger;

    private static readonly (string UserName, string Password, string[] Roles)[] DemoUsers =
    {
        ("profesor", "Demo123$", new[] { "Administrador", "Revisor" }),
        ("estudiante", "Demo123$", new[] { "Estudiante" }),
        ("coordinador", "Demo123$", new[] { "Revisor" }),
        ("COORDINADOR", "Demo123$", new[] { "Revisor", "Administrador" })
    };

    public DummyIdentitySeeder(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<DummyIdentitySeeder> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        foreach (var roleName in DemoUsers.SelectMany(u => u.Roles).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (!roleResult.Succeeded)
                {
                    _logger.LogWarning("No se pudo crear el rol {Role}: {Errors}", roleName, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
        }

        foreach (var (userName, password, roles) in DemoUsers)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user is null)
            {
                user = new IdentityUser
                {
                    UserName = userName,
                    Email = $"{userName}@demo.local",
                    EmailConfirmed = true,
                };

                var createResult = await _userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                {
                    _logger.LogWarning("No se pudo crear el usuario {User}: {Errors}", userName, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    continue;
                }
            }

            foreach (var roleName in roles)
            {
                if (!await _userManager.IsInRoleAsync(user, roleName))
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, roleName);
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogWarning("No se pudo asignar el rol {Role} al usuario {User}: {Errors}", roleName, userName, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }
            }
        }
    }
}
