using AutomaticBrewingCoffee.Domain.Context;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Services.Utils;
using Services.Dtos.Auth;
using Services.Tests.TestBase;
using Services.Utils;

namespace Services.Tests.AuthService.TestData;

public class TestDataLogin
{
    public static Account CreateUser() => new Account()
    {
        AccountId = Guid.NewGuid().ToString(),
        RoleName = "Admin",
        FullName = "Administrator",
        Email = "admin.test@gmail.com",
        Password = Hasher.Hash("admin"),
        Status = EBaseStatus.Active.ToString(),
        CreatedDate = DateTime.UtcNow,
        IsDeleted = false,
        DeletedDate = null
    };

    public static LoginDto CreateLoginDto(string email, string password) => new LoginDto()
    {
        Password = password,
        Email = email
    };

    public static async Task<Account> CreateTestUser(AutoBrewingBeContext dbContext)
    {
        var user = CreateUser();
        await dbContext.AddAsync(user);
        await dbContext.SaveChangesAsync();
        return user;
    }
}