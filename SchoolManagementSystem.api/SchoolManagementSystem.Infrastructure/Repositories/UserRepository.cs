using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Domain.Entities;
using SchoolManagementSystem.Infrastructure.Persistence;

namespace SchoolManagementSystem.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Users.AsNoTracking().ToListAsync(ct);

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Users
            .Include(u => u.WalletAdjustments)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        var trackedUsers = _context.ChangeTracker.Entries<User>()
            .Select(e => new { e.Entity.Id, e.State })
            .ToList();

        Console.WriteLine($"About to save. Tracked users: {string.Join(", ", trackedUsers.Select(u => $"{u.Id} ({u.State})"))}");

        await _context.SaveChangesAsync(ct);
    }

    public async Task AddWalletAdjustmentAsync(WalletAdjustment adjustment, CancellationToken ct = default)
    {
        await _context.WalletAdjustments.AddAsync(adjustment, ct);
    }
}