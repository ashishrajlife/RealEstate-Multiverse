using Microsoft.EntityFrameworkCore;
using Multiverse.Web.Data;
using Multiverse.Web.Interfaces;
using Multiverse.Web.Models;

namespace Multiverse.Web.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    public UserService(AppDbContext db) => _db = db;

    public Task<User?> GetByEmailAsync(string email) =>
        _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);

    public Task<User?> GetByIdAsync(int id) =>
        _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

    public async Task<IReadOnlyList<User>> GetAllAsync() =>
        await _db.Users.AsNoTracking().ToListAsync();
}