using DotNetTemplateClean.Application;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DotNetTemplateClean.WebAPI;

public class Fonctions : IEndpointGroup
{
    public static string RoutePrefix => "/api/fonctions";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        groupBuilder.MapGet(GetFonctionsTree, "/tree");
    }

    public static async Task<Ok<List<PrimeNgTreeNodeDto>>> GetFonctionsTree(ISender sender)
    {
        ArgumentNullException.ThrowIfNull(sender);

        var result = await sender.Send(new GetFonctionsTreeQuery());

        return TypedResults.Ok(result);
    }
}
