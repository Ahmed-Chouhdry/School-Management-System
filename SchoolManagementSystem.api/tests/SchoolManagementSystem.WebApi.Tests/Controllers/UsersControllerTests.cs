using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SchoolManagementSystem.Application.Common;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Domain.Enums;
using SchoolManagementSystem.WebApi.Controllers;

namespace SchoolManagementSystem.WebApi.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _controller = new UsersController(_mockUserService.Object);
    }


    [Fact]
    public async Task GetAll_ReturnsOkWithUserList()
    {
        var users = new List<UserDto>
        {
            new(Guid.NewGuid(), "Ahmed", "Javed", "ahmed@school.edu", UserRole.Student, 100m, UserStatus.Active),
            new(Guid.NewGuid(), "Sara", "Khan", "sara@school.edu", UserRole.Admin, 0m, UserStatus.Active)
        };

        _mockUserService
            .Setup(s => s.GetAllUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await _controller.GetAll(CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task GetAll_NoUsers_ReturnsOkWithEmptyList()
    {
        _mockUserService
            .Setup(s => s.GetAllUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserDto>());

        var result = await _controller.GetAll(CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<List<UserDto>>().Which.Should().BeEmpty();
    }

    [Fact]
    public async Task GetById_ExistingUser_ReturnsOkWithUser()
    {
        var userId = Guid.NewGuid();
        var user = new UserDto(userId, "Ahmed", "Javed", "ahmed@school.edu", UserRole.Student, 100m, UserStatus.Active);

        _mockUserService
            .Setup(s => s.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(user));

        var result = await _controller.GetById(userId, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(user);
    }

    [Fact]
    public async Task GetById_NonExistentUser_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();

        _mockUserService
            .Setup(s => s.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Failure($"User '{userId}' not found.", ResultErrorType.NotFound));

        var result = await _controller.GetById(userId, CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_CallsServiceWithCorrectId()
    {
        var userId = Guid.NewGuid();
        _mockUserService
            .Setup(s => s.GetUserByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Failure("not found", ResultErrorType.NotFound));

        await _controller.GetById(userId, CancellationToken.None);

        _mockUserService.Verify(s => s.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task AdjustWallet_ValidPositiveAmount_ReturnsCreatedWithResult()
    {
        var userId = Guid.NewGuid();
        var request = new WalletAdjustmentRequestDto(50m, "Top-up");
        var response = new WalletAdjustmentResponseDto(Guid.NewGuid(), userId, 50m, 150m, "Top-up", DateTime.UtcNow);

        _mockUserService
            .Setup(s => s.AdjustWalletAsync(userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<WalletAdjustmentResponseDto>.Success(response));

        var result = await _controller.AdjustWallet(userId, request, CancellationToken.None);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().Be(response);
    }

    [Fact]
    public async Task AdjustWallet_UserNotFound_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var request = new WalletAdjustmentRequestDto(50m, "Top-up");

        _mockUserService
            .Setup(s => s.AdjustWalletAsync(userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<WalletAdjustmentResponseDto>.Failure(
                $"User '{userId}' not found.", ResultErrorType.NotFound));

        var result = await _controller.AdjustWallet(userId, request, CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AdjustWallet_WouldGoNegative_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var request = new WalletAdjustmentRequestDto(-500m, "Overdraw attempt");

        _mockUserService
            .Setup(s => s.AdjustWalletAsync(userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<WalletAdjustmentResponseDto>.Failure(
                "Cannot apply adjustment of -500. Resulting balance would be negative.", ResultErrorType.Validation));

        var result = await _controller.AdjustWallet(userId, request, CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AdjustWallet_ZeroAmount_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var request = new WalletAdjustmentRequestDto(0m, "Invalid");

        _mockUserService
            .Setup(s => s.AdjustWalletAsync(userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<WalletAdjustmentResponseDto>.Failure(
                "Adjustment amount cannot be zero.", ResultErrorType.Validation));

        var result = await _controller.AdjustWallet(userId, request, CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AdjustWallet_CallsServiceExactlyOnceWithCorrectArguments()
    {
        var userId = Guid.NewGuid();
        var request = new WalletAdjustmentRequestDto(25m, "Test");

        _mockUserService
            .Setup(s => s.AdjustWalletAsync(It.IsAny<Guid>(), It.IsAny<WalletAdjustmentRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<WalletAdjustmentResponseDto>.Failure("irrelevant", ResultErrorType.NotFound));

        await _controller.AdjustWallet(userId, request, CancellationToken.None);

        _mockUserService.Verify(
            s => s.AdjustWalletAsync(userId, request, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}