using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Domain.Entities;
using SchoolManagementSystem.Domain.Enums;
using SchoolManagementSystem.Domain.Exceptions;
using SchoolManagementSystem.Infrastructure.Persistence;
using SchoolManagementSystem.Infrastructure.Repositories;
using FluentAssertions;

namespace SchoolManagementSystem.Tests.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new UserRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers_WithoutTracking()
    {
        var user1 = new User("Alice", "Smith", "alice@school.com", UserRole.Student);
        var user2 = new User("Bob", "Jones", "bob@school.com", UserRole.Admin);

        await _context.Users.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        var result = await _repository.GetAllAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(u => u.FirstName == "Alice");
        result.Should().Contain(u => u.FirstName == "Bob");

        _context.ChangeTracker.Entries<User>().Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUserWithWalletAdjustments_WhenUserExists()
    {
        var user = new User("Charlie", "Brown", "charlie@school.com", UserRole.Student);

        var adjustment1 = user.ApplyWalletAdjustment(100.00m, "Initial Deposit");
        var adjustment2 = user.ApplyWalletAdjustment(-30.00m, "Book Purchase");

        await _context.Users.AddAsync(user);
        await _context.WalletAdjustments.AddRangeAsync(adjustment1, adjustment2);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.WalletBalance.Should().Be(70.00m);
        result.WalletAdjustments.Should().HaveCount(2);
        result.WalletAdjustments.Should().Contain(wa => wa.Amount == 100.00m);
    }

    [Fact]
    public async Task AddWalletAdjustmentAsync_ShouldAddAdjustmentToDbSet()
    {
        var user = new User("Dave", "Miller", "dave@school.com", UserRole.Student);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var adjustment = user.ApplyWalletAdjustment(50.00m, "Scholarship Reward");

        await _repository.AddWalletAdjustmentAsync(adjustment);
        await _repository.SaveChangesAsync();

        var dbAdjustment = await _context.WalletAdjustments.FindAsync(adjustment.Id);
        dbAdjustment.Should().NotBeNull();
        dbAdjustment!.Amount.Should().Be(50.00m);
        dbAdjustment.UserId.Should().Be(user.Id);
    }

    [Theory]
    [InlineData(0)]
    public void ApplyWalletAdjustment_ShouldThrow_WhenAmountIsZero(decimal invalidAmount)
    {
        var user = new User("Eve", "Adams", "eve@school.com", UserRole.Student);

        var act = () => user.ApplyWalletAdjustment(invalidAmount, "No-op");

        act.Should().Throw<InvalidWalletAdjustmentException>()
           .WithMessage("Adjustment amount cannot be zero.");
    }

    [Fact]
    public void ApplyWalletAdjustment_ShouldThrow_WhenBalanceGoesNegative()
    {
        var user = new User("Frank", "Wright", "frank@school.com", UserRole.Student);

        var act = () => user.ApplyWalletAdjustment(-10.00m, "Overdraft attempt");

        act.Should().Throw<InsufficientWalletBalanceException>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}