using Microsoft.EntityFrameworkCore;
using Multiverse.Web.Data;
using Multiverse.Web.Interfaces;
using Multiverse.Web.Models;

namespace Multiverse.Web.Services;

public class DocumentService : IDocumentService
{
    private const long MaxBytes = 10 * 1024 * 1024; // 10 MB
    private static readonly HashSet<string> AllowedExt =
        new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".jpg", ".jpeg", ".png" };

    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public DocumentService(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<IReadOnlyList<ClientDocument>> GetForClientAsync(int clientId) =>
        await _db.ClientDocuments
            .AsNoTracking()
            .Where(d => d.ClientId == clientId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();

    public Task<ClientDocument?> GetByIdAsync(int id) =>
        _db.ClientDocuments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);

    public async Task<ClientDocument> UploadAsync(Client client, DocumentType type, IFormFile file, int uploadedByUserId)
    {
        if (file is null || file.Length == 0)
            throw new InvalidOperationException("Empty file.");
        if (file.Length > MaxBytes)
            throw new InvalidOperationException("File exceeds 10 MB limit.");

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExt.Contains(ext))
            throw new InvalidOperationException("Only PDF, JPG, JPEG and PNG are allowed.");

        // Sanitized client folder: "rajesh-sharma-42"
        var slug = Slugify(client.FullName) + "-" + client.Id;
        var relDir = Path.Combine("uploads", slug, type.FolderName()).Replace("\\", "/");
        var absDir = Path.Combine(_env.WebRootPath, relDir);
        Directory.CreateDirectory(absDir);

        var storedName = Guid.NewGuid().ToString("N") + ext.ToLowerInvariant();
        var absFile = Path.Combine(absDir, storedName);
        await using (var fs = File.Create(absFile))
            await file.CopyToAsync(fs);

        var doc = new ClientDocument
        {
            ClientId = client.Id,
            Type = type,
            FilePath = relDir + "/" + storedName,
            OriginalName = Path.GetFileName(file.FileName),
            ContentType = file.ContentType,
            SizeBytes = file.Length,
            UploadedAt = DateTime.UtcNow,
            UploadedByUserId = uploadedByUserId
        };
        _db.ClientDocuments.Add(doc);
        await _db.SaveChangesAsync();
        return doc;
    }

    public async Task<bool> DeleteAsync(int documentId, int requestingUserId)
    {
        var doc = await _db.ClientDocuments.FirstOrDefaultAsync(d => d.Id == documentId);
        if (doc is null) return false;

        // Remove file from disk (best effort — orphan file isn't fatal)
        var abs = Path.Combine(_env.WebRootPath, doc.FilePath.Replace('/', Path.DirectorySeparatorChar));
        try { if (File.Exists(abs)) File.Delete(abs); } catch { /* swallow */ }

        _db.ClientDocuments.Remove(doc);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<string?> GetPhysicalPathAsync(int documentId)
    {
        var doc = await _db.ClientDocuments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == documentId);
        if (doc is null) return null;
        var abs = Path.Combine(_env.WebRootPath, doc.FilePath.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(abs) ? abs : null;
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
}