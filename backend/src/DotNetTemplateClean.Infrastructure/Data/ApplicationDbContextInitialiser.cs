
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace DotNetTemplateClean.Infrastructure;

public static class InitialiserExtensions
{
    public static async Task InitialiseDbProdAsync(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app, nameof(app));

        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();

        await initialiser.SeedDevAsync();
    }


    public static async Task InitialiseDbDevAsync(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app, nameof(app));

        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.SeedDevAsync();

    }
}

internal class ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger,
                                                ApplicationDbContext context,
                                                UserManager<ApplicationUser> userManager,
                                                RoleManager<IdentityRole> roleManager)
{
        public async Task InitialiseAsync()
        {
            try
            {
                    await context.Database.MigrateAsync();
               
            }
            catch (Exception ex)
            {
#pragma warning disable CA1848 // Template should be a static expression
            logger.LogError(ex, "An error occurred while initialising the database.");
                throw;
#pragma warning restore CA1848 // Template should be a static expression
        }
    }

    public async Task SeedDevAsync() => await TrySeedAsync();

    public async Task TrySeedAsync()
        {
            // On ne vérifie qu'une seule chose : si la base est vide
            if (await context.TypeEntites.AnyAsync()) return;

            var seedDate = new DateTime(2026, 2, 22, 12, 41, 55, DateTimeKind.Utc);

            // LES TYPES
            var dgType = new TypeEntite { Code = "DG", Libelle = "Direction générale", Rang = 1, CreatedDate = seedDate };
            var dirType = new TypeEntite { Code = "DIR", Libelle = "Direction", Rang = 2, CreatedDate = seedDate };
            var divType = new TypeEntite { Code = "DIV", Libelle = "Division", Rang = 3, CreatedDate = seedDate };
            var srvType = new TypeEntite { Code = "SRV", Libelle = "Service", Rang = 4, CreatedDate = seedDate };
            var labType = new TypeEntite { Code = "LAB", Libelle = "Laboratoire", Rang = 5, CreatedDate = seedDate };

            // LES ENTITÉS (Rattachées directement aux variables ci-dessus)
            var dg = new Entite { Code = "DG", Libelle = "Direction générale", TypeEntite = dgType, CreatedDate = seedDate };
            var ds = new Entite { Code = "DS", Libelle = "Direction support", Rattachement = dg, TypeEntite = dirType, CreatedDate = seedDate };
            var daf = new Entite { Code = "DAF", Libelle = "Division administrative et financière", Rattachement = ds, TypeEntite = divType, CreatedDate = seedDate };
            var dtl = new Entite { Code = "DTL", Libelle = "Division transport et logistique", Rattachement = ds, TypeEntite = divType, CreatedDate = seedDate };
            var ssi = new Entite { Code = "SSI", Libelle = "Service système d'information", Rattachement = dtl, TypeEntite = srvType, CreatedDate = seedDate };

            // LES FONCTIONS
            var fonctions = new List<Fonction>
            {
                new() { Code = "DG", Designation = "Directeur Générale", Domaine = FonctionDomaine.Management, Type = TypeFonction.Management, TypeEntite = dgType, CreatedDate = seedDate },
                new() { Code = "DR", Designation = "Directeur", Domaine = FonctionDomaine.Management, Type = TypeFonction.Management, TypeEntite = dirType, CreatedDate = seedDate },
                new() { Code = "CDIV", Designation = "Chef de division", Domaine = FonctionDomaine.Management, Type = TypeFonction.Management, TypeEntite = divType, CreatedDate = seedDate },
                new() { Code = "CDS", Designation = "Chef de service", Domaine = FonctionDomaine.Management, Type = TypeFonction.Management, TypeEntite = srvType, CreatedDate = seedDate },
                new() { Code = "RLAB", Designation = "Responsable laboratoire", Domaine = FonctionDomaine.ManagementLaboratoire, Type = TypeFonction.Technique, TypeEntite = labType, CreatedDate = seedDate },
                new() { Code = "AQ", Designation = "Attaché qualité", Domaine = FonctionDomaine.Qualité, Type = TypeFonction.Support, CreatedDate = seedDate },
                new() { Code = "OPL", Designation = "Opérateur laboratoire", Domaine = FonctionDomaine.RadioLogie, Type = TypeFonction.Technique, CreatedDate = seedDate },
                new() { Code = "MET", Designation = "Responsable métrologie", Domaine = FonctionDomaine.Métrologie, Type = TypeFonction.Technique, CreatedDate = seedDate }
            };

            // On ajoute tout dans le contexte
            context.TypeEntites.AddRange(dgType, dirType, divType, srvType, labType);
            context.Entites.AddRange(dg, ds, daf, dtl, ssi);
            context.Fonctions.AddRange(fonctions);

            // Une seule transaction : Tout passe ou tout casse
            await context.SaveChangesAsync();

            // Default roles
            var administratorRole = new IdentityRole
            {
                Name = "Admin",
                NormalizedName = "ADMIN",                 
            };

            if (roleManager.Roles.All(r => r.Name != administratorRole.Name))
            {
                await roleManager.CreateAsync(administratorRole);
            }

            var admin = new ApplicationUser
            {           
                Matricule = 201210,
                FirstName = "Zakaria",
                LastName = "DAOUDI",
                UserName = "Zakaria",
                NormalizedUserName = "ZAKARIA",
                Email = "zakaria.daoudi@gmail.com",
                NormalizedEmail = "ZAKARIA.DAOUDI@GMAIL.COM",
                EmailConfirmed = true,
                EntiteId = 5,
                DateRecrutement  = DateOnly.FromDateTime(DateTime.UtcNow)                
            };

            if (userManager.Users.All(u => u.UserName != admin.UserName))
            {
                await userManager.CreateAsync(admin, "Zakaria@1977");
                if (!string.IsNullOrWhiteSpace(administratorRole.Name))
                {
                    await userManager.AddToRolesAsync(admin, [administratorRole.Name]);
                }
            }


    }
}
