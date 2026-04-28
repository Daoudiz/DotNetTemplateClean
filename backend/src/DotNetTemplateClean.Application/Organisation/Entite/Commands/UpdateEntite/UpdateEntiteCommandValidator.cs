namespace DotNetTemplateClean.Application;

public class UpdateEntiteCommandValidator : AbstractValidator<UpdateEntiteCommand>
{
    public UpdateEntiteCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull();

        When(x => x.Dto is not null, () =>
        {
            RuleFor(x => x.Dto.Id)
                .NotNull()
                .GreaterThan(0);

            RuleFor(x => x.Dto.Code)
                .NotEmpty()
                .MaximumLength(250);

            RuleFor(x => x.Dto.Libelle)
                .NotEmpty()
                .MaximumLength(250);

            RuleFor(x => x.Dto.TypeEntiteId)
                .GreaterThan(0);
        });
    }
}
