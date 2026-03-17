using FluentValidation;

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

        RuleFor(x => x)
            .MustAsync(async (dto, ct) =>
            {
                if (dto.RattachementEntiteId == null)
                    return true;

                var parent = await context.Entites
                    .Where(e => e.Id == dto.RattachementEntiteId)
                    .Select(e => new { e.TypeEntiteId })
                    .FirstOrDefaultAsync(ct);

                if (parent == null) return false;

                return parent.TypeEntiteId > dto.TypeEntiteId;
            })
            .WithMessage("Le rang du parent est invalide.");
    }
}
