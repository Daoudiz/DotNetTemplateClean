using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetTemplateClean.Infrastructure;

public class OrganisationRelationsConfiguration :
    IEntityTypeConfiguration<AffectationPersonnel>,
    IEntityTypeConfiguration<Entite>,
    IEntityTypeConfiguration<Personnel>,
    IEntityTypeConfiguration<Fonction>
{
    public void Configure(EntityTypeBuilder<AffectationPersonnel> builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        // Configuration de l'association Ternaire (Priorite 1)
        builder.HasOne(ap => ap.Personnel)
               .WithMany(p => p.Affectations)
               .HasForeignKey(ap => ap.PersonnelId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ap => ap.Entite)
               .WithMany(e => e.Affectations)
               .HasForeignKey(ap => ap.EntiteId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ap => ap.Fonction)
               .WithMany(f => f.Affectations)
               .HasForeignKey(ap => ap.FonctionId)
               .OnDelete(DeleteBehavior.Restrict);

        // Query-oriented indexes for frequent filters on FK + active/soft-delete flags.
        builder.HasIndex(ap => new { ap.PersonnelId, ap.IsDeleted, ap.IsActive });
        builder.HasIndex(ap => new { ap.EntiteId, ap.IsDeleted, ap.IsActive });
    }

    public void Configure(EntityTypeBuilder<Entite> builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        builder.Property(e => e.Code)
               .HasMaxLength(250);

        builder.Property(e => e.Libelle)
               .HasMaxLength(250);

        builder.HasIndex(e => e.Code)
               .IsUnique();

        builder.HasIndex(e => e.Libelle)
               .IsUnique();

        // Configuration de la Hierarchie (Auto-rattachement)
        builder.HasOne(e => e.Rattachement)
               .WithMany(e => e.Children)
               .HasForeignKey(e => e.RattachementEntiteId)
               .OnDelete(DeleteBehavior.Restrict);

        // Relation avec TypeEntite
        builder.HasOne(e => e.TypeEntite)
               .WithMany(t => t.Entites)
               .HasForeignKey(e => e.TypeEntiteId)
               .OnDelete(DeleteBehavior.Restrict);

        // Query-oriented indexes for hierarchy/type filters with soft-delete.
        builder.HasIndex(e => new { e.RattachementEntiteId, e.IsDeleted });
        builder.HasIndex(e => new { e.TypeEntiteId, e.IsDeleted });

        // Relation avec le Personnel (Affectations)
        builder.HasMany(e => e.Personnel)
               .WithOne(p => p.Entite)
               .HasForeignKey(p => p.EntiteId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
    }

#pragma warning disable CA1822
    public void Configure(EntityTypeBuilder<Fonction> builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        // Config enums as strings for better readability in the database
        builder.Property(f => f.Domaine)
             .HasConversion<string>();

        builder.Property(f => f.Type)
             .HasConversion<string>();
    }

    // Configure enums for Personnel as strings for better readability in the database
    public void Configure(EntityTypeBuilder<Personnel> builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        builder.Property(p => p.Matricule)
               .HasMaxLength(100);

        builder.HasIndex(p => p.Matricule)
               .IsUnique();

        // Query-oriented index for branch/personnel filtering with soft-delete.
        builder.HasIndex(p => new { p.EntiteId, p.IsDeleted });

        builder.Property(p => p.Statut)
             .HasConversion<string>();

        builder.Property(p => p.DateRecrutement)
            .HasConversion(
                date => date.HasValue
                    ? date.Value.ToDateTime(TimeOnly.MinValue)
                    : (DateTime?)null,
                value => value.HasValue
                    ? DateOnly.FromDateTime(value.Value)
                    : null)
            .HasColumnName(nameof(Personnel.DateRecrutement));

        builder.Property(p => p.DateNaissance)
            .HasConversion(
                dateNaissance => dateNaissance == null
                    ? (DateTime?)null
                    : dateNaissance.Value.ToDateTime(TimeOnly.MinValue),
                value => value.HasValue
                    ? DateNaissance.Create(DateOnly.FromDateTime(value.Value))
                    : null)
            .HasColumnName(nameof(Personnel.DateNaissance));
    }
#pragma warning restore CA1822
}
