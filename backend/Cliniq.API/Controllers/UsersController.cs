using Cliniq.Services.DataServices.Helpers;
using System.Text;

namespace Cliniq.API.Controllers;

[Authorize(Roles = nameof(UserRole.Administrator))]
public sealed class UsersController
    : BaseCrudController<User, UserCreateDto, UserUpdateDto, UserResponseDto>
{
    private readonly IUserDataService _users;

    public UsersController(IUserDataService users) : base(users)
    {
        _users = users;
    }

    [HttpPost("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        return await _users.DeactivateAsync(id) ? NoContent() : NotFound();
    }

    public override async Task<ActionResult<UserResponseDto>> Update(int id, UserUpdateDto input)
    {
        var existing = await _users.FindAsync(id);
        if (existing is null)
        {
            return NotFound();
        }

        if (!string.Equals(existing.Email, input.Email.Trim(), StringComparison.OrdinalIgnoreCase) &&
            await _users.ExistsByEmailAsync(input.Email))
        {
            throw new BaseWebException(
                "An account with this email already exists.",
                StatusCodes.Status409Conflict,
                "email_already_registered");
        }

        ApplyUpdate(existing, input);
        await _users.UpdateAsync(existing);
        return Ok(ToResponse(existing));
    }

    public override Task<IActionResult> Delete(int id)
    {
        throw MethodNotAllowed("User deletion");
    }

    protected override async Task<User> CreateEntityAsync(UserCreateDto input)
    {
        if (await _users.ExistsByEmailAsync(input.Email))
        {
            throw new BaseWebException(
                "An account with this email already exists.",
                StatusCodes.Status409Conflict,
                "email_already_registered");
        }

        return await _users.AddAsync(MapCreate(input));
    }

    protected override User MapCreate(UserCreateDto input)
    {
        if (Encoding.UTF8.GetByteCount(input.Password) > 72)
        {
            throw new ArgumentOutOfRangeException(
                nameof(input.Password),
                "Password must not exceed 72 UTF-8 bytes.");
        }

        if (!Enum.IsDefined(input.Role))
        {
            throw new ArgumentOutOfRangeException(nameof(input.Role));
        }

        return new User
        {
            FullName = input.FullName.Trim(),
            Email = input.Email.Trim(),
            PasswordHash = JwtHelpers.HashPassword(input.Password),
            Role = input.Role,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    protected override void ApplyUpdate(User entity, UserUpdateDto input)
    {
        if (!Enum.IsDefined(input.Role))
        {
            throw new ArgumentOutOfRangeException(nameof(input.Role));
        }

        entity.FullName = input.FullName.Trim();
        entity.Email = input.Email.Trim();
        entity.Role = input.Role;
        entity.IsActive = input.IsActive;
    }

    protected override UserResponseDto ToResponse(User entity)
    {
        return new UserResponseDto
        {
            Id = entity.Id,
            FullName = entity.FullName,
            Email = entity.Email,
            Role = entity.Role,
            IsActive = entity.IsActive,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }
}
