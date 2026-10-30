using Multiverse.Web.Models;

namespace Multiverse.Web.Interfaces;

public interface IUserService
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<IReadOnlyList<User>> GetAllAsync();
}