using SchoolManagementSystem.Domain.Enums;
using SchoolManagementSystem.Domain.Exceptions;

namespace SchoolManagementSystem.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public UserRole Role { get; private set; }
    public decimal WalletBalance { get; private set; }
    public UserStatus Status { get; private set; }

    private readonly List<WalletAdjustment> _walletAdjustments = new();
    public IReadOnlyCollection<WalletAdjustment> WalletAdjustments => _walletAdjustments.AsReadOnly();

    private User() { }

    public User(string firstName, string lastName, string email, UserRole role, UserStatus status = UserStatus.Active)
    {
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Role = role;
        Status = status;
        WalletBalance = 0;
    }

    public WalletAdjustment ApplyWalletAdjustment(decimal amount, string reason)
    {
        if (amount == 0)
            throw new InvalidWalletAdjustmentException("Adjustment amount cannot be zero.");

        var newBalance = WalletBalance + amount;

        if (newBalance < 0)
            throw new InsufficientWalletBalanceException(WalletBalance, amount);

        WalletBalance = newBalance;

        var adjustment = new WalletAdjustment(Id, amount, newBalance, reason);
        _walletAdjustments.Add(adjustment);

        return adjustment;
    }
}
