using AutomaticBrewingCoffee.Domain.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Services.Dtos.Auth;
using Services.Interfaces;
using Services.Tests.AuthService.TestData;
using Services.Tests.TestBase;

namespace Services.Tests.AuthService;

[Collection("Test Collection")]
public class RefreshTest(StartUpTestBase fixture) : IAsyncLifetime
{
    private readonly AutoBrewingBeContext _dbContext = fixture.DbContext;
    private readonly Func<Task> _resetDatabase = fixture.ResetDatabaseAsync;
    private readonly IAuthService _authService = fixture.ServiceProvider.GetRequiredService<IAuthService>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _resetDatabase();


    [Fact]
    public async Task Handle_RefreshWithCorrectRefreshToken_ReturnsJwtToken()
    {
        // Arrange
        var user = await TestDataLogin.CreateTestUser(_dbContext);
        var account = TestDataLogin.CreateLoginDto(user.Email, "admin");

        // Act
        var serviceLogin = _authService.Login(account);
        var resultLogin = await serviceLogin;
        var serviceRefresh = _authService.Refresh(new RefreshDto()
        {
            RefreshToken = resultLogin.Response!.RefreshToken
        });
        var resultRefresh = await serviceRefresh;

        // Assert
        Assert.Equal(StatusCodes.Status200OK, resultRefresh.StatusCode);
    }
}