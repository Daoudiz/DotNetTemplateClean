
using Microsoft.AspNetCore.Mvc;
using DotNetTemplateClean.Application;

namespace DotNetTemplateClean.WebAPI;

[ApiController]
[Route("api/[controller]")]
public class ApiBaseController : ControllerBase
{
    protected ActionResult HandleResult<T>(ServiceResult<T> result)
    {
        if (result == null) return NotFound();

        if (result.IsSuccess)
        {
            if (result.Value == null && result.StatusCode == 200)
                return NoContent();         


            return StatusCode(result.StatusCode, new { data = result.Value } );
        }

        // Harmonisation : On utilise la vraie classe ProblemDetails
        var problemDetails = new ProblemDetails
        {
            Status = result.StatusCode,
            Title = "Conflit ou Erreur métier",
            Detail = result.ErrorMessage, 
            Instance = HttpContext.Request.Path
        };

        return StatusCode(result.StatusCode, problemDetails);
    }        
}
