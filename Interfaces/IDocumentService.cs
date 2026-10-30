using Multiverse.Web.Models;

namespace Multiverse.Web.Interfaces;

public interface IDocumentService
{
    Task<IReadOnlyList<ClientDocument>> GetForClientAsync(int clientId);
    Task<ClientDocument?> GetByIdAsync(int id);

    /// <summary>
    /// Saves the file under wwwroot/uploads/{sanitized-client-folder}/{type}/ and persists a ClientDocument row.
    /// </summary>
    Task<ClientDocument> UploadAsync(Client client, DocumentType type, IFormFile file, int uploadedByUserId);

    Task<bool> DeleteAsync(int documentId, int requestingUserId);

    /// <summary>Absolute physical path on disk (for FileStreamResult).</summary>
    Task<string?> GetPhysicalPathAsync(int documentId);
}