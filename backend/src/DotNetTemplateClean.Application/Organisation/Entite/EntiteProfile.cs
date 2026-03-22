namespace DotNetTemplateClean.Application;

public class OrganizationProfile : Profile
{
    public OrganizationProfile()
    {
        // ==========================================
        // SENS : LECTURE (Entite -> ResponseDto)
        // ==========================================
        CreateMap<Entite, OrganizationUnitResponseDto>()
            // L'aplatissement (Flattening) automatique :
            // AutoMapper cherche par convention "TypeEntite" + "Libelle" 
            // pour remplir la propriété "TypeEntiteLibelle".
            // On peut toutefois être explicite pour plus de clarté :
            .ForMember(dest => dest.TypeEntiteLibelle,
                        opt => opt.MapFrom(src => src.TypeEntite.Libelle))
            .ForMember(dest => dest.RattachementEntiteLibelle,
                        opt => opt.MapFrom(src => src.Rattachement!.Libelle));

        // ==========================================
        // SENS : ÉCRITURE (SaveDto -> Entite)
        // ==========================================
        CreateMap<OrganizationUnitSaveDto, Entite>(MemberList.Source)

            //  On ignore l'ID si c'est une création (Id est nul ou 0)
            .ForMember(dest => dest.Id, opt => opt.Condition(src => src.Id > 0))

            // IMPORTANT : On ignore les objets de navigation (TypeEntite, RattachementEntite)
            // car on veut seulement mapper les Foreign Keys (TypeEntiteId, RattachementEntiteId).
            // Si on ne les ignore pas, EF pourrait croire qu'on veut créer de nouveaux types d'entités.
            .ForMember(dest => dest.TypeEntite, opt => opt.Ignore())
            .ForMember(dest => dest.Rattachement, opt => opt.Ignore())

            //On ignore les champs d'audit pour ne pas écraser les valeurs auto-générées
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore());
    }
}
