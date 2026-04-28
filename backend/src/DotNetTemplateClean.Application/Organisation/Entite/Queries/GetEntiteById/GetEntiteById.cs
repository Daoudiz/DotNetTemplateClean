namespace DotNetTemplateClean.Application;

public record GetEntiteByIdQuery(int Id) : IRequest<ServiceResult<Entite>>;

public class GetEntiteByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetEntiteByIdQuery, ServiceResult<Entite>>
{
    public async Task<ServiceResult<Entite>> Handle(GetEntiteByIdQuery request, CancellationToken cancellationToken)
    {
        var entite = await context.Entites
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entite is null)
        {
            return ServiceResult.Failure<Entite>(AppErrorMessages.Entite.EntiteNotFound, 404);
        }

        return ServiceResult.Success(entite);
    }
}
