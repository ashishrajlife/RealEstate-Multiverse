using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multiverse.Web.Interfaces;
using Multiverse.Web.Models;

namespace Multiverse.Web.Controllers;

[Authorize(Roles = "Admin,Receptionist,Manager")]
public class ClientDocumentsController : Controller
{
    private readonly IDocumentService _docs;
    private readonly IClientService _clients;

    public ClientDocumentsController(IDocumentService docs, IClientService clients)
    {
        _docs = docs;
        _clients = clients;
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(11 * 1024 * 1024)] // 10 MB file + form
    public async Task<IActionResult> Upload(int clientId, DocumentType type, IFormFile file)
    {
        var client = await _clients.GetByIdAsync(clientId);
        if (client is null) return NotFound();

        if (file is null || file.Length == 0)
        {
            TempData["Error"] = "Choose a file first.";
            return RedirectToAction("Edit", "Clients", new { id = clientId });
        }

        try
        {
            var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _docs.UploadAsync(client, type, file, uid);
            TempData["Toast"] = $"{type.Label()} uploaded.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Edit", "Clients", new { id = clientId });
    }

    // [HttpGet]
    // public async Task<IActionResult> Download(int id)
    // {
    //     var doc = await _docs.GetByIdAsync(id);
    //     if (doc is null) return NotFound();

    //     var path = await _docs.GetPhysicalPathAsync(id);
    //     if (path is null) return NotFound("File missing on disk.");

    //     // Serve as attachment with the original filename
    //     var stream = System.IO.File.OpenRead(path);
    //     return File(stream, doc.ContentType, doc.OriginalName);
    // }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int clientId)
    {
        var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _docs.DeleteAsync(id, uid);
        TempData["Toast"] = "Document removed.";
        return RedirectToAction("Edit", "Clients", new { id = clientId });
    }
    // ---------- INLINE VIEWER: opens in a new tab, admin can download from there ----------
[HttpGet]
public async Task<IActionResult> Inline(int id)
{
    var doc = await _docs.GetByIdAsync(id);
    if (doc is null) return NotFound();

    var path = await _docs.GetPhysicalPathAsync(id);
    if (path is null) return NotFound("File missing on disk.");

    // Stream through a signed-URL-free (authenticated) view endpoint
    ViewData["DocId"] = doc.Id;
    ViewData["OriginalName"] = doc.OriginalName;
    ViewData["ContentType"] = doc.ContentType;
    ViewData["FileUrl"] = Url.Action(nameof(Raw), "ClientDocuments", new { id = doc.Id });
    ViewData["DownloadUrl"] = Url.Action(nameof(Download), "ClientDocuments", new { id = doc.Id });
    ViewData["IsPdf"] = doc.ContentType?.Contains("pdf", StringComparison.OrdinalIgnoreCase) == true
                        || doc.OriginalName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
    ViewData["IsImage"] = doc.ContentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true;

    return View("~/Views/ClientDocuments/Viewer.cshtml");
}

// ---------- RAW FILE STREAM: used only by the viewer iframe/img tag ----------
[HttpGet]
public async Task<IActionResult> Raw(int id)
{
    var doc = await _docs.GetByIdAsync(id);
    if (doc is null) return NotFound();

    var path = await _docs.GetPhysicalPathAsync(id);
    if (path is null) return NotFound("File missing on disk.");

    // Serve inline so the browser renders it inside the viewer iframe
    var stream = System.IO.File.OpenRead(path);
    return File(stream, doc.ContentType ?? "application/octet-stream", enableRangeProcessing: true);
}

// ---------- FORCE DOWNLOAD: "Save as" dialog ----------
[HttpGet]
public async Task<IActionResult> Download(int id)
{
    var doc = await _docs.GetByIdAsync(id);
    if (doc is null) return NotFound();

    var path = await _docs.GetPhysicalPathAsync(id);
    if (path is null) return NotFound("File missing on disk.");

    var stream = System.IO.File.OpenRead(path);
    return File(stream, doc.ContentType ?? "application/octet-stream", doc.OriginalName);   // attachment
}
}