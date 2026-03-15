
namespace DotNetTemplateClean.Application;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // Règle pour la CREATION d'un utilisateur
        CreateMap<CreateViewModel, UserCreationDto>();
        CreateMap<UpdateUserViewModel, UserUpdateDto>();

    }
}
