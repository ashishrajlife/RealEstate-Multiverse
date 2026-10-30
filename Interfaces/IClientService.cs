using Multiverse.Web.Models;
using Multiverse.Web.ViewModels;

namespace Multiverse.Web.Interfaces;

public class ClientFilter
{
    public string? Q { get; set; }
    public ClientStatus? Status { get; set; }
    public IndianState? State { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}

public interface IClientService
{
    // existing
    Task<IReadOnlyList<Client>> GetAllAsync();
    Task<Client?> GetByIdAsync(int id);
    Task<Client> CreateAsync(Client client, int createdByUserId);
    Task<bool> UpdateAsync(Client client);
    Task<bool> DeactivateAsync(int id);
    Task<bool> ReactivateAsync(int id);
    Task<bool> DeleteAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? exceptId = null);
    Task<int> CountAsync();
    Task<int> CountActiveAsync();
    Task<int> CountNewThisWeekAsync();

    // new
    Task<(IReadOnlyList<Client> Rows, int Total)> QueryAsync(ClientFilter filter, int page, int pageSize);
    Task<IReadOnlyList<Client>> GetForExportAsync(ClientFilter filter);

    Task<int> BulkActivateAsync(IEnumerable<int> ids, int actorUserId);
    Task<int> BulkDeactivateAsync(IEnumerable<int> ids, int actorUserId);
    Task<int> BulkDeleteAsync(IEnumerable<int> ids, int actorUserId);
    
    Task<IReadOnlyList<EmployeeOption>> GetAssignableEmployeesAsync();
}