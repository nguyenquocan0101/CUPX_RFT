using Services.Base;
using Services.Dtos.Account;
using Services.Dtos.Auth;

namespace Services.Interfaces;

public interface IAuthService
{
    Task<BaseResult<LoginDto, JwtDto>> Login(LoginDto loginDto);
    Task<BaseResult<RefreshDto, JwtDto>> Refresh(RefreshDto refreshDto);
    Task<BaseResult<LoginDto, JwtDto>> LoginFirebase(LoginDto loginDto);
    Task<BaseResult<CreateAccountDto, string>> CreateAccount(CreateAccountDto createAccountDto);
    Task<BaseResult<ChangePasswordDto, AccountDto>> ChangePassword(ChangePasswordDto changePasswordDto);
    Task<BaseResult<BanAccountDto, AccountDto>> BanAccount(BanAccountDto banAccountDto);
    Task<BaseResult<UnbanAccountDto, AccountDto>> UnbanAccount(UnbanAccountDto banAccountDto);
    Task<BaseResult<dynamic, AccountDto>> CurrentAccount();
    Task<BaseResult<dynamic, dynamic>> Logout();
}