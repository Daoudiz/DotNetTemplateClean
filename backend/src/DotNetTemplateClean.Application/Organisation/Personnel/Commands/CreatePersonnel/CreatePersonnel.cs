namespace DotNetTemplateClean.Application;

public record CreatePersonnelCommand : IRequest<int>
{
    public required string Matricule { get; init; }
    public required string Nom { get; init; }
    public required string Prenom { get; init; }
    public DateOnly? DateRecrutement { get; init; }
    public DateTime? DateNaissance { get; init; }
    public string Email { get; init; } = string.Empty;
    public int EntiteId { get; init; }
    public string? Statut { get; init; }
    public string? Grade { get; init; }

    public bool CreateUser { get; init; }
    public string? UserRole { get; init; }

    public IList<CreateAffectationDto> Affectations { get; init; } = [];
}

public record CreateAffectationDto(int EntiteId, int FonctionId, DateTime DateDebut, string Nature);

public class CreatePersonnelCommandHandler(IApplicationDbContext context, IUserService userService)
    : IRequestHandler<CreatePersonnelCommand, int>
{
    public async Task<int> Handle(CreatePersonnelCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        int personnelId = 0;

        await context.ExecuteInTransactionAsync(async () =>
        {
            var dateNaissance = DateNaissance.Create(
                request.DateNaissance.HasValue
                    ? DateOnly.FromDateTime(request.DateNaissance.Value)
                    : null);

            var dateRecrutement = request.DateRecrutement.HasValue
                ? request.DateRecrutement.Value
                : throw new DomainException("La date de recrutement est obligatoire.");

            var entity = Personnel.Create(
                request.Matricule,
                request.Nom,
                request.Prenom,
                dateRecrutement,
                dateNaissance,
                request.Email,
                request.EntiteId,
                request.Statut,
                request.Grade);

            foreach (var aff in request.Affectations)
            {
                entity.Affectations.Add(new AffectationPersonnel
                {
                    EntiteId = aff.EntiteId,
                    FonctionId = aff.FonctionId,
                    DateDebutAffectation = aff.DateDebut,
                    Nature = aff.Nature,
                    IsActive = true
                });
            }

            if (request.CreateUser)
            {
                var user = new UserCreationDto
                {
                    Matricule = int.Parse(request.Matricule, CultureInfo.InvariantCulture),
                    FirstName = request.Prenom,
                    LastName = request.Nom,
                    DateRecrutement = request.DateRecrutement ?? DateOnly.FromDateTime(DateTime.Now),
                    Email = request.Email,
                    UserName = request.Email,
                    Password = request.Prenom + "@2026",
                    UserRole = request.UserRole!,
                    Service = request.EntiteId,
                    TwoFactorEnabled = false
                };

                var result = await userService.CreateUserWithRoleAsync(user);

                if (result.IsSuccess)
                {
                    entity.IdentityId = result.Value;
                }
            }

            context.Personnels.Add(entity);
            await context.SaveChangesAsync(cancellationToken);
            personnelId = entity.Id;

        }, cancellationToken);

        return personnelId;
    }
}
