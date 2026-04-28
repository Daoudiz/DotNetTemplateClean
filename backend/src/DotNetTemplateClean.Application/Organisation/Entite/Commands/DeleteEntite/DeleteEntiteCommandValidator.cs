namespace DotNetTemplateClean.Application;

public class DeleteEntiteCommandValidator : AbstractValidator<DeleteEntiteCommand>
{
    public DeleteEntiteCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Identificant invalide");
    }
}
