
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace DotNetTemplateClean.Infrastructure;

//Configuration des Rôles
public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public const string ADMINROLEID = "341743f0-asd2-42de-afbf-59kmkkmk72cf6";

    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        builder.HasData(new IdentityRole
        {
            Id = ADMINROLEID,
            Name = "Admin",
            NormalizedName = "ADMIN",
            ConcurrencyStamp = ADMINROLEID
        });
    }
}

//Configuration de l'Utilisateur
public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public const string ADMINUSERID = "02174cf0-9412-4cfe-afbf-59f706d72cf6";

    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        var admin = new ApplicationUser
        {
            Id = ADMINUSERID,
            Matricule = 201210,
            FirstName = "Zakaria",
            LastName = "DAOUDI",
            UserName = "Zakaria",
            NormalizedUserName = "ZAKARIA",
            Email = "zakaria.daoudi@gmail.com",
            NormalizedEmail = "ZAKARIA.DAOUDI@GMAIL.COM",
            EmailConfirmed = true,
            EntiteId = 5,
            DateRecrutement = new DateTime(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc),
            SecurityStamp = "78641976-5602-4680-8260-705307771767" // GUID fixe pour éviter les updates inutiles
        };

        var hasher = new PasswordHasher<ApplicationUser>();
        admin.PasswordHash = hasher.HashPassword(admin, "A@Z200711");

        builder.HasData(admin);
    }
}

//Configuration de la relation User-Role
public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        builder.HasData(new IdentityUserRole<string>
        {
            RoleId = RoleConfiguration.ADMINROLEID,
            UserId = UserConfiguration.ADMINUSERID
        });
    }
}
