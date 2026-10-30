using Microsoft.EntityFrameworkCore;
using Multiverse.Web.Data;
using Multiverse.Web.Interfaces;
using Multiverse.Web.Models;

namespace Multiverse.Web.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    public AuthService(AppDbContext db) => _db = db;

    public async Task<User?> ValidateAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email && u.Password == password && u.IsActive);
    }
}