
using Ardalis.GuardClauses;

namespace DotNetTemplateClean.Application;

public record DeletePersonnelCommand(int Id) : IRequest;

public class DeletePersonnelCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeletePersonnelCommand>
{
    public async Task Handle(DeletePersonnelCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        var entity = await context.Personnels.FindAsync([request.Id], cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        context.Personnels.Remove(entity);
        //remove affectations
        var affectations = context.AffectationsPersonnel.Where(a => a.PersonnelId == request.Id);

        context.AffectationsPersonnel.RemoveRange(affectations);

        await context.SaveChangesAsync(cancellationToken);
        
    }
}
