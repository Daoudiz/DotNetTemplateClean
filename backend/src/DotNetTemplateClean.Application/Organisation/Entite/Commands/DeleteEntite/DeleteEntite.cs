namespace DotNetTemplateClean.Application;

public record DeleteEntiteCommand(int Id) : IRequest<ServiceResult<string>>, IAuthorizeRequest
{
    public IReadOnlyCollection<string> Roles => ["Admin"];
}

public class DeleteEntiteCommandHandler(
    IApplicationDbContext context,
    IValidator<DeleteEntiteCommand> validator,
    IEntiteHierarchyService entiteHierarchyService)
    : IRequestHandler<DeleteEntiteCommand, ServiceResult<string>>
{
    public async Task<ServiceResult<string>> Handle(DeleteEntiteCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return ServiceResult.Failure<string>(
                string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)),
                400);
        }

        var existing = await context.Entites.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.Failure<string>(AppErrorMessages.Entite.EntiteNotFound, 404);
        }

        var descendantIds = await entiteHierarchyService.GetFlattenedChildEntityIds(request.Id).ConfigureAwait(false);
        if (descendantIds.Count > 1)
        {
            return ServiceResult.Failure<string>(AppErrorMessages.Entite.EntiteHasChildren, 409);
        }

        existing.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success(request.Id.ToString(CultureInfo.InvariantCulture));
    }
}
