namespace DotNetTemplateClean.Application;

public record CreateEntiteCommand(OrganizationUnitSaveDto Dto) : IRequest<ServiceResult<string>>, IAuthorizeRequest
{
    public IReadOnlyCollection<string> Roles => ["Admin"];
}

public class CreateEntiteCommandHandler(
    IApplicationDbContext context,
    IMapper mapper,
    IValidator<CreateEntiteCommand> validator,
    IEntiteRulesService entiteRulesService)
    : IRequestHandler<CreateEntiteCommand, ServiceResult<string>>
{
    public async Task<ServiceResult<string>> Handle(CreateEntiteCommand request, CancellationToken cancellationToken)
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
        var uniquenessFailure = await entiteRulesService
            .ValidateCreateUniquenessAsync(dto.Code, dto.Libelle, cancellationToken)
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

        var entite = mapper.Map<Entite>(dto);
        await context.Entites.AddAsync(entite, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success(entite.Id.ToString(CultureInfo.InvariantCulture));
    }
}
