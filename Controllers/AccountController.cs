using System.Linq;
using Becas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Becas.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AccountController(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel
        {
            ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? Url.Action("Index", "Home") : returnUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(model.UserName!, model.Password!, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Inicio de sesión no válido. Verifique sus credenciales.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Roles()
    {
        var roles = await Task.FromResult(_roleManager.Roles.Select(r => r.Name ?? string.Empty).Where(r => !string.IsNullOrWhiteSpace(r)).OrderBy(r => r).ToList());
        var usersWithRoles = new List<UserRolesViewModel>();

        var users = _userManager.Users.ToList();
        foreach (var user in users)
        {
            var rolesForUser = await _userManager.GetRolesAsync(user);
            usersWithRoles.Add(new UserRolesViewModel
            {
                UserName = user.UserName ?? user.Id,
                Roles = rolesForUser
            });
        }

        var viewModel = new RolesViewModel
        {
            AvailableRoles = roles,
            Users = usersWithRoles
        };

        return View(viewModel);
    }

    [Authorize]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
