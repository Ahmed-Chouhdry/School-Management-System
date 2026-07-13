using SchoolManagementSystem.Application.Common;
using SchoolManagementSystem.Application.DTOs;

namespace SchoolManagementSystem.Application.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync(CancellationToken ct = default);
    Task<Result<UserDto>> GetUserByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<WalletAdjustmentResponseDto>> AdjustWalletAsync(Guid userId, WalletAdjustmentRequestDto request, CancellationToken ct = default);
}