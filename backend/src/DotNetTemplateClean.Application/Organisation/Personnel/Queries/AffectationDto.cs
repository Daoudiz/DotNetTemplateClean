
namespace DotNetTemplateClean.Application;

public class AffectationDto
{
    public int Id { get; init; }
    public DateTime DateDebutAffectation { get; init; }
    public DateTime? DateFinAffectation { get; init; }

    // Propriétés aplaties
    public string? FonctionLibelle { get; init; }
    public string? EntiteLibelle { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<AffectationPersonnel, AffectationDto>()
                // AutoMapper aplatit automatiquement si les noms correspondent :
                // Affectation.Fonction.Designation -> FonctionLibelle
                // Affectation.Entite.Libelle -> EntiteLibelle
                // Mais si tes noms diffèrent, on le précise explicitement :
                .ForMember(d => d.FonctionLibelle, opt => opt.MapFrom(s => s.Fonction.Designation))
                .ForMember(d => d.EntiteLibelle, opt => opt.MapFrom(s => s.Entite.Libelle));
        }
    }
}
