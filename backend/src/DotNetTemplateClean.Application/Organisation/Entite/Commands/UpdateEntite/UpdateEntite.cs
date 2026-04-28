namespace DotNetTemplateClean.Application;

public record UpdateEntiteCommand(OrganizationUnitSaveDto Dto) : IRequest<ServiceResult<string>>, IAuthorizeRequest
{
    public IReadOnlyCollection<string> Roles => ["Admin"];
}

public class UpdateEntiteCommandHandler(
    IApplicationDbContext context,
    IMapper mapper,
    IValidator<UpdateEntiteCommand> validator,
    IEntiteRulesService entiteRulesService)
    : IRequestHandler<UpdateEntiteCommand, ServiceResult<string>>
{
    public async Task<ServiceResult<string>> Handle(UpdateEntiteCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return ServiceResult.Failure<string>(
                string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)),
                400);
        }

        var dto = request.Dto;
        var existing = await context.Entites.FirstOrDefaultAsync(e => e.Id == dto.Id!.Value, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.Failure<string>(AppErrorMessages.Entite.EntiteNotFound, 404);
        }

        var uniquenessFailure = await entiteRulesService
            .ValidateUpdateUniquenessAsync(existing, dto, cancellationToken)
            .ConfigureAwait(false);
        if (uniquenessFailure is not null)
        {
            return uniquenessFailure;
        }

        var parentRankFailure = await entiteRulesService
            .ValidateParentRankAsync(dto.TypeEntiteId, dto.RattachementEntiteId, cancellationToken)
            .ConfigureAwait(false);
        if (parentRankFailure is not null)
        {
            return parentRankFailure;
        }

        mapper.Map(dto, existing);
        await context.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success(existing.Id.ToString(CultureInfo.InvariantCulture));
    }
}
