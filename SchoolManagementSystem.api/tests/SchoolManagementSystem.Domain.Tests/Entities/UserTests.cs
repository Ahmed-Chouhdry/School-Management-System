using FluentAssertions;
using SchoolManagementSystem.Domain.Entities;
using SchoolManagementSystem.Domain.Enums;
using SchoolManagementSystem.Domain.Exceptions;

namespace SchoolManagementSystem.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_SetsAllPropertiesCorrectly()
    {
        var user = CreateUser("Sara", "Khan", "sara@school.edu", UserRole.Admin, UserStatus.Active);

        user.FirstName.Should().Be("Sara");
        user.LastName.Should().Be("Khan");
        user.Email.Should().Be("sara@school.edu");
        user.Role.Should().Be(UserRole.Admin);
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void Constructor_AssignsNonEmptyId()
    {
        var user = CreateUser();

        user.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_TwoUsers_HaveDifferentIds()
    {
        var user1 = CreateUser();
        var user2 = CreateUser();

        user1.Id.Should().NotBe(user2.Id);
    }

    [Fact]
    public void Constructor_InitializesWalletBalanceToZero()
    {
        var user = CreateUser();

        user.WalletBalance.Should().Be(0m);
    }

    [Fact]
    public void Constructor_InitializesEmptyWalletAdjustmentsCollection()
    {
        var user = CreateUser();

        user.WalletAdjustments.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_DefaultStatus_IsActive()
    {
        var user = new User("Ahmed", "Javed", "ahmed@school.edu", UserRole.Student);

        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void ApplyWalletAdjustment_PositiveAmount_IncreasesBalance()
    {
        var user = CreateUser();

        user.ApplyWalletAdjustment(100m, "Allowance");

        user.WalletBalance.Should().Be(100m);
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(50.5, 50.5)]
    [InlineData(1000, 1000)]
    [InlineData(0.01, 0.01)]
    public void ApplyWalletAdjustment_VariousPositiveAmounts_IncreasesBalanceByExactAmount(decimal amount, decimal expectedBalance)
    {
        var user = CreateUser();

        user.ApplyWalletAdjustment(amount, "Credit");

        user.WalletBalance.Should().Be(expectedBalance);
    }

    [Fact]
    public void ApplyWalletAdjustment_MultiplePositiveAdjustments_AccumulateCorrectly()
    {
        var user = CreateUser();

        user.ApplyWalletAdjustment(100m, "Credit 1");
        user.ApplyWalletAdjustment(50m, "Credit 2");
        user.ApplyWalletAdjustment(25m, "Credit 3");

        user.WalletBalance.Should().Be(175m);
    }

    [Fact]
    public void ApplyWalletAdjustment_NegativeAmountWithinBalance_DecreasesBalance()
    {
        var user = CreateUser();
        user.ApplyWalletAdjustment(200m, "Initial balance");

        user.ApplyWalletAdjustment(-50m, "Purchase");

        user.WalletBalance.Should().Be(150m);
    }

    [Fact]
    public void ApplyWalletAdjustment_NegativeAmountExactlyEqualToBalance_ResultsInZero()
    {
        var user = CreateUser();
        user.ApplyWalletAdjustment(100m, "Initial balance");

        user.ApplyWalletAdjustment(-100m, "Full withdrawal");

        user.WalletBalance.Should().Be(0m);
    }

    [Fact]
    public void ApplyWalletAdjustment_MultipleNegativeAdjustments_AccumulateCorrectly()
    {
        var user = CreateUser();
        user.ApplyWalletAdjustment(500m, "Initial balance");

        user.ApplyWalletAdjustment(-100m, "Debit 1");
        user.ApplyWalletAdjustment(-150m, "Debit 2");

        user.WalletBalance.Should().Be(250m);
    }

    [Fact]
    public void ApplyWalletAdjustment_WouldResultInNegativeBalance_ThrowsInsufficientWalletBalanceException()
    {
        var user = CreateUser(); // balance = 0

        var act = () => user.ApplyWalletAdjustment(-10m, "Overdraw attempt");

        act.Should().Throw<InsufficientWalletBalanceException>();
    }

    [Fact]
    public void ApplyWalletAdjustment_WouldResultInNegativeBalance_DoesNotChangeBalance()
    {
        var user = CreateUser();
        user.ApplyWalletAdjustment(50m, "Initial balance");

        try { user.ApplyWalletAdjustment(-100m, "Overdraw attempt"); } catch (InsufficientWalletBalanceException) { }

        user.WalletBalance.Should().Be(50m); // unchanged
    }

    [Fact]
    public void ApplyWalletAdjustment_WouldResultInNegativeBalance_DoesNotRecordAdjustment()
    {
        var user = CreateUser();
        user.ApplyWalletAdjustment(50m, "Initial balance");

        try { user.ApplyWalletAdjustment(-100m, "Overdraw attempt"); } catch (InsufficientWalletBalanceException) { }

        user.WalletAdjustments.Should().HaveCount(1); // only the initial credit, not the failed debit
    }

    [Fact]
    public void ApplyWalletAdjustment_ExceptionMessage_ContainsCurrentBalanceAndAttemptedAmount()
    {
        var user = CreateUser();
        user.ApplyWalletAdjustment(50m, "Initial balance");

        var act = () => user.ApplyWalletAdjustment(-200m, "Overdraw attempt");

        act.Should().Throw<InsufficientWalletBalanceException>()
            .WithMessage("*50*")
            .WithMessage("*-200*");
    }

    [Fact]
    public void ApplyWalletAdjustment_ZeroAmount_ThrowsInvalidWalletAdjustmentException()
    {
        var user = CreateUser();

        var act = () => user.ApplyWalletAdjustment(0m, "Invalid");

        act.Should().Throw<InvalidWalletAdjustmentException>();
    }

    [Fact]
    public void ApplyWalletAdjustment_ZeroAmount_DoesNotRecordAdjustment()
    {
        var user = CreateUser();

        try { user.ApplyWalletAdjustment(0m, "Invalid"); } catch (InvalidWalletAdjustmentException) { }

        user.WalletAdjustments.Should().BeEmpty();
    }

    [Fact]
    public void ApplyWalletAdjustment_SuccessfulAdjustment_IsRecordedInWalletAdjustments()
    {
        var user = CreateUser();

        user.ApplyWalletAdjustment(100m, "Allowance");

        user.WalletAdjustments.Should().HaveCount(1);
    }

    [Fact]
    public void ApplyWalletAdjustment_MultipleSuccessfulAdjustments_AllAreRecorded()
    {
        var user = CreateUser();

        user.ApplyWalletAdjustment(100m, "Credit 1");
        user.ApplyWalletAdjustment(-30m, "Debit 1");
        user.ApplyWalletAdjustment(50m, "Credit 2");

        user.WalletAdjustments.Should().HaveCount(3);
    }

    [Fact]
    public void ApplyWalletAdjustment_RecordedAdjustment_HasCorrectUserId()
    {
        var user = CreateUser();

        user.ApplyWalletAdjustment(100m, "Allowance");

        user.WalletAdjustments.Single().UserId.Should().Be(user.Id);
    }

    [Fact]
    public void ApplyWalletAdjustment_RecordedAdjustment_HasCorrectAmountAndReason()
    {
        var user = CreateUser();

        user.ApplyWalletAdjustment(75m, "Birthday gift");

        var adjustment = user.WalletAdjustments.Single();
        adjustment.Amount.Should().Be(75m);
        adjustment.Reason.Should().Be("Birthday gift");
    }

    [Fact]
    public void ApplyWalletAdjustment_RecordedAdjustment_HasResultingBalanceMatchingUserBalance()
    {
        var user = CreateUser();
        user.ApplyWalletAdjustment(200m, "Initial");

        user.ApplyWalletAdjustment(-50m, "Purchase");

        var latestAdjustment = user.WalletAdjustments.Last();
        latestAdjustment.ResultingBalance.Should().Be(user.WalletBalance);
        latestAdjustment.ResultingBalance.Should().Be(150m);
    }

    [Fact]
    public void ApplyWalletAdjustment_RecordedAdjustment_HasUniqueId()
    {
        var user = CreateUser();

        user.ApplyWalletAdjustment(50m, "Credit 1");
        user.ApplyWalletAdjustment(50m, "Credit 2");

        var adjustments = user.WalletAdjustments.ToList();
        adjustments[0].Id.Should().NotBe(adjustments[1].Id);
    }

    [Fact]
    public void ApplyWalletAdjustment_RecordedAdjustment_HasCreatedAtUtcCloseToNow()
    {
        var user = CreateUser();
        var before = DateTime.UtcNow;

        user.ApplyWalletAdjustment(50m, "Credit");

        var after = DateTime.UtcNow;
        var adjustment = user.WalletAdjustments.Single();
        adjustment.CreatedAtUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void ApplyWalletAdjustment_ReturnsTheSameAdjustmentThatWasRecorded()
    {
        var user = CreateUser();

        var returnedAdjustment = user.ApplyWalletAdjustment(100m, "Allowance");

        user.WalletAdjustments.Single().Should().BeSameAs(returnedAdjustment);
    }

    [Fact]
    public void ApplyWalletAdjustment_EmptyReason_IsAllowed()
    {
        var user = CreateUser();

        var act = () => user.ApplyWalletAdjustment(50m, string.Empty);

        act.Should().NotThrow();
    }

    [Fact]
    public void ApplyWalletAdjustment_VerySmallDecimalAmount_IsHandledPrecisely()
    {
        var user = CreateUser();

        user.ApplyWalletAdjustment(0.01m, "Rounding test");

        user.WalletBalance.Should().Be(0.01m);
    }

    [Fact]
    public void ApplyWalletAdjustment_LargeAmount_IsHandledCorrectly()
    {
        var user = CreateUser();

        user.ApplyWalletAdjustment(1_000_000m, "Large credit");

        user.WalletBalance.Should().Be(1_000_000m);
    }

    [Fact]
    public void ApplyWalletAdjustment_ExactBoundary_LeavesBalanceAtZeroNotNegative()
    {
        var user = CreateUser();
        user.ApplyWalletAdjustment(100m, "Initial");

        var act = () => user.ApplyWalletAdjustment(-100m, "Exact drawdown");

        act.Should().NotThrow();
        user.WalletBalance.Should().Be(0m);
    }

    private static User CreateUser(
    string firstName = "Ahmed",
    string lastName = "Javed",
    string email = "ahmed@school.edu",
    UserRole role = UserRole.Student,
    UserStatus status = UserStatus.Active)
    {
        return new User(firstName, lastName, email, role, status);
    }
}