using Backend.Cliniq.Models.Entities.Base;
using Cliniq.Services.DataServices.Base;

namespace Cliniq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public abstract class BaseCrudController<TEntity, TCreateDto, TUpdateDto, TResponseDto>
    : ControllerBase
    where TEntity : BaseEntity, new()
{
    protected readonly IBaseDataService<TEntity> Service;

    protected BaseCrudController(IBaseDataService<TEntity> service)
    {
        Service = service;
    }

    [HttpGet]
    public virtual async Task<ActionResult<IReadOnlyList<TResponseDto>>> GetAll(
        [FromQuery] string? filterOn,
        [FromQuery] string? filterQuery,
        [FromQuery] string? sortBy,
        [FromQuery] bool isAscending = true,
        [FromQuery] int pageSize = 20,
        [FromQuery] int pageNumber = 1)
    {
        ValidatePage(pageSize, pageNumber);
        var entities = await Service.GetAllAsync(
            filterOn, filterQuery, sortBy, isAscending, pageSize, pageNumber);
        return Ok(entities.Select(ToResponse).ToArray());
    }

    [HttpGet("{id:int}")]
    public virtual async Task<ActionResult<TResponseDto>> GetById(int id)
    {
        var entity = await Service.FindAsync(id);
        return entity is null ? NotFound() : Ok(ToResponse(entity));
    }

    [HttpPost]
    public virtual async Task<ActionResult<TResponseDto>> Create(TCreateDto input)
    {
        var entity = await CreateEntityAsync(input);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToResponse(entity));
    }

    [HttpPut("{id:int}")]
    public virtual async Task<ActionResult<TResponseDto>> Update(int id, TUpdateDto input)
    {
        var entity = await Service.FindAsync(id);
        if (entity is null)
        {
            return NotFound();
        }

        ApplyUpdate(entity, input);
        await Service.UpdateAsync(entity);
        return Ok(ToResponse(entity));
    }

    [HttpDelete("{id:int}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        var entity = await Service.FindAsync(id);
        if (entity is null)
        {
            return NotFound();
        }

        await Service.DeleteAsync(entity);
        return NoContent();
    }

    protected virtual Task<TEntity> CreateEntityAsync(TCreateDto input)
    {
        return Service.AddAsync(MapCreate(input));
    }

    protected virtual TEntity MapCreate(TCreateDto input)
    {
        throw new NotSupportedException("Create is not supported for this resource.");
    }

    protected abstract void ApplyUpdate(TEntity entity, TUpdateDto input);

    protected abstract TResponseDto ToResponse(TEntity entity);

    protected BaseWebException MethodNotAllowed(string operation)
    {
        return new BaseWebException(
            $"{operation} is not supported for this resource.",
            StatusCodes.Status405MethodNotAllowed,
            "method_not_allowed");
    }

    private static void ValidatePage(int pageSize, int pageNumber)
    {
        if (pageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 100.");
        }

        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be positive.");
        }
    }
}
