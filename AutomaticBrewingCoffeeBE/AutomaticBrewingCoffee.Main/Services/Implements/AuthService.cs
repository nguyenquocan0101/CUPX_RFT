using System.Security.Claims;
using AutoMapper;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using AutomaticBrewingCoffee.Services.Utils;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Services.Base;
using Services.CapRabbitMQ.Messages.Notification;
using Services.CapRabbitMQ.Topics;
using Services.Dtos.Account;
using Services.Dtos.Auth;
using Services.Dtos.Organization;
using Services.Firebase;
using Services.Interfaces;
using Services.Redis;
using Services.Utils;

namespace Services.Implements;

public class AuthService : BaseService<AuthService>, IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IRedisService _redisService;
    private readonly IFirebaseAuthService _firebaseAuthService;
    private readonly ICapPublisher _capPublisher;

    public AuthService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILoggerFactory loggerFactory,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        IRedisService redisService,
        IFirebaseAuthService firebaseAuthService,
        ICapPublisher capPublisher) : base(
        unitOfWork,
        mapper,
        loggerFactory,
        httpContextAccessor
    )
    {
        _configuration = configuration;
        _redisService = redisService;
        _firebaseAuthService = firebaseAuthService;
        _capPublisher = capPublisher;
    }

    public async Task<BaseResult<LoginDto, JwtDto>> Login(LoginDto loginDto)
    {
        var user = await _unitOfWork.GetRepository<Account>()
            .SingleOrDefaultAsync(
                predicate: user => user.Email == loginDto.Email
            );

        if (user is null)
        {
            return new BaseResult<LoginDto, JwtDto>()
            {
                Message = "Email hoặc mật khẩu không chính xác",
                StatusCode = StatusCodes.Status401Unauthorized,
                IsSuccess = false,
                Response = null,
                Request = loginDto
            };
        }

        if (user.IsBanned)
        {
            return new BaseResult<LoginDto, JwtDto>()
            {
                Message = MessageUtil.IsBan<Account>(),
                StatusCode = StatusCodes.Status400BadRequest,
                IsSuccess = false,
                Response = null,
                Request = loginDto
            };
        }

        if (!Hasher.Verify(loginDto.Password, user.Password))
        {
            return new BaseResult<LoginDto, JwtDto>()
            {
                Message = "Email hoặc mật khẩu không chính xác",
                StatusCode = StatusCodes.Status401Unauthorized,
                IsSuccess = false,
                Response = null,
                Request = loginDto
            };
        }

        var accessToken = JwtUtil.GenerateAccessToken(user, _configuration);
        var refreshToken = JwtUtil.GenerateRefreshToken(user, _configuration);

        var result = await _redisService.SetDataAsync($"refresh:{user.AccountId}", refreshToken,
            DateTimeOffset.UtcNow.AddDays(7));

        if (!result)
        {
            user.RefreshToken = refreshToken;
            _unitOfWork.GetRepository<Account>().Update(user);
            await _unitOfWork.CommitAsync();
        }

        return new BaseResult<LoginDto, JwtDto>()
        {
            Message = "Đăng nhập thành công",
            StatusCode = StatusCodes.Status200OK,
            IsSuccess = true,
            Response = new JwtDto()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            },
            Request = null
        };
    }

    public async Task<BaseResult<RefreshDto, JwtDto>> Refresh(RefreshDto refreshDto)
    {
        var claimPrincipal = JwtUtil.GetPrincipalFromToken(refreshDto.RefreshToken, _configuration);

        if (claimPrincipal is null)
        {
            return new BaseResult<RefreshDto, JwtDto>()
            {
                Message = "Không thể xác thực mã làm mới",
                StatusCode = StatusCodes.Status401Unauthorized,
                IsSuccess = false,
                Response = null,
                Request = refreshDto
            };
        }

        var user = new Account()
        {
            AccountId = claimPrincipal.FindFirst("AccountId")?.Value!,
            Email = claimPrincipal.FindFirst(ClaimTypes.Email)?.Value!,
            RoleName = claimPrincipal.FindFirst(ClaimTypes.Role)?.Value!,
            FullName = claimPrincipal.FindFirst("fullname")?.Value!,
        };

        var storeToken = await _redisService.GetDataAsync<string>($"refresh:{user.AccountId}");

        if (storeToken is null)
        {
            var account = await _unitOfWork.GetRepository<Account>().SingleOrDefaultAsync(
                predicate: x => x.AccountId == user.AccountId);

            if (account?.RefreshToken is null)
            {
                return new BaseResult<RefreshDto, JwtDto>()
                {
                    Message = "Không thể xác thực mã làm mới",
                    StatusCode = StatusCodes.Status401Unauthorized,
                    IsSuccess = false,
                    Response = null,
                    Request = refreshDto
                };
            }

            storeToken = account.RefreshToken;
        }

        if (!storeToken.Equals(refreshDto.RefreshToken))
        {
            return new BaseResult<RefreshDto, JwtDto>()
            {
                Message = "Không thể xác thực mã làm mới",
                StatusCode = StatusCodes.Status401Unauthorized,
                IsSuccess = false,
                Response = null,
                Request = refreshDto
            };
        }

        var accessToken = JwtUtil.GenerateAccessToken(user, _configuration);
        var refreshToken = JwtUtil.GenerateRefreshToken(user, _configuration);

        var result = await _redisService.SetDataAsync($"refresh:{user.AccountId}", refreshToken,
            DateTimeOffset.UtcNow.AddDays(7));

        if (!result)
        {
            user.RefreshToken = refreshToken;
            _unitOfWork.GetRepository<Account>().Update(user);
            await _unitOfWork.CommitAsync();
        }

        return new BaseResult<RefreshDto, JwtDto>()
        {
            Message = "Làm mới phiên thành công",
            StatusCode = StatusCodes.Status200OK,
            IsSuccess = true,
            Response = new JwtDto()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            },
            Request = null
        };
    }

    public async Task<BaseResult<LoginDto, JwtDto>> LoginFirebase(LoginDto loginDto)
    {
        var firebaseLoginResponse = await _firebaseAuthService.LoginByEmailPassword(loginDto.Email, loginDto.Password);

        return new BaseResult<LoginDto, JwtDto>()
        {
            Message = "Đăng nhập thành công",
            StatusCode = StatusCodes.Status200OK,
            IsSuccess = true,
            Response = new JwtDto()
            {
                AccessToken = firebaseLoginResponse.IdToken,
                RefreshToken = firebaseLoginResponse.RefreshToken
            },
            Request = null
        };
    }

    public async Task<BaseResult<CreateAccountDto, string>> CreateAccount(CreateAccountDto createAccountDto)
    {
        var account = await _unitOfWork.GetRepository<Account>()
            .SingleOrDefaultAsync(predicate: x => x.Email == createAccountDto.Email);

        if (account is not null)
        {
            return new BaseResult<CreateAccountDto, string>()
            {
                Message = "Email đã tồn tại",
                IsSuccess = false,
                Response = null,
                Request = createAccountDto,
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        if (createAccountDto.ReferenceId is not null)
        {
            switch (createAccountDto.RoleName)
            {
                case nameof(ERoleName.Organization):
                {
                    var organization = await _unitOfWork.GetRepository<Organization>()
                        .SingleOrDefaultAsync(predicate: x => x.OrganizationId == createAccountDto.ReferenceId);

                    if (organization is not null)
                    {
                        return new BaseResult<CreateAccountDto, string>()
                        {
                            Message = MessageUtil.AlreadyExists<Account>(),
                            IsSuccess = false,
                            Response = null,
                            Request = createAccountDto,
                            StatusCode = StatusCodes.Status400BadRequest
                        };
                    }

                    break;
                }
                case nameof(ERoleName.Admin):
                {
                    return new BaseResult<CreateAccountDto, string>()
                    {
                        Message = MessageUtil.Invalid<ERoleName>(),
                        IsSuccess = false,
                        Response = null,
                        Request = createAccountDto,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                    break;
                }
            }
        }

        account = _mapper.Map<Account>(createAccountDto);
        account.Password = Hasher.Hash(account.Password);

        await _unitOfWork.GetRepository<Account>().InsertAsync(account);
        await _unitOfWork.CommitAsync();

        return new BaseResult<CreateAccountDto, string>()
        {
            Message = MessageUtil.CreateSuccess<Account>(),
            IsSuccess = true,
            Response = null,
            Request = null,
            StatusCode = StatusCodes.Status201Created
        };
    }

    public async Task<BaseResult<ChangePasswordDto, AccountDto>> ChangePassword(ChangePasswordDto changePasswordDto)
    {
        var accountId = GetAccountIdFromJwt();

        var account = await _unitOfWork.GetRepository<Account>().SingleOrDefaultAsync(
            predicate: x => x.AccountId == accountId
        );

        if (account is null)
        {
            return new BaseResult<ChangePasswordDto, AccountDto>()
            {
                Request = changePasswordDto,
                IsSuccess = false,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound,
                Message = MessageUtil.NotFound<Account>()
            };
        }

        var isPasswordCorrect = Hasher.Verify(changePasswordDto.OldPassword, account.Password);

        if (!isPasswordCorrect)
        {
            return new BaseResult<ChangePasswordDto, AccountDto>()
            {
                Message = MessageUtil.Invalid<ChangePasswordDto>(),
                IsSuccess = false,
                Request = changePasswordDto,
                StatusCode = StatusCodes.Status400BadRequest,
                Response = null
            };
        }

        account.Password = Hasher.Hash(changePasswordDto.NewPassword);
        account.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.GetRepository<Account>().Update(account);
        await _unitOfWork.CommitAsync();

        var accountDto = _mapper.Map<AccountDto>(account);

        return new BaseResult<ChangePasswordDto, AccountDto>()
        {
            Message = MessageUtil.UpdateSuccess<Account>(),
            IsSuccess = true,
            Response = accountDto,
            Request = changePasswordDto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<BanAccountDto, AccountDto>> BanAccount(BanAccountDto banAccountDto)
    {
        var account = await _unitOfWork.GetRepository<Account>().SingleOrDefaultAsync(
            predicate: x => x.AccountId == banAccountDto.AccountId
        );

        if (account is null)
        {
            return new BaseResult<BanAccountDto, AccountDto>()
            {
                Message = MessageUtil.NotFound<Account>(),
                IsSuccess = false,
                Response = null,
                Request = banAccountDto,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        await _redisService.RemoveDataAsync($"refresh:{account.AccountId}");
        account.Ban(banAccountDto.BannedReason);

        _unitOfWork.GetRepository<Account>().Update(account);
        await _unitOfWork.CommitAsync();

        var accountDto = _mapper.Map<AccountDto>(account);

        await _capPublisher.PublishAsync(
            NotificationCapTopic.NotificationForceLogout,
            new NotificationForceLogoutCapMessage()
            {
                AccountId = account.AccountId
            });


        return new BaseResult<BanAccountDto, AccountDto>()
        {
            Message = MessageUtil.BanSuccess<Account>(),
            IsSuccess = true,
            Response = accountDto,
            Request = banAccountDto,
            StatusCode = StatusCodes.Status202Accepted
        };
    }

    public async Task<BaseResult<UnbanAccountDto, AccountDto>> UnbanAccount(UnbanAccountDto unbanAccountDto)
    {
        var account = await _unitOfWork.GetRepository<Account>().SingleOrDefaultAsync(
            predicate: x => x.AccountId == unbanAccountDto.AccountId
        );

        if (account is null)
        {
            return new BaseResult<UnbanAccountDto, AccountDto>()
            {
                Message = MessageUtil.NotFound<Account>(),
                IsSuccess = false,
                Response = null,
                Request = unbanAccountDto,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        account.Unban(unbanAccountDto.UnbannedReason);

        _unitOfWork.GetRepository<Account>().Update(account);
        await _unitOfWork.CommitAsync();

        var accountDto = _mapper.Map<AccountDto>(account);

        return new BaseResult<UnbanAccountDto, AccountDto>()
        {
            Message = MessageUtil.UnbanSuccess<Account>(),
            IsSuccess = true,
            Response = accountDto,
            Request = unbanAccountDto,
            StatusCode = StatusCodes.Status202Accepted
        };
    }

    public async Task<BaseResult<dynamic, AccountDto>> CurrentAccount()
    {
        var accountId = GetAccountIdFromJwt();

        if (accountId is null)
        {
            return new BaseResult<dynamic, AccountDto>()
            {
                IsSuccess = false,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound,
                Message = MessageUtil.NotFound<Account>(),
                Request = accountId
            };
        }

        var account = await _unitOfWork.GetRepository<Account>().SingleOrDefaultAsync(
            predicate: x => x.AccountId == accountId
        );

        if (account is null)
        {
            return new BaseResult<dynamic, AccountDto>()
            {
                IsSuccess = false,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound,
                Message = MessageUtil.NotFound<Account>(),
                Request = accountId
            };
        }

        var accountDto = _mapper.Map<AccountDto>(account);

        switch (account.RoleName)
        {
            case nameof(ERoleName.Organization):
            {
                var organization = await _unitOfWork.GetRepository<Organization>().SingleOrDefaultAsync(
                    predicate: x => x.OrganizationId == account.OrganizationId
                );

                var organizationDto = _mapper.Map<OrganizationDto>(organization);

                accountDto.Organization = organizationDto;

                break;
            }
        }

        return new BaseResult<dynamic, AccountDto>()
        {
            Message = MessageUtil.ReadSuccess<Account>(),
            IsSuccess = true,
            Request = accountId,
            Response = accountDto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<dynamic, dynamic>> Logout()
    {
        return new BaseResult<dynamic, dynamic>()
        {
            Message = "",
            Request = null,
            StatusCode = StatusCodes.Status202Accepted,
            IsSuccess = true,
            Response = null
        };
    }
}