using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.Account;
using Services.Dtos.Auth;
using Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace AutomaticBrewingCoffee.API.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/auth")]
    [ApiController]
    [TrimStrings]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IAccountService _accountService;

        public AuthController(IAuthService authService, IAccountService accountService)
        {
            _authService = authService;
            _accountService = accountService;
        }

        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Account Login",
            Description = "Authenticate user with username and password to get a JWT token."
        )]
        public async Task<ActionResult<BaseResult<LoginDto, JwtDto>>> Login(LoginDto loginDto)
        {
            var response = await _authService.Login(loginDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("refresh")]
        [SwaggerOperation(
            Summary = "Refresh JWT Token",
            Description = "Refresh an expired JWT token using the refresh token."
        )]
        public async Task<ActionResult<BaseResult<RefreshDto, JwtDto>>> Refresh(RefreshDto refreshDto)
        {
            var response = await _authService.Refresh(refreshDto);
            return StatusCode(response.StatusCode, response);
        }

        // [HttpPost("firebase/login")]
        // [SwaggerOperation(
        //     Summary = "",
        //     Description = ""
        // )]
        // public async Task<ActionResult<BaseResult<LoginDto, JwtDto>>> LoginFirebase(LoginDto loginDto)
        // {
        //     var response = await _authService.LoginFirebase(loginDto);
        //     return StatusCode(response.StatusCode, response);
        // }

        [HttpPost("create-account")]
        [Authorizes(nameof(ERoleName.Organization), nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "User Registration",
            Description = "Register a new user with username, password, and email."
        )]
        public async Task<ActionResult<BaseResult<CreateAccountDto, string>>> CreateAccount(
            [FromBody] CreateAccountDto createAccountDto)
        {
            var response = await _authService.CreateAccount(createAccountDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("accounts")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Get all account exist in the system with queries",
            Description = "Get all account exist in the system with queries."
        )]
        public async Task<ActionResult<BaseResult<AccountQueryDto, Paginate<AccountDto>>>> Gets(
            [FromQuery] AccountQueryDto accountQueryDto)
        {
            var response = await _accountService.GetAccounts(accountQueryDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("accounts/{accountId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Get a exist account",
            Description = "Get a exist account."
        )]
        public async Task<ActionResult<BaseResult<string, AccountDto>>> Get(
            [FromRoute] string accountId)
        {
            var response = await _accountService.GetAccount(accountId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("change-password")]
        [Authorizes(nameof(ERoleName.Organization), nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Change password of a account",
            Description = "Change password of a account."
        )]
        public async Task<ActionResult<BaseResult<string, AccountDto>>> ChangePassword(
            [FromBody] ChangePasswordDto changePasswordDto)
        {
            var response = await _authService.ChangePassword(changePasswordDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("ban-account")]
        [Authorizes(nameof(ERoleName.Organization), nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Ban an account",
            Description = "Ban an account"
        )]
        public async Task<ActionResult<BaseResult<string, AccountDto>>> Ban(
            [FromBody] BanAccountDto banAccountDto)
        {
            var response = await _authService.BanAccount(banAccountDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("unban-account")]
        [Authorizes(nameof(ERoleName.Organization), nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Unban an account",
            Description = "Unban an account"
        )]
        public async Task<ActionResult<BaseResult<string, AccountDto>>> Unban(
            [FromBody] UnbanAccountDto unbanAccountDto)
        {
            var response = await _authService.UnbanAccount(unbanAccountDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("current-account")]
        [Authorizes(nameof(ERoleName.Organization), nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Get caller account",
            Description = "Get caller account"
        )]
        public async Task<ActionResult<BaseResult<dynamic, AccountDto>>> CurrentAccount()
        {
            var response = await _authService.CurrentAccount();
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("logout")]
        [Authorizes(nameof(ERoleName.Organization), nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Unban an account",
            Description = "Unban an account"
        )]
        public async Task<ActionResult<BaseResult<dynamic, dynamic>>> Logout()
        {
            var response = await _authService.Logout();
            return StatusCode(response.StatusCode, response);
        }
    }
}