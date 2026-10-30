using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multiverse.Web.Interfaces;
using Multiverse.Web.ViewModels;

namespace Multiverse.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IUserService _users;
    private readonly IClientService _clients;

    public AdminController(IUserService users, IClientService clients)
    {
        _users = users;
        _clients = clients;
    }

    public async Task<IActionResult> Dashboard()
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var me = await _users.GetByIdAsync(id);

        ViewData["TotalClients"] = await _clients.CountAsync();
        ViewData["ActiveClients"] = await _clients.CountActiveAsync();

        return View(new DashboardViewModel
        {
            User = me!,
            RoleLabel = "Administrator"
        });
    }
}