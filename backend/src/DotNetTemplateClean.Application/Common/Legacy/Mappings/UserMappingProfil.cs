namespace DotNetTemplateClean.Application;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<CreateViewModel, UserCreationDto>()
            .ForMember(
                destination => destination.MustChangePasswordOnFirstLogin,
                options => options.MapFrom(_ => false));

        CreateMap<UpdateUserViewModel, UserUpdateDto>();
    }
}
