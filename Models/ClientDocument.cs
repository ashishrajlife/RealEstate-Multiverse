namespace Multiverse.Web.Models;

public class ClientDocument
{
    public int Id { get; set; }

    public int ClientId { get; set; }
    public Client Client { get; set; } = default!;

    public DocumentType Type { get; set; }

    // Relative path under wwwroot, e.g. "uploads/rajesh-sharma-42/Aadhaar/abc123.pdf"
    public string FilePath { get; set; } = string.Empty;

    public string OriginalName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public int UploadedByUserId { get; set; }
}