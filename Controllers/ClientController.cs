using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multiverse.Web.Interfaces;

namespace Multiverse.Web.Controllers;


[Authorize(Roles = "Client")]
public class ClientController(IClientService clients, IDocumentService docs) : Controller
{
     private readonly IClientService _clients = clients;
    private readonly IDocumentService _docs = docs;

    public IActionResult Dashboard() => View();

 public async Task<IActionResult> Details(int id)
    {
        var client = await _clients.GetByIdAsync(id);
        if (client is null) return NotFound();

        ViewData["Documents"] = await _docs.GetForClientAsync(id);
        return View(client);
    }

}