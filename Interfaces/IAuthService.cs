using Multiverse.Web.Models;

namespace Multiverse.Web.Interfaces;

public interface IAuthService
{
    Task<User?> ValidateAsync(string email, string password);
}