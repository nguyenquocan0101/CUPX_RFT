using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Pagination;
using FirebaseAdmin.Auth;
using Services.Base;
using Services.Dtos.Account;

namespace Services.Interfaces;

public interface IAccountService
{
    Task<Account?> GetFirebaseUserAsync(string firebaseId);
    // Task<Account> CreateViaFirebase(UserRecord userRecord);

    Task<BaseResult<AccountQueryDto, Paginate<AccountDto>>> GetAccounts(AccountQueryDto accountQueryDto);
    Task<BaseResult<string, AccountDto>> GetAccount(string accountId);
}