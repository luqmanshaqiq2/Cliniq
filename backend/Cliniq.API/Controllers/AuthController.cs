using Cliniq.API.Contracts;
using Cliniq.Models.Options;
using Cliniq.Services.DataServices.Helpers;
using Microsoft.Extensions.Options;
using System.Text;

namespace Cliniq.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IUserDataService _users;
    private readonly JwtOptions _jwtOptions;

    public AuthController(IUserDataService users, IOptions<JwtOptions> jwtOptions)
    {
        _users = users;
        _jwtOptions = jwtOptions.Value;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<UserResponseDto>> Register(RegisterRequest request)
    {
        if (Encoding.UTF8.GetByteCount(request.Password) > 72)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Password),
                "Password must not exceed 72 UTF-8 bytes.");
        }

        if (await _users.ExistsByEmailAsync(request.Email))
        {
            throw new BaseWebException(
                "An account with this email already exists.",
                StatusCodes.Status409Conflict,
                "email_already_registered");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = JwtHelpers.HashPassword(request.Password),
            Role = UserRole.Patient,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        await _users.AddAsync(user);
        return StatusCode(StatusCodes.Status201Created, ToResponse(user));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _users.FindByEmailAsync(request.Email);
        if (user is null ||
            !user.IsActive ||
            !JwtHelpers.VerifyPasswordHash(request.Password, user.PasswordHash))
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid credentials",
                Detail = "The email or password is incorrect."
            });
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.Lifetime);
        var token = JwtHelpers.GenerateJwtToken(user, _jwtOptions);
        return Ok(new AuthResponse(ToResponse(user), token, expiresAt));
    }

    private static UserResponseDto ToResponse(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAtUtc = user.CreatedAtUtc
        };
    }
}
