using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotNetTemplateClean.Application;


namespace DotNetTemplateClean.WebAPI;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class EntiteController(IEntiteService entiteService) : ApiBaseController
{
    private readonly IEntiteService _entiteService = entiteService;


    /// <summary>
    /// Recherche des unités organisationnelles avec filtres et pagination.
    /// </summary>
    [HttpGet("EntiteSearch")]
    [ProducesResponseType(typeof(PagedResult<OrganizationUnitResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<OrganizationUnitResponseDto>>> GetUnits([FromQuery] OrganizationSearchFilters filters)
    {
       
        var result = await _entiteService.SearchUnitsAsync(filters); 
        
        return Ok(result);
    }

    // GET: api/entites/directions
    [HttpGet("directions")]
    [Authorize]
    public async Task<IActionResult> GetAllDirections()
    {
       
            var results = await _entiteService.GetAllDirections();
            return Ok(results);
       
    }

    // GET: api/entites/divisions/5
    [HttpGet("divisions/{directionId}")]
    [Authorize]
    public async Task<IActionResult> GetDivisionsByDirection(int directionId)
    {
        if (directionId <= 0) return BadRequest("ID de direction invalide.");

         var results = await _entiteService.GetDivisionsByDirection(directionId);
            return Ok(results);
       
    }

    // GET: api/entites/services/12
    [HttpGet("services/{rattachementId}")]
    [Authorize]
    public async Task<IActionResult> GetServicesByRattachement(int rattachementId)
    {
        if (rattachementId <= 0) return BadRequest("ID de division invalide.");

        var results = await _entiteService.GetServicesByRattachement(rattachementId);
        
        return Ok(results);
       
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("CreateEntite")]       
    public async Task<IActionResult> Create([FromBody] OrganizationUnitSaveDto dto, CancellationToken ct)
    {
        //Appel au service
        var result = await _entiteService.CreateEntiteAsync(dto, ct);

       return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromBody] OrganizationUnitSaveDto dto, CancellationToken ct)
    {
        if (dto is null) return BadRequest();

        var result = await _entiteService.UpdateEntiteAsync(dto, ct);

        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (id <= 0) return BadRequest("Invalid identifier.");

        var result = await _entiteService.DeleteEntiteAsync(id, ct);

        return HandleResult(result);
    }

    /// <summary>
    /// Get all available TypeEntite values.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("TypesEntites")]    
    public async Task<IActionResult> GetAllTypes()
    {
        var results = await _entiteService.GetAllTypeEntite();
        return Ok(results);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("OrganizationTree")]
    public async Task<ActionResult<List<TreeNodeDto>>> GetTree()
    {
        var tree = await _entiteService.GetOrganizationTreeAsync();
        return Ok(tree);
    }

    /// <summary>
    /// Get a single entité by id.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("{id}", Name = "GetEntiteById")]    
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEntiteById(int id)
    {
        if (id <= 0) return BadRequest("Invalid identifier.");

        var result = await _entiteService.GetEntiteById(id);

        return HandleResult(result);
    }
}
