using DotNetTemplateClean.Application;
using DotNetTemplateClean.Domain;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DotNetTemplateClean.WebAPI;

public class EntiteEndpointGroup : IEndpointGroup
{
    public static string RoutePrefix => "/api/entite";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetUnits, "/EntiteSearch");
        groupBuilder.MapGet(GetAllDirections, "/directions");
        groupBuilder.MapGet(GetDivisionsByDirection, "/divisions/{directionId}");
        groupBuilder.MapGet(GetServicesByRattachement, "/services/{rattachementId}");

        groupBuilder.MapPost(Create, "/CreateEntite")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        groupBuilder.MapPut(Update, "/{id}")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        groupBuilder.MapDelete(Delete, "/{id}")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        groupBuilder.MapGet(GetAllTypes, "/TypesEntites")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        groupBuilder.MapGet(GetTree, "/OrganizationTree")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        groupBuilder.MapGet(GetEntiteById, "/{id}")
            .WithName("GetEntiteById")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }

    public static async Task<Ok<PagedResult<OrganizationUnitResponseDto>>> GetUnits(ISender sender, [AsParameters] OrganizationSearchFilters filters)
    {
        var result = await sender.Send(new SearchEntitesQuery
        {
            SearchTerm = filters.SearchTerm,
            TypeEntiteId = filters.TypeEntiteId,
            ParentId = filters.ParentId,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize
        });

        return TypedResults.Ok(result);
    }

    public static async Task<Ok<List<Entite>>> GetAllDirections(ISender sender)
    {
        var results = await sender.Send(new GetEntitesByTypeQuery(EntityTypes.Direction, SelectIdAndCodeOnly: true));
        return TypedResults.Ok(results);
    }

    public static async Task<Results<Ok<List<Entite>>, BadRequest<string>>> GetDivisionsByDirection(ISender sender, int directionId)
    {
        if (directionId <= 0)
        {
            return TypedResults.BadRequest("ID de direction invalide.");
        }

        var results = await sender.Send(new GetEntitesByParentQuery(EntityTypes.Division, directionId));
        return TypedResults.Ok(results);
    }

    public static async Task<Results<Ok<List<Entite>>, BadRequest<string>>> GetServicesByRattachement(ISender sender, int rattachementId)
    {
        if (rattachementId <= 0)
        {
            return TypedResults.BadRequest("ID de division invalide.");
        }

        var results = await sender.Send(new GetEntitesByParentQuery(EntityTypes.Service, rattachementId));
        return TypedResults.Ok(results);
    }

    public static async Task<IResult> Create(ISender sender, HttpContext httpContext, OrganizationUnitSaveDto dto, CancellationToken ct)
    {
        var result = await sender.Send(new CreateEntiteCommand(dto), ct);
        return result.IsSuccess
            ? HandleResult(result)
            : ToProblemDetails(result, httpContext);
    }

    public static async Task<IResult> Update(ISender sender, HttpContext httpContext, int id, OrganizationUnitSaveDto dto, CancellationToken ct)
    {
        if (dto is null)
        {
            return TypedResults.BadRequest();
        }

        if (!dto.Id.HasValue || dto.Id.Value != id)
        {
            return TypedResults.BadRequest();
        }

        var result = await sender.Send(new UpdateEntiteCommand(dto), ct);
        if (!result.IsSuccess)
        {
            return ToProblemDetails(result, httpContext);
        }

        return TypedResults.NoContent();
    }

    public static async Task<IResult> Delete(ISender sender, HttpContext httpContext, int id, CancellationToken ct)
    {
        if (id <= 0)
        {
            return TypedResults.BadRequest("Invalid identifier.");
        }

        var result = await sender.Send(new DeleteEntiteCommand(id), ct);
        if (!result.IsSuccess)
        {
            return ToProblemDetails(result, httpContext);
        }

        return TypedResults.NoContent();
    }

    public static async Task<Ok<List<TypeEntiteDto>>> GetAllTypes(ISender sender)
    {
        var results = await sender.Send(new GetTypeEntitesQuery());
        return TypedResults.Ok(results);
    }

    public static async Task<Ok<List<TreeNodeDto>>> GetTree(ISender sender)
    {
        var tree = await sender.Send(new GetOrganisationTreeQuery());
        return TypedResults.Ok(tree);
    }

    public static async Task<IResult> GetEntiteById(ISender sender, int id)
    {
        if (id <= 0)
        {
            return TypedResults.BadRequest("Invalid identifier.");
        }

        var result = await sender.Send(new GetEntiteByIdQuery(id));
        return HandleResult(result);
    }

    private static IResult ToProblemDetails(ServiceResult<string> result, HttpContext httpContext)
    {
        var problemDetails = new ProblemDetails
        {
            Status = result.StatusCode,
            Title = "Conflit ou Erreur métier",
            Detail = result.ErrorMessage,
            Instance = httpContext.Request.Path
        };

        return TypedResults.Json(problemDetails, statusCode: result.StatusCode);
    }

    private static IResult HandleResult<T>(ServiceResult<T> result)
    {
        if (result is null)
        {
            return TypedResults.NotFound();
        }

        if (result.IsSuccess)
        {
            if (result.Value == null && result.StatusCode == 200)
            {
                return TypedResults.NoContent();
            }

            return TypedResults.Json(new { data = result.Value }, statusCode: result.StatusCode);
        }

        var problemDetails = new ProblemDetails
        {
            Status = result.StatusCode,
            Title = "Conflit ou Erreur métier",
            Detail = result.ErrorMessage
        };

        return TypedResults.Json(problemDetails, statusCode: result.StatusCode);
    }
}
