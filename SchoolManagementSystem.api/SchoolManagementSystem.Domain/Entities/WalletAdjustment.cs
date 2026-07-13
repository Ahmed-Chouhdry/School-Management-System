namespace SchoolManagementSystem.Domain.Entities;

public class WalletAdjustment
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }
    public decimal ResultingBalance { get; private set; }
    public string? Reason { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private WalletAdjustment() { }

    public WalletAdjustment(Guid userId, decimal amount, decimal resultingBalance, string? reason)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Amount = amount;
        ResultingBalance = resultingBalance;
        Reason = reason;
        CreatedAtUtc = DateTime.UtcNow;
    }
}