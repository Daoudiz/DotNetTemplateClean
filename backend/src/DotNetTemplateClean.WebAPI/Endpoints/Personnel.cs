using CleanArchitecture.Application.Common.Models;

using DotNetTemplateClean.Application;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DotNetTemplateClean.WebAPI;

public class Personnel: IEndpointGroup
{
    public static string RoutePrefix => "/api/personnel";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        // On verrouille tout le groupe pour les Admins uniquement
        groupBuilder.RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        // Définition de la route GET
        groupBuilder.MapGet(GetPersonnels);
    }

    public static async Task<Ok<PaginatedList<PersonnelListDto>>> GetPersonnels(
        ISender sender,
        [AsParameters] GetPersonnelsWithFiltersQuery query)
    {
        ArgumentNullException.ThrowIfNull(sender);

        // [AsParameters] lie automatiquement les params d'URL (?SearchTerm=...) à la Query
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }
}
