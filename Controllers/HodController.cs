using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Multiverse.Web.Controllers;

[Authorize(Roles = "Hod")]
public class HodController : Controller
{
    public IActionResult Dashboard() => View();
}