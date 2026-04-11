
using Ardalis.GuardClauses;

namespace DotNetTemplateClean.Application;

public record GetPersonnelByIdQuery(int Id) : IRequest<PersonnelDetailsDto>;

public class GetPersonnelByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetPersonnelByIdQuery, PersonnelDetailsDto>
{
    public async Task<PersonnelDetailsDto> Handle(GetPersonnelByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var personnel = await context.Personnels
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .ProjectTo<PersonnelDetailsDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, personnel);

        return personnel;
    }
}

public class PersonnelDetailsDto
{
    public int Id { get; init; }
    public required string Matricule { get; init; }
    public required string Nom { get; init; }
    public required string Prenom { get; init; }
    public DateTime? DateRecrutement { get; init; }
    public DateTime? DateNaissance { get; init; }
    public required string Email { get; init; }
    public int EntiteId { get; init; }
    public string? Statut { get; init; }
    public string? Grade { get; init; }
    public IReadOnlyCollection<PersonnelAffectationDetailsDto> Affectations { get; init; } = [];

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Personnel, PersonnelDetailsDto>();
        }
    }
}

public class PersonnelAffectationDetailsDto
{
    public int Id { get; init; }
    public int EntiteId { get; init; }
    public int FonctionId { get; init; }
    public DateTime DateDebut { get; init; }
    public DateTime? DateFinAffectation { get; init; }
    public required string Nature { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<AffectationPersonnel, PersonnelAffectationDetailsDto>()
                .ForMember(d => d.DateDebut, opt => opt.MapFrom(s => s.DateDebutAffectation));
        }
    }
}
