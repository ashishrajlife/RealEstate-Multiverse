using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Multiverse.Web.Controllers;

[Authorize(Roles = "Receptionist")]
public class ReceptionController : Controller
{
    public IActionResult Dashboard() => View();
}