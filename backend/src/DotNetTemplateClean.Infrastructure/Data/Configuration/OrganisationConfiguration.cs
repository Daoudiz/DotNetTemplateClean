

using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace DotNetTemplateClean.Infrastructure;

public class OrganisationRelationsConfiguration :
    IEntityTypeConfiguration<AffectationPersonnel>,
    IEntityTypeConfiguration<Entite>

    /*IEntityTypeConfiguration<TypeEntite>,
    IEntityTypeConfiguration<Fonction>*/
{
    public void Configure(EntityTypeBuilder<AffectationPersonnel> builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        // Configuration de l'association Ternaire (Priorité 1)
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
    }

    public void Configure(EntityTypeBuilder<Entite> builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        // Configuration de la Hiérarchie (Auto-rattachement)
        // Les annotations ont du mal avec la navigation inverse des enfants
        builder.HasOne(e => e.Rattachement)
               .WithMany(e => e.Children)
               .HasForeignKey(e => e.RattachementEntiteId)
               .OnDelete(DeleteBehavior.Restrict);

        // Relation avec TypeEntite
        builder.HasOne(e => e.TypeEntite)
               .WithMany(t => t.Entites)
               .HasForeignKey(e => e.TypeEntiteId)
               .OnDelete(DeleteBehavior.Restrict);

        // Relation avec les Utilisateurs (Sécurité)
        // On la configure ici car c'est l'Entite qui "possède" les users
        builder.HasMany(e => e.Users)
               .WithOne(u => u.Entite)
               .HasForeignKey(u => u.EntiteId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(); // Un utilisateur doit forcément appartenir à une entité

        //Relation avec le Personnel (Affectations)
        builder.HasMany(e => e.Personnel)
               .WithOne(p => p.Entite)
               .HasForeignKey(p => p.EntiteId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(); // Un personnel doit forcément appartenir à une entité

    }
#pragma warning disable CA1822 
    public void Configure (EntityTypeBuilder<Fonction> builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        //Config enums as strings for better readability in the database
        builder.Property(f => f.Domaine)
             .HasConversion<string>();

        builder.Property(f => f.Type)
             .HasConversion<string>();
    }

    //Configure enums for Personnel as strings for better readability in the database
    public void Configure(EntityTypeBuilder<Personnel> builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        builder.Property(p => p.Statut)
             .HasConversion<string>();
        
    }
#pragma warning restore CA1822  
}
