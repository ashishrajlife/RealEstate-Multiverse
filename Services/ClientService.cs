using Microsoft.EntityFrameworkCore;
using Multiverse.Web.Data;
using Multiverse.Web.Interfaces;
using Multiverse.Web.Models;
using Multiverse.Web.ViewModels;

namespace Multiverse.Web.Services;

public class ClientService : IClientService
{
    private readonly AppDbContext _db;
   private readonly IWebHostEnvironment _env;
    public ClientService(AppDbContext db, IWebHostEnvironment env)
{
    _db = db;
    _env = env;
}

    public async Task<IReadOnlyList<Client>> GetAllAsync() =>
        await _db.Clients.AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    public Task<Client?> GetByIdAsync(int id) =>
        _db.Clients.AsNoTracking().Include(c => c.AssignedEmployee).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Client> CreateAsync(Client client, int createdByUserId)
    {
        client.CreatedAt = DateTime.UtcNow;
        client.CreatedByUserId = createdByUserId;
        client.UpdatedAt = null;
        _db.Clients.Add(client);
        await _db.SaveChangesAsync();
        return client;
    }

    public async Task<bool> UpdateAsync(Client client)
    {
        var existing = await _db.Clients.FirstOrDefaultAsync(c => c.Id == client.Id);
        if (existing is null) return false;

        existing.FullName       = client.FullName;
        existing.Email          = client.Email;
        existing.Phone          = client.Phone;
        existing.AadhaarNumber  = client.AadhaarNumber;
        existing.PanNumber      = client.PanNumber;
        existing.AddressLine    = client.AddressLine;
        existing.City           = client.City;
        existing.State          = client.State;
        existing.PinCode        = client.PinCode;
        existing.Status         = client.Status;
        existing.UpdatedAt      = DateTime.UtcNow;
        existing.ClientType            = client.ClientType;
        existing.Occupation            = client.Occupation;
        existing.InterestedIn          = client.InterestedIn;
        existing.PreferredPropertyType = client.PreferredPropertyType;
        existing.PreferredLocation     = client.PreferredLocation;
        existing.MinBudget             = client.MinBudget;
        existing.MaxBudget             = client.MaxBudget;
        existing.BHK                   = client.BHK;
        existing.PreferredArea         = client.PreferredArea;
        existing.Possession            = client.Possession;
        existing.LoanRequired          = client.LoanRequired;
        existing.LeadSource            = client.LeadSource;
        existing.AssignedEmployeeId    = client.AssignedEmployeeId;
        existing.LeadStatus            = client.LeadStatus;
        existing.Priority              = client.Priority;
        existing.NextFollowUpAt        = client.NextFollowUpAt;
        existing.InitialNotes          = client.InitialNotes;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var existing = await _db.Clients.FirstOrDefaultAsync(c => c.Id == id);
        if (existing is null) return false;
        existing.Status = ClientStatus.Inactive;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public Task<bool> EmailExistsAsync(string email, int? exceptId = null) =>
        _db.Clients.AnyAsync(c => c.Email == email && (!exceptId.HasValue || c.Id != exceptId.Value));

    public Task<int> CountAsync() => _db.Clients.CountAsync();

    public Task<int> CountActiveAsync() =>
        _db.Clients.CountAsync(c => c.Status == ClientStatus.Active);

    public Task<int> CountNewThisWeekAsync()
    {
        var cutoff = DateTime.UtcNow.AddDays(-7);
        return _db.Clients.CountAsync(c => c.CreatedAt >= cutoff);
    }
    public async Task<bool> ReactivateAsync(int id)
{
    var existing = await _db.Clients.FirstOrDefaultAsync(c => c.Id == id);
    if (existing is null) return false;
    existing.Status = ClientStatus.Active;
    existing.UpdatedAt = DateTime.UtcNow;
    await _db.SaveChangesAsync();
    return true;
}

public async Task<bool> DeleteAsync(int id)
{
    var existing = await _db.Clients
        .Include(c => c.Documents)
        .FirstOrDefaultAsync(c => c.Id == id);
    if (existing is null) return false;

    // 1. Delete each document file from disk (best effort)
    foreach (var doc in existing.Documents)
    {
        var abs = Path.Combine(_env.WebRootPath, doc.FilePath.Replace('/', Path.DirectorySeparatorChar));
        try { if (File.Exists(abs)) File.Delete(abs); } catch { /* swallow */ }
    }

    // 2. Delete the entire uploads/{slug}-{id} folder if it exists
    var slug = Slugify(existing.FullName) + "-" + existing.Id;
    var folder = Path.Combine(_env.WebRootPath, "uploads", slug);
    try { if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true); } catch { }

    // 3. Delete DB rows (cascade handles ClientDocuments)
    _db.Clients.Remove(existing);
    await _db.SaveChangesAsync();
    return true;
}

private static string Slugify(string name)
{
    var chars = name.Trim().ToLowerInvariant()
        .Select(c => char.IsLetterOrDigit(c) ? c : '-')
        .ToArray();
    var s = new string(chars);
    while (s.Contains("--")) s = s.Replace("--", "-");
    return s.Trim('-');
}

public async Task<(IReadOnlyList<Client> Rows, int Total)> QueryAsync(string? q, int page, int pageSize)
{
    page = Math.Max(1, page);
    pageSize = Math.Clamp(pageSize, 5, 100);

    var query = _db.Clients.AsNoTracking().AsQueryable();

    if (!string.IsNullOrWhiteSpace(q))
    {
        var ql = q.Trim().ToLower();
        query = query.Where(c =>
            c.FullName.ToLower().Contains(ql) ||
            c.Email.ToLower().Contains(ql) ||
            c.Phone.Contains(ql) ||
            c.City.ToLower().Contains(ql));
    }

    var total = await query.CountAsync();
    var rows = await query
        .OrderByDescending(c => c.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return (rows, total);
}

public async Task<(IReadOnlyList<Client> Rows, int Total)> QueryAsync(ClientFilter f, int page, int pageSize)
{
    page = Math.Max(1, page);
    pageSize = Math.Clamp(pageSize, 5, 200);

    var q = BuildQuery(f);
    var total = await q.CountAsync();
    var rows = await q   .Include(c => c.AssignedEmployee)
    .OrderByDescending(c => c.CreatedAt)
                      .Skip((page - 1) * pageSize)
                      .Take(pageSize)
                      .ToListAsync();
    return (rows, total);
}

public async Task<IReadOnlyList<Client>> GetForExportAsync(ClientFilter f) =>
    await BuildQuery(f).OrderBy(c => c.Id).ToListAsync();

private IQueryable<Client> BuildQuery(ClientFilter f)
{
    var query = _db.Clients.AsNoTracking().AsQueryable();

    if (!string.IsNullOrWhiteSpace(f.Q))
    {
        var ql = f.Q.Trim().ToLower();
        query = query.Where(c =>
            c.FullName.ToLower().Contains(ql) ||
            c.Email.ToLower().Contains(ql) ||
            c.Phone.Contains(ql) ||
            c.City.ToLower().Contains(ql));
    }
    if (f.Status.HasValue)
        query = query.Where(c => c.Status == f.Status.Value);
    if (f.State.HasValue)
        query = query.Where(c => c.State == f.State.Value);
    if (f.CreatedFrom.HasValue)
        query = query.Where(c => c.CreatedAt >= f.CreatedFrom.Value);
    if (f.CreatedTo.HasValue)
        query = query.Where(c => c.CreatedAt < f.CreatedTo.Value.AddDays(1));

    return query;
}

public async Task<int> BulkActivateAsync(IEnumerable<int> ids, int actor)
{
    var list = ids.Distinct().ToList();
    var rows = await _db.Clients.Where(c => list.Contains(c.Id)).ToListAsync();
    foreach (var c in rows) { c.Status = ClientStatus.Active; c.UpdatedAt = DateTime.UtcNow; }
    await _db.SaveChangesAsync();
    return rows.Count;
}

public async Task<int> BulkDeactivateAsync(IEnumerable<int> ids, int actor)
{
    var list = ids.Distinct().ToList();
    var rows = await _db.Clients.Where(c => list.Contains(c.Id)).ToListAsync();
    foreach (var c in rows) { c.Status = ClientStatus.Inactive; c.UpdatedAt = DateTime.UtcNow; }
    await _db.SaveChangesAsync();
    return rows.Count;
}

public async Task<int> BulkDeleteAsync(IEnumerable<int> ids, int actor)
{
    var list = ids.Distinct().ToList();
    var rows = await _db.Clients
        .Include(c => c.Documents)
        .Where(c => list.Contains(c.Id) && c.Status == ClientStatus.Inactive)
        .ToListAsync();

    foreach (var c in rows)
    {
        foreach (var d in c.Documents)
        {
            var abs = Path.Combine(_env.WebRootPath, d.FilePath.Replace('/', Path.DirectorySeparatorChar));
            try { if (File.Exists(abs)) File.Delete(abs); } catch { }
        }
        var slug = Slugify(c.FullName) + "-" + c.Id;
        var folder = Path.Combine(_env.WebRootPath, "uploads", slug);
        try { if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true); } catch { }
    }
    _db.Clients.RemoveRange(rows);
    await _db.SaveChangesAsync();
    return rows.Count;
}

public async Task<IReadOnlyList<EmployeeOption>> GetAssignableEmployeesAsync()
{
    return await _db.Users
        .Where(u => u.IsActive &&
                    (u.Role == UserRole.Admin || u.Role == UserRole.Hod ||
                     u.Role == UserRole.Manager || u.Role == UserRole.Receptionist))
        .OrderBy(u => u.FullName)
        .Select(u => new EmployeeOption(
            u.Id,
            u.FullName,
            u.Role.ToString()))
        .ToListAsync();
}

}