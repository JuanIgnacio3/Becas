using System.Collections.Generic;

namespace Becas.Models;

public class RolesViewModel
{
    public IList<string> AvailableRoles { get; set; } = new List<string>();
    public IList<UserRolesViewModel> Users { get; set; } = new List<UserRolesViewModel>();
}

public class UserRolesViewModel
{
    public string UserName { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();
}
