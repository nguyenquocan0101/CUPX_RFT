using AutomaticBrewingCoffee.Domain.Context;
using AutomaticBrewingCoffee.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Services.Interfaces;
using Services.Tests.AuthService.TestData;
using Services.Tests.TestBase;

namespace Services.Tests.AuthService;

[Collection("Test Collection")]
public class LoginTest(StartUpTestBase fixture) : IAsyncLifetime
{
    private readonly AutoBrewingBeContext _dbContext = fixture.DbContext;
    private readonly Func<Task> _resetDatabase = fixture.ResetDatabaseAsync;
    private readonly IAuthService _authService = fixture.ServiceProvider.GetRequiredService<IAuthService>();

    public async Task InitializeAsync()
    {
    }

    public async Task DisposeAsync() => await _resetDatabase();


    [Fact]
    public async Task Handle_LoginWithCorrectAccount_ReturnsJwtToken()
    {
        // Arrange
        var user = await TestDataLogin.CreateTestUser(_dbContext);
        var account = TestDataLogin.CreateLoginDto(user.Email, "admin");

        var service = _authService.Login(account);

        // Act
        var result = await service;

        // Assert
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    [Fact]
    public async Task Handle_LoginWithInCorrectUsername_ReturnsForbidden()
    {
        // Arrange
        var account = TestDataLogin.CreateLoginDto("!admin", "admin");

        var service = _authService.Login(account);

        // Act
        var result = await service;

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task Handle_LoginWithInCorrectPassword_ReturnsForbidden()
    {
        // Arrange
        var account = TestDataLogin.CreateLoginDto("admin", "!admin");

        var service = _authService.Login(account);

        // Act
        var result = await service;

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task Handle_LoginWithIncorrectAccount_ReturnsForbidden()
    {
        // Arrange
        var account = TestDataLogin.CreateLoginDto("!admin", "!admin");

        var service = _authService.Login(account);

        // Act
        var result = await service;

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task Handle_LoginWithEmptyInput_ReturnsForbidden()
    {
        // Arrange
        var account = TestDataLogin.CreateLoginDto("", "");

        var service = _authService.Login(account);

        // Act
        var result = await service;

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
    }
}