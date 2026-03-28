
namespace DotNetTemplateClean.Application;

public record CreatePersonnelCommand : IRequest<int>
{
    public required string Matricule { get; init; }
    public required string Nom { get; init; }
    public required string Prenom { get; init; }
    public DateTime? DateRecrutement { get; init; }
    public DateTime? DateNaissance { get; init; }
    public string Email { get; init; } = string.Empty;
    public int EntiteId { get; init; } // ID de l'entité d'affectation principale
    public string? Statut { get; init; }
    public string? Grade { get; init; }

    public bool CreateUser { get; init; }  // Option pour créer un compte utilisateur lié

    // ID role pour l'attribution d'un rôle lors de la création du compte utilisateur
    public string? UserRole { get; init; }

    // Liste des affectations initiales
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
            
            //Instanciation de l'entité Personnel
            var entity = new Personnel
            {
                Matricule = request.Matricule,
                Nom = request.Nom,
                Prenom = request.Prenom,
                DateRecrutement = request.DateRecrutement,
                DateNaissance = request.DateNaissance,
                Statut = request.Statut,
                Grade = request.Grade,
                EntiteId = request.EntiteId
            };

            // Création des affectations liées
            foreach (var aff in request.Affectations)
            {
                entity.Affectations.Add(new AffectationPersonnel
                {
                    EntiteId = aff.EntiteId,
                    FonctionId = aff.FonctionId,
                    DateDebutAffectation = aff.DateDebut,
                    Nature = aff.Nature
                    // Le PersonnelId sera injecté automatiquement par EF Core lors du Save
                });
            }

            

            if (request.CreateUser)
            {
                var user = new UserCreationDto
                {
                    Matricule = int.Parse(request.Matricule, CultureInfo.InvariantCulture),
                    FirstName = request.Prenom,
                    LastName = request.Nom,
                    DateRecrutement = request.DateRecrutement ?? DateTime.Now,
                    Email = request.Email,
                    UserName = request.Email, // Utiliser l'email comme nom d'utilisateur
                    Password = request.Prenom + "@2026", // Mot de passe par défaut (à changer à la première connexion)
                    UserRole = request.UserRole!,
                    Service = request.EntiteId, // Associer l'utilisateur à l'entité d'affectation principale
                    TwoFactorEnabled = false
                };

                var result = await userService.CreateUserWithRoleAsync(user);

                if (result.IsSuccess)
                {
                   
                    // Associer le Personnel à l'utilisateur créé
                    entity.IdentityId = result.Value;
                    context.Personnels.Add(entity);
                    await context.SaveChangesAsync(cancellationToken);


                }
                else
                {

                    throw new InvalidOperationException($"Failed to create user: {result.ErrorMessage}");
                }

            }

            personnelId = entity.Id;

        }, cancellationToken);

        return personnelId;

    }
}
