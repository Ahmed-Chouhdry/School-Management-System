using FluentAssertions;
using Moq;
using SchoolManagementSystem.Application.Common;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Application.Services;
using SchoolManagementSystem.Domain.Entities;
using SchoolManagementSystem.Domain.Enums;

namespace SchoolManagementSystem.Application.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepository = new Mock<IUserRepository>();
        _userService = new UserService(_userRepository.Object);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllUsersMappedToDto()
    {
        var users = new List<User> { CreateUser(), CreateUser("Sara", "Khan", "sara@school.edu", UserRole.Admin) };

        _userRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await _userService.GetAllUsersAsync(CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].FirstName.Should().Be("Ahmed");
        result[1].FirstName.Should().Be("Sara");
        result.Should().AllSatisfy(dto => dto.Id.Should().NotBeEmpty());
    }

    [Fact]
    public async Task GetAllUsersAsync_NoUsers_ReturnsEmptyList()
    {
        _userRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        var result = await _userService.GetAllUsersAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllUsersAsync_MapsWalletBalanceCorrectly()
    {
        var user = CreateUser();
        user.ApplyWalletAdjustment(100m, "Test credit");

        _userRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });

        var result = await _userService.GetAllUsersAsync(CancellationToken.None);

        result.Single().WalletBalance.Should().Be(100m);
    }


    [Fact]
    public async Task GetUserByIdAsync_ExistingUser_ReturnsSuccessWithMappedDto()
    {
        var user = CreateUser();

        _userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _userService.GetUserByIdAsync(user.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(user.Id);
        result.Value.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_NonExistentUser_ReturnsNotFoundFailure()
    {
        var userId = Guid.NewGuid();

        _userRepository
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _userService.GetUserByIdAsync(userId, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        result.Error.Should().Contain(userId.ToString());
    }

    [Fact]
    public async Task GetUserByIdAsync_CallsRepositoryWithCorrectId()
    {
        var userId = Guid.NewGuid();

        _userRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await _userService.GetUserByIdAsync(userId, CancellationToken.None);

        _userRepository.Verify(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AdjustWalletAsync_UserNotFound_ReturnsNotFoundFailure_AndDoesNotSave()
    {
        var userId = Guid.NewGuid();
        var request = new WalletAdjustmentRequestDto(50m, "Top-up");

        _userRepository
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _userService.AdjustWalletAsync(userId, request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);

        _userRepository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AdjustWalletAsync_PositiveAmount_ReturnsSuccessAndSaves()
    {
        var user = CreateUser();
        var request = new WalletAdjustmentRequestDto(100m, "Allowance");

        _userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _userService.AdjustWalletAsync(user.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Amount.Should().Be(100m);
        result.Value.ResultingBalance.Should().Be(100m);
        result.Value.UserId.Should().Be(user.Id);

        _userRepository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AdjustWalletAsync_NegativeAmountWithinBalance_ReturnsSuccessAndDecreasesBalance()
    {
        var user = CreateUser();
        user.ApplyWalletAdjustment(200m, "Initial balance");

        var request = new WalletAdjustmentRequestDto(-50m, "Purchase");

        _userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _userService.AdjustWalletAsync(user.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ResultingBalance.Should().Be(150m);
    }

    [Fact]
    public async Task AdjustWalletAsync_WouldGoNegative_ReturnsValidationFailure_AndDoesNotSave()
    {
        var user = CreateUser();
        var request = new WalletAdjustmentRequestDto(-10m, "Overdraw attempt");

        _userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _userService.AdjustWalletAsync(user.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Validation);

        _userRepository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AdjustWalletAsync_ZeroAmount_ReturnsValidationFailure_AndDoesNotSave()
    {
        var user = CreateUser();
        var request = new WalletAdjustmentRequestDto(0m, "Invalid");

        _userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _userService.AdjustWalletAsync(user.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Validation);

        _userRepository.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AdjustWalletAsync_NullReason_DoesNotThrow_UsesEmptyString()
    {
        var user = CreateUser();
        var request = new WalletAdjustmentRequestDto(25m, null);

        _userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _userService.AdjustWalletAsync(user.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Reason.Should().BeEmpty();
    }

    [Fact]
    public async Task AdjustWalletAsync_MultipleAdjustments_EachAppliesCorrectlyInSequence()
    {
        var user = CreateUser();

        _userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _userService.AdjustWalletAsync(user.Id, new WalletAdjustmentRequestDto(100m, "Credit 1"), CancellationToken.None);
        await _userService.AdjustWalletAsync(user.Id, new WalletAdjustmentRequestDto(-30m, "Debit 1"), CancellationToken.None);
        var finalResult = await _userService.AdjustWalletAsync(user.Id, new WalletAdjustmentRequestDto(20m, "Credit 2"), CancellationToken.None);

        finalResult.Value!.ResultingBalance.Should().Be(90m); // 100 - 30 + 20
        _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
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