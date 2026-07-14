using FluentAssertions;
using SchoolManagementSystem.Domain.Entities;

namespace SchoolManagementSystem.Domain.Tests.Entities;

public class WalletAdjustmentTests
{
    [Fact]
    public void Constructor_SetsAllPropertiesCorrectly()
    {
        var userId = Guid.NewGuid();

        var adjustment = new WalletAdjustment(userId, 100m, 150m, "Allowance");

        adjustment.UserId.Should().Be(userId);
        adjustment.Amount.Should().Be(100m);
        adjustment.ResultingBalance.Should().Be(150m);
        adjustment.Reason.Should().Be("Allowance");
    }

    [Fact]
    public void Constructor_AssignsNonEmptyId()
    {
        var adjustment = new WalletAdjustment(Guid.NewGuid(), 50m, 50m, "Credit");

        adjustment.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_SetsCreatedAtUtcToCurrentTime()
    {
        var before = DateTime.UtcNow;

        var adjustment = new WalletAdjustment(Guid.NewGuid(), 50m, 50m, "Credit");

        var after = DateTime.UtcNow;
        adjustment.CreatedAtUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Constructor_NullReason_IsAllowed()
    {
        var act = () => new WalletAdjustment(Guid.NewGuid(), 50m, 50m, null);

        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_NegativeAmount_IsStoredAsIs()
    {
        var adjustment = new WalletAdjustment(Guid.NewGuid(), -50m, 0m, "Debit");

        adjustment.Amount.Should().Be(-50m);
    }
}