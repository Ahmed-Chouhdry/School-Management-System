using SchoolManagementSystem.Application.Common;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Domain.Exceptions;

namespace SchoolManagementSystem.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserDto>> GetAllUsersAsync(CancellationToken ct = default)
    {
        var users = await _userRepository.GetAllAsync(ct);
        return users.Select(MapToDto).ToList();
    }

    public async Task<Result<UserDto>> GetUserByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user is null)
            return Result<UserDto>.Failure($"User '{id}' not found.", ResultErrorType.NotFound);

        return Result<UserDto>.Success(MapToDto(user));
    }

    public async Task<Result<WalletAdjustmentResponseDto>> AdjustWalletAsync(
        Guid userId, WalletAdjustmentRequestDto request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user is null)
            return Result<WalletAdjustmentResponseDto>.Failure($"User '{userId}' not found.", ResultErrorType.NotFound);

        try
        {
            var adjustment = user.ApplyWalletAdjustment(request.Amount, request.Reason ?? string.Empty);
            await _userRepository.AddWalletAdjustmentAsync(adjustment, ct);
            await _userRepository.SaveChangesAsync(ct);


            var dto = new WalletAdjustmentResponseDto(
                adjustment.Id, adjustment.UserId, adjustment.Amount,
                adjustment.ResultingBalance, adjustment.Reason, adjustment.CreatedAtUtc);

            return Result<WalletAdjustmentResponseDto>.Success(dto);
        }
        catch (DomainException ex)
        {
            return Result<WalletAdjustmentResponseDto>.Failure(ex.Message, ResultErrorType.Validation);
        }
    }

    private static UserDto MapToDto(Domain.Entities.User u) =>
        new(u.Id, u.FirstName, u.LastName, u.Email, u.Role, u.WalletBalance, u.Status);
}