using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multiverse.Web.Models;

namespace Multiverse.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        // If not logged in, boot them to the login page
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Login", "Auth");
        }

        // If authenticated, route to the correct dashboard
        if (User.IsInRole("Admin"))        return RedirectToAction("Dashboard", "Admin");
        if (User.IsInRole("Hod"))          return RedirectToAction("Dashboard", "Hod");
        if (User.IsInRole("Manager"))      return RedirectToAction("Dashboard", "Manager");
        if (User.IsInRole("Receptionist")) return RedirectToAction("Dashboard", "Reception");
        if (User.IsInRole("Client"))       return RedirectToAction("Dashboard", "Client");

        // Fallback if a role is missing
        return RedirectToAction("Login", "Auth");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}