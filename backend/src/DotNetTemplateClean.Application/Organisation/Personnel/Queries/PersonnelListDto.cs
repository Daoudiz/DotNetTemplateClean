

namespace DotNetTemplateClean.Application;

public class PersonnelListDto
{
    public int Id { get; init; }
    public required string Matricule { get; init; }
    public  required string Nom { get; init; }
    public  required string Prenom { get; init; }
    public DateTime? DateRecrutement { get; init; }
    public DateTime? DateNaissance { get; init; }
    public string? Statut { get; init; }
    public string? Grade { get; init; }

    // La liste des affectations liées
    public IReadOnlyCollection<AffectationDto> Affectations { get; init; } = [];

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Personnel, PersonnelListDto>();
        }
    }
}
