
using Ardalis.GuardClauses;


namespace DotNetTemplateClean.Application;

public record DeletePersonnelCommand(int Id) : IRequest;

public class DeletePersonnelCommandHandler(IApplicationDbContext context, IUserService userService)
    : IRequestHandler<DeletePersonnelCommand>
{
    //public async Task Handle(DeletePersonnelCommand request, CancellationToken cancellationToken)
    //{
    //    ArgumentNullException.ThrowIfNull(request, nameof(request));
    //    var entity = await context.Personnels.FindAsync([request.Id], cancellationToken);

    //    Guard.Against.NotFound(request.Id, entity);

    //    context.Personnels.Remove(entity);
    //    //remove affectations
    //    var affectations = context.AffectationsPersonnel.Where(a => a.PersonnelId == request.Id);

    //    context.AffectationsPersonnel.RemoveRange(affectations);

    //    await context.SaveChangesAsync(cancellationToken);

    //}

    public async Task Handle(DeletePersonnelCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));        

        var entity = await context.Personnels
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        await context.ExecuteInTransactionAsync(async () =>
        {
            // Soft delete du personnel
            entity.IsDeleted = true;

            // Soft delete des affectations liées
            var affectations = await context.AffectationsPersonnel
                .Where(a => a.PersonnelId == request.Id)
                .ToListAsync(cancellationToken);

            foreach (var affectation in affectations)
            {
                affectation.IsDeleted = true;
            }

            // Deactivate associated Identity user if present
            if (!string.IsNullOrWhiteSpace(entity.IdentityId))
            {
                var userResult = await userService.DeleteUserAsync(entity.IdentityId, string.Empty);
                if (!userResult.IsSuccess)
                {
                    // Any failure must abort the whole transaction
                    throw new InvalidOperationException($"Impossible de désactiver l'utilisateur: {userResult.ErrorMessage}");
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
    }
}
