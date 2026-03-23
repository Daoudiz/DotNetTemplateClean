namespace DotNetTemplateClean.Application;
public class CreateEntiteValidator : AbstractValidator<OrganizationUnitSaveDto>
{
    public CreateEntiteValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(250)
            .MustAsync(async (code, ct) =>
                !await context.Entites.AnyAsync(e => e.Code == code, ct))
            .WithMessage("Le code existe déjà.");

        RuleFor(x => x.Libelle)
            .NotEmpty()
            .MaximumLength(250)
            .MustAsync(async (libelle, ct) =>
                !await context.Entites.AnyAsync(e => e.Libelle == libelle, ct))
            .WithMessage("Le libellé existe déjà.");

        RuleFor(x => x.TypeEntiteId)
            .GreaterThan(0);

       
    }
}
