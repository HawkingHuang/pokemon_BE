using api.Dtos.Account;
using api.Extensions;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[Route("api/account")]
[ApiController]
public class AccountController : ControllerBase
{
    private const string RefreshTokenCookie = "refreshToken";
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);
    private static readonly TimeSpan ReuseGracePeriod = TimeSpan.FromSeconds(10);

    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepo;

    public AccountController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepo)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _refreshTokenRepo = refreshTokenRepo;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var appUser = new AppUser
        {
            UserName = registerDto.UserName,
            Email = registerDto.Email
        };

        var result = await _userManager.CreateAsync(appUser, registerDto.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        var authResponseDto = await SignInAsync(appUser);
        return Ok(authResponseDto);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var user = await _userManager.FindByNameAsync(loginDto.UserName);
        if (user == null) return Unauthorized("Invalid username or password.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);
        if (result.IsLockedOut) return Unauthorized("Too many failed attempts. Try again in a few minutes.");
        if (!result.Succeeded) return Unauthorized("Invalid username or password.");

        var authResponseDto = await SignInAsync(user);
        return Ok(authResponseDto);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var currentUserDto = new CurrentUserDto
        {
            Id = User.GetUserId(),
            UserName = User.GetUserName()
        };
        return Ok(currentUserDto);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var token = Request.Cookies[RefreshTokenCookie];
        if (string.IsNullOrEmpty(token)) return Unauthorized();

        var stored = await _refreshTokenRepo.GetByHashAsync(_tokenService.HashRefreshToken(token));
        if (stored == null || stored.ExpiresAt <= DateTime.UtcNow) return Unauthorized();

        if (stored.RevokedAt != null)
        {
            // rotated moments ago by a parallel request: the client retries with the newer cookie
            if (DateTime.UtcNow - stored.RevokedAt.Value < ReuseGracePeriod) return Conflict();

            // an old rotated token came back, so it leaked: end every session of this user
            await _refreshTokenRepo.DeleteAllAsync(stored.AppUserId);
            DeleteRefreshTokenCookie();
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(stored.AppUserId);
        if (user == null) return Unauthorized();

        var newToken = _tokenService.CreateRefreshToken();
        var replacement = BuildRefreshToken(user.Id, newToken);
        await _refreshTokenRepo.RotateAsync(stored, replacement);
        SetRefreshTokenCookie(newToken, replacement.ExpiresAt);

        var authResponseDto = BuildAuthResponse(user);
        return Ok(authResponseDto);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Cookies[RefreshTokenCookie];
        if (!string.IsNullOrEmpty(token))
        {
            var stored = await _refreshTokenRepo.GetByHashAsync(_tokenService.HashRefreshToken(token));
            if (stored != null) await _refreshTokenRepo.DeleteAsync(stored);
        }

        DeleteRefreshTokenCookie();
        return NoContent();
    }

    private async Task<AuthResponseDto> SignInAsync(AppUser user)
    {
        var token = _tokenService.CreateRefreshToken();
        var refreshToken = BuildRefreshToken(user.Id, token);
        await _refreshTokenRepo.CreateAsync(refreshToken);
        SetRefreshTokenCookie(token, refreshToken.ExpiresAt);

        return BuildAuthResponse(user);
    }

    private AuthResponseDto BuildAuthResponse(AppUser user) => new()
    {
        UserName = user.UserName ?? string.Empty,
        AccessToken = _tokenService.CreateAccessToken(user)
    };

    private RefreshToken BuildRefreshToken(string appUserId, string token)
    {
        var now = DateTime.UtcNow;
        return new RefreshToken
        {
            TokenHash = _tokenService.HashRefreshToken(token),
            AppUserId = appUserId,
            CreatedAt = now,
            ExpiresAt = now.Add(RefreshTokenLifetime)
        };
    }

    private static CookieOptions RefreshTokenCookieOptions(DateTime? expires = null) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = "/api/account",
        Expires = expires
    };

    private void SetRefreshTokenCookie(string token, DateTime expiresAt) =>
        Response.Cookies.Append(RefreshTokenCookie, token, RefreshTokenCookieOptions(expiresAt));

    private void DeleteRefreshTokenCookie() =>
        Response.Cookies.Delete(RefreshTokenCookie, RefreshTokenCookieOptions());
}
