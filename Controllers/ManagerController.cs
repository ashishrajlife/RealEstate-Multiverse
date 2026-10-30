using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Multiverse.Web.Controllers;

[Authorize(Roles = "Manager")]
public class ManagerController : Controller
{
    public IActionResult Dashboard() => View();
}