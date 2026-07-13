using SchoolManagementSystem.Domain.Enums;

namespace SchoolManagementSystem.Application.DTOs;

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    decimal WalletBalance,
    UserStatus Status);

public record WalletAdjustmentRequestDto(decimal Amount, string? Reason);

public record WalletAdjustmentResponseDto(
    Guid Id,
    Guid UserId,
    decimal Amount,
    decimal ResultingBalance,
    string? Reason,
    DateTime CreatedAtUtc);