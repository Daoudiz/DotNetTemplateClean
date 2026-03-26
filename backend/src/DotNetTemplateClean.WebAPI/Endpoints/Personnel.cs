using DotNetTemplateClean.Application;
using DotNetTemplateClean.Domain;

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
        groupBuilder.MapPost(CreatePersonnel);
        groupBuilder.MapPut(UpdatePersonnel,"/{id}" );
        groupBuilder.MapDelete(DeletePersonnel, "/{id}").WithName("DeletePersonnel");
        groupBuilder.MapGet(GetStatutPersonnelEnum,"/statutpersonnel");
        
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

    [EndpointSummary("Create a new Personnel")]
    [EndpointDescription("Creates a new Personnel  using the provided data and returns the ID of the created personnel.")]
    public static async Task<Created<int>> CreatePersonnel(ISender sender, CreatePersonnelCommand command)
    {
        ArgumentNullException.ThrowIfNull(sender);

        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Personnel)}/{id}", id);
    }

    [EndpointSummary("Update a Personnel by ID")]
    [EndpointDescription("Updates the Personnel with the specified ID using the provided data. Returns 204 No Content if successful, or 404 Not Found if the Personnel does not exist.")]
    public static async Task<Results<NoContent, BadRequest>> UpdatePersonnel(ISender sender, int id, UpdatePersonnelCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(sender);

        if( id != command.Id)
        {
            return TypedResults.BadRequest();
        }

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete a Personnel by ID")]
    [EndpointDescription("Deletes the Personnel with the specified ID. Returns 204 No Content if successful, or 404 Not Found if the Personnel does not exist.")]
    public static async Task<NoContent> DeletePersonnel(ISender sender, int id)
    {
        ArgumentNullException.ThrowIfNull(sender);
        await sender.Send(new DeletePersonnelCommand(id));
        return TypedResults.NoContent();
    }

    public static async Task<Ok<List<EnumItemDto>>> GetStatutPersonnelEnum()
    {
        var result = EnumHelper.GetEnumWithDisplayNames<StatutPersonnel>().ToList();

        return TypedResults.Ok(result);
    }
}
