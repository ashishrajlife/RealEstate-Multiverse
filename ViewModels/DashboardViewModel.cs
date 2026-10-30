using Multiverse.Web.Models;

namespace Multiverse.Web.ViewModels;

public class DashboardViewModel
{
    public User User { get; set; } = default!;
    public string RoleLabel { get; set; } = string.Empty;
}