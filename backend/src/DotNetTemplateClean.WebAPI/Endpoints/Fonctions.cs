using DotNetTemplateClean.Application;
using DotNetTemplateClean.Domain;

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
        groupBuilder.MapGet(GetNatureFonctionEnum, "/naturefonction");
    }

    public static async Task<Ok<List<PrimeNgTreeNodeDto>>> GetFonctionsTree(ISender sender)
    {
        ArgumentNullException.ThrowIfNull(sender);

        var result = await sender.Send(new GetFonctionsTreeQuery());

        return TypedResults.Ok(result);
    }

    public static Ok<List<EnumItemDto>> GetNatureFonctionEnum()
    {
        var result = EnumHelper.GetEnumWithDisplayNames<NatureFonction>().ToList();

        return TypedResults.Ok(result);
    }
}
