using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multiverse.Web.Interfaces;
using Multiverse.Web.Models;
using Multiverse.Web.ViewModels;

namespace Multiverse.Web.Controllers;

[Authorize(Roles = "Admin,Receptionist")]
public class ClientsController : Controller
{
    private readonly IClientService _clients;
      private readonly IDocumentService _docs;
    public ClientsController(IClientService clients, IDocumentService docs)
    {
        _clients = clients;
        _docs = docs;
    }

    [HttpGet]
public async Task<IActionResult> Index(
    string? q, ClientStatus? status, IndianState? state,
    DateTime? from, DateTime? to, int page = 1, int pageSize = 10)
{
    var filter = new ClientFilter
    {
        Q = q, Status = status, State = state, CreatedFrom = from, CreatedTo = to
    };

    var (rows, filteredTotal) = await _clients.QueryAsync(filter, page, pageSize);

    var vm = new ClientListViewModel
    {
        Clients = rows,
        Query = q, Status = status, State = state,
        CreatedFrom = from, CreatedTo = to,
        Page = page, PageSize = pageSize, FilteredTotal = filteredTotal,
        Total = await _clients.CountAsync(),
        Active = await _clients.CountActiveAsync(),
        NewThisWeek = await _clients.CountNewThisWeekAsync()
    };

    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        return PartialView("_ClientsTable", vm);

    return View(vm);
}

// ---------- EXPORT ----------
[HttpGet]
public async Task<IActionResult> Export(
    string? q, ClientStatus? status, IndianState? state,
    DateTime? from, DateTime? to)
{
    var filter = new ClientFilter
    {
        Q = q, Status = status, State = state, CreatedFrom = from, CreatedTo = to
    };

    var rows = await _clients.GetForExportAsync(filter);

    using var wb = new ClosedXML.Excel.XLWorkbook();
    var ws = wb.Worksheets.Add("Clients");
    var headers = new[]
    {
        "Id", "Full Name", "Email", "Phone", "Aadhaar", "PAN",
        "Address", "City", "State", "PIN", "Status", "Created", "Updated"
    };
    for (int i = 0; i < headers.Length; i++)
        ws.Cell(1, i + 1).Value = headers[i];

    var head = ws.Range(1, 1, 1, headers.Length);
    head.Style.Font.Bold = true;
    head.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#f4f7fc");
    head.Style.Border.BottomBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

    int r = 2;
    foreach (var c in rows)
    {
        ws.Cell(r, 1).Value = c.Id;
        ws.Cell(r, 2).Value = c.FullName;
        ws.Cell(r, 3).Value = c.Email;
        ws.Cell(r, 4).Value = c.Phone;
        ws.Cell(r, 5).Value = c.AadhaarNumber ?? "";
        ws.Cell(r, 6).Value = c.PanNumber ?? "";
        ws.Cell(r, 7).Value = c.AddressLine;
        ws.Cell(r, 8).Value = c.City;
        ws.Cell(r, 9).Value = c.State.ToString();
        ws.Cell(r, 10).Value = c.PinCode;
        ws.Cell(r, 11).Value = c.Status.ToString();
        ws.Cell(r, 12).Value = c.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
        ws.Cell(r, 13).Value = c.UpdatedAt?.ToLocalTime().ToString("yyyy-MM-dd HH:mm") ?? "";
        r++;
    }
    ws.Columns().AdjustToContents();

    using var ms = new MemoryStream();
    wb.SaveAs(ms);
    ms.Position = 0;
    var name = $"clients-{DateTime.UtcNow:yyyyMMdd-HHmm}.xlsx";
    return File(ms.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        name);
}

// ---------- IMPORT ----------
[HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(20 * 1024 * 1024)]
public async Task<IActionResult> Import(IFormFile file)
{
    if (file is null || file.Length == 0)
    {
        TempData["Error"] = "Choose an .xlsx file first.";
        return RedirectToAction(nameof(Index));
    }
    if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
    {
        TempData["Error"] = "Only .xlsx files are supported.";
        return RedirectToAction(nameof(Index));
    }

    var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    int created = 0, skipped = 0;
    var errors = new List<string>();

    using var stream = file.OpenReadStream();
    using var wb = new ClosedXML.Excel.XLWorkbook(stream);
    var ws = wb.Worksheet(1);

    // Expect headers in row 1, data from row 2
    var headerRow = ws.Row(1);
    var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    foreach (var cell in headerRow.CellsUsed())
        colMap[cell.GetString().Trim()] = cell.Address.ColumnNumber;

    string Get(ClosedXML.Excel.IXLRow row, string key)
    {
        if (!colMap.TryGetValue(key, out var col)) return "";
        return row.Cell(col).GetString().Trim();
    }

    foreach (var row in ws.RowsUsed().Skip(1))
    {
        var fullName = Get(row, "Full Name");
        var email = Get(row, "Email");
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
        {
            skipped++;
            continue;
        }

        if (await _clients.EmailExistsAsync(email))
        {
            skipped++;
            errors.Add($"Row {row.RowNumber()}: {email} already exists.");
            continue;
        }

        var stateRaw = Get(row, "State");
        IndianState state = IndianState.Maharashtra;
        if (!string.IsNullOrWhiteSpace(stateRaw))
        {
            if (Enum.TryParse<IndianState>(stateRaw.Replace(" ", ""), true, out var st)) state = st;
            else
            {
                // try by display value already collapsed in the sheet
                var hit = Enum.GetValues<IndianState>()
                    .FirstOrDefault(v => string.Equals(v.ToString(), stateRaw, StringComparison.OrdinalIgnoreCase));
                if (hit != default) state = hit;
            }
        }

        var statusRaw = Get(row, "Status");
        ClientStatus status = ClientStatus.Active;
        if (Enum.TryParse<ClientStatus>(statusRaw, true, out var s)) status = s;

        var client = new Client
        {
            FullName = fullName,
            Email = email.ToLowerInvariant(),
            Phone = Get(row, "Phone"),
            AadhaarNumber = string.IsNullOrWhiteSpace(Get(row, "Aadhaar")) ? null : Get(row, "Aadhaar"),
            PanNumber = string.IsNullOrWhiteSpace(Get(row, "PAN")) ? null : Get(row, "PAN").ToUpperInvariant(),
            AddressLine = string.IsNullOrWhiteSpace(Get(row, "Address")) ? "—" : Get(row, "Address"),
            City = string.IsNullOrWhiteSpace(Get(row, "City")) ? "—" : Get(row, "City"),
            State = state,
            PinCode = string.IsNullOrWhiteSpace(Get(row, "PIN")) ? "000000" : Get(row, "PIN"),
            Status = status
        };
        await _clients.CreateAsync(client, uid);
        created++;
    }

    var msg = $"Import complete: {created} created, {skipped} skipped.";
    TempData[created > 0 ? "Toast" : "Error"] = msg;
    return RedirectToAction(nameof(Index));
}

// ---------- BULK ----------
[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Bulk(int action, string ids)
{
    var list = (ids ?? "")
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(s => int.TryParse(s, out var n) ? n : 0)
        .Where(n => n > 0)
        .Distinct()
        .ToList();

    if (list.Count == 0)
        return Json(new { ok = false, message = "Nothing selected." });

    var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var act = action switch
    {
        1 => (await _clients.BulkActivateAsync(list, uid), "activated"),
        2 => (await _clients.BulkDeactivateAsync(list, uid), "deactivated"),
        3 => (await _clients.BulkDeleteAsync(list, uid), "deleted"),
        _ => (0, "unknown")
    };

    return Json(new { ok = true, count = act.Item1, verb = act.Item2 });
}

    public async Task<IActionResult> Details(int id)
    {
        var client = await _clients.GetByIdAsync(id);
        if (client is null) return NotFound();
        ViewData["Documents"] = await _docs.GetForClientAsync(id); 
        return View(client);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new ClientFormViewModel
        {
            AssignableEmployees = await _clients.GetAssignableEmployeesAsync()
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClientFormViewModel vm)
    {
        if (await _clients.EmailExistsAsync(vm.Email))
            ModelState.AddModelError(nameof(vm.Email), "A client with this email already exists.");

        if (vm.MinBudget.HasValue && vm.MaxBudget.HasValue && vm.MaxBudget < vm.MinBudget)
        ModelState.AddModelError(nameof(vm.MaxBudget), "Maximum budget must be greater than or equal to minimum.");

        if (!ModelState.IsValid)
        {
            vm.AssignableEmployees = await _clients.GetAssignableEmployeesAsync();
            return View(vm);
        }

        if (!ModelState.IsValid) return View(vm);

        var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var client = MapToEntity(vm);
        await _clients.CreateAsync(client, uid);

        TempData["Toast"] = $"Client “{client.FullName}” created.";
        return RedirectToAction(nameof(Index));
    }

    
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var client = await _clients.GetByIdAsync(id);
        if (client is null) return NotFound();

        var vm = MapToForm(client);
        vm.AssignableEmployees = await _clients.GetAssignableEmployeesAsync();

        ViewData["Documents"] = await _docs.GetForClientAsync(id);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClientFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        if (await _clients.EmailExistsAsync(vm.Email, exceptId: id))
            ModelState.AddModelError(nameof(vm.Email), "Another client already uses this email.");

        if (!ModelState.IsValid)
        {
            vm.AssignableEmployees = await _clients.GetAssignableEmployeesAsync();
            ViewData["Documents"] = await _docs.GetForClientAsync(id);           
            return View(vm);
        }

        var ok = await _clients.UpdateAsync(MapToEntity(vm));
        if (!ok) return NotFound();

        TempData["Toast"] = $"Client “{vm.FullName}” updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _clients.DeactivateAsync(id);
        TempData["Toast"] = "Client marked inactive.";
        return RedirectToAction(nameof(Index));
    }

    // AJAX: toggles Active <-> Inactive
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var client = await _clients.GetByIdAsync(id);
        if (client is null) return NotFound();

        if (client.Status == ClientStatus.Active)
            await _clients.DeactivateAsync(id);
        else
            await _clients.ReactivateAsync(id);   // new — see step 2

        var updated = await _clients.GetByIdAsync(id);
        return Json(new { ok = true, status = updated!.Status.ToString() });
    }

// Guarded hard delete — only allowed when Inactive
[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(int id)
{
    var client = await _clients.GetByIdAsync(id);
    if (client is null) return NotFound();

    if (client.Status != ClientStatus.Inactive)
        return Json(new { ok = false, message = "Deactivate the client first, then delete." });

    var deleted = await _clients.DeleteAsync(id);   // new — see step 2
    if (!deleted)
        return Json(new { ok = false, message = "Could not delete client." });

    return Json(new { ok = true, message = $"Client “{client.FullName}” and all documents deleted." });
}

[HttpGet]
public IActionResult Template()
{
    using var wb = new ClosedXML.Excel.XLWorkbook();
    var ws = wb.Worksheets.Add("Clients");
    var headers = new[]
    {
        "Full Name", "Email", "Phone", "Aadhaar", "PAN",
        "Address", "City", "State", "PIN", "Status"
    };
    for (int i = 0; i < headers.Length; i++) ws.Cell(1, i + 1).Value = headers[i];
    ws.Row(1).Style.Font.Bold = true;
    ws.Cell(2, 1).Value = "Example Client";
    ws.Cell(2, 2).Value = "example@example.in";
    ws.Cell(2, 3).Value = "+919876543210";
    ws.Cell(2, 8).Value = "Maharashtra";
    ws.Cell(2, 10).Value = "Active";
    ws.Columns().AdjustToContents();
    using var ms = new MemoryStream(); wb.SaveAs(ms);
    return File(ms.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "clients-template.xlsx");
}

    // ---- mappers ----
    private static Client MapToEntity(ClientFormViewModel vm) => new()
    {
        Id = vm.Id,
        FullName = vm.FullName.Trim(),
        Email = vm.Email.Trim().ToLowerInvariant(),
        Phone = vm.Phone.Trim(),
        AadhaarNumber = string.IsNullOrWhiteSpace(vm.AadhaarNumber) ? null : vm.AadhaarNumber.Trim(),
        PanNumber = string.IsNullOrWhiteSpace(vm.PanNumber) ? null : vm.PanNumber.Trim().ToUpperInvariant(),
        AddressLine = vm.AddressLine.Trim(),
        City = vm.City.Trim(),
        State = vm.State,
        PinCode = vm.PinCode.Trim(),
        Status = vm.Status,
        // NEW
        ClientType = vm.ClientType,
        Occupation = string.IsNullOrWhiteSpace(vm.Occupation) ? null : vm.Occupation.Trim(),
        InterestedIn = vm.InterestedIn,
        PreferredPropertyType = vm.PreferredPropertyType,
        PreferredLocation = string.IsNullOrWhiteSpace(vm.PreferredLocation) ? null : vm.PreferredLocation.Trim(),
        MinBudget = vm.MinBudget,
        MaxBudget = vm.MaxBudget,
        BHK = vm.BHK,
        PreferredArea = string.IsNullOrWhiteSpace(vm.PreferredArea) ? null : vm.PreferredArea.Trim(),
        Possession = vm.Possession,
        LoanRequired = vm.LoanRequired,
        LeadSource = vm.LeadSource,
        AssignedEmployeeId = vm.AssignedEmployeeId,
        LeadStatus = vm.LeadStatus,
        Priority = vm.Priority,
        NextFollowUpAt = vm.NextFollowUpAt,
        InitialNotes = string.IsNullOrWhiteSpace(vm.InitialNotes) ? null : vm.InitialNotes.Trim()
    };

    private static ClientFormViewModel MapToForm(Client c) => new()
    {
        Id = c.Id,
        FullName = c.FullName,
        Email = c.Email,
        Phone = c.Phone,
        AadhaarNumber = c.AadhaarNumber,
        PanNumber = c.PanNumber,
        AddressLine = c.AddressLine,
        City = c.City,
        State = c.State,
        PinCode = c.PinCode,
        Status = c.Status,

        ClientType = c.ClientType,
        Occupation = c.Occupation,
        InterestedIn = c.InterestedIn,
        PreferredPropertyType = c.PreferredPropertyType,
        PreferredLocation = c.PreferredLocation,
        MinBudget = c.MinBudget,
        MaxBudget = c.MaxBudget,
        BHK = c.BHK,
        PreferredArea = c.PreferredArea,
        Possession = c.Possession,
        LoanRequired = c.LoanRequired,
        LeadSource = c.LeadSource,
        AssignedEmployeeId = c.AssignedEmployeeId,
        LeadStatus = c.LeadStatus,
        Priority = c.Priority,
        NextFollowUpAt = c.NextFollowUpAt,
        InitialNotes = c.InitialNotes
    };
}