using SchoolManagementSystem.Domain.Entities;

namespace SchoolManagementSystem.Application.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync(CancellationToken ct = default);

    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddWalletAdjustmentAsync(WalletAdjustment adjustment, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}