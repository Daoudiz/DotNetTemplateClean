

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

        // Seeding data
       /* builder.HasData(
            new Entite { Id = 1, Code = "DG", Libelle = "Direcion générale", RattachementEntiteId = null , TypeEntiteId = 1, CreatedDate = new DateTime(2026, 2, 22, 12, 41, 55, 630, DateTimeKind.Utc).AddTicks(1110) },
            new Entite { Id = 2, Code = "DS", Libelle = "Direction support", RattachementEntiteId = 1 , TypeEntiteId = 2, CreatedDate = new DateTime(2026, 2, 22, 12, 41, 55, 630, DateTimeKind.Utc).AddTicks(1110) },
            new Entite { Id = 3, Code = "DAF", Libelle = "Division administrative et financière", RattachementEntiteId = 2 , TypeEntiteId = 3, CreatedDate = new DateTime(2026, 2, 22, 12, 41, 55, 630, DateTimeKind.Utc).AddTicks(1110) },
            new Entite { Id = 4, Code = "DTL", Libelle = "Division transport et logistique", RattachementEntiteId = 2 , TypeEntiteId = 3, CreatedDate = new DateTime(2026, 2, 22, 12, 41, 55, 630, DateTimeKind.Utc).AddTicks(1110) },
            new Entite { Id = 5, Code = "SSI", Libelle = "Service système d'information", RattachementEntiteId = 4 , TypeEntiteId = 4, CreatedDate = new DateTime(2026, 2, 22, 12, 41, 55, 630, DateTimeKind.Utc).AddTicks(1110) }
           );*/
    }

   /* public  void Configure(EntityTypeBuilder<TypeEntite> builder)
    {
        builder.HasData(            
            new TypeEntite { Id = 1, Code = "DG", Libelle = "Direction générale", Rang = 1, CreatedDate= new DateTime(2026, 2, 22, 12, 41, 55, 630, DateTimeKind.Utc).AddTicks(1110)},
            new TypeEntite { Id = 2, Code = "DIR", Libelle = "Direction", Rang = 2, CreatedDate = new DateTime(2026, 2, 22, 12, 41, 55, 630, DateTimeKind.Utc).AddTicks(1110) },
            new TypeEntite { Id = 3, Code = "DIV", Libelle = "Division", Rang = 3, CreatedDate = new DateTime(2026, 2, 22, 12, 41, 55, 630, DateTimeKind.Utc).AddTicks(1110) },
            new TypeEntite { Id = 4, Code = "SRV", Libelle = "Service", Rang = 4, CreatedDate = new DateTime(2026, 2, 22, 12, 41, 55, 630, DateTimeKind.Utc).AddTicks(1110) },
            new TypeEntite { Id = 5, Code = "LAB", Libelle = "Laboratoire", Rang = 5, CreatedDate = new DateTime(2026, 2, 22, 12, 41, 55, 630, DateTimeKind.Utc).AddTicks(1110) }
           );
    }*/

   /* public void Configure(EntityTypeBuilder<Fonction> builder)
    {
        builder.HasData(
            new Fonction { Id=1, Code ="DG", Designation = "Directeur Générale", Domaine = "Management", Type = "Management", TypeEntiteId = 1, CreatedDate = new DateTime(2026, 2, 22, 12, 41, 55, 630, DateTimeKind.Utc).AddTicks(1110) },
            new Fonction { Id=2, Code ="DR", Designation = "Directeur", Domaine = "Management", Type = "Management", TypeEntiteId = 2, CreatedDate = new DateTime(2026, 2, 22, 12, 41, 55, 630, DateTimeKind.Utc).AddTicks(1110) },
            new Fonction { Id=3, Code ="CDIV", Designation = "Chef de division", Domaine = "Management", Type = "Management", TypeEntiteId = 3, CreatedDate = new DateTime(2026, 2, 22, 12, 41, 55, 630, DateTimeKind.Utc).AddTicks(1110) },
            new Fonction { Id=4, Code ="CDS", Designation = "Chef de service", Domaine = "Management", Type = "Management", TypeEntiteId = 4, CreatedDate = new DateTime(2026, 2, 22, 12, 41, 55, 630, DateTimeKind.Utc).AddTicks(1110) },
            new Fonction { Id=5, Code ="RLAB", Designation = "Responsable laboratoire", Domaine = "Technique", Type = "Métier", TypeEntiteId = 5, CreatedDate = new DateTime(2026, 2, 22, 12, 41, 55, 630, DateTimeKind.Utc).AddTicks(1110) },
            new Fonction { Id=6, Code ="AQ", Designation = "Attaché qualité", Domaine = "Qualité", Type = "Métier", TypeEntiteId = null, CreatedDate = new DateTime(2026, 2, 22, 12, 41, 55, 630, DateTimeKind.Utc).AddTicks(1110) },
            new Fonction { Id=7, Code ="OPL", Designation = "Opérateur laboratoire", Domaine = "Technique", Type = "Métier", TypeEntiteId = null, CreatedDate = new DateTime(2026, 2, 22, 12, 41, 55, 630, DateTimeKind.Utc).AddTicks(1110) },
            new Fonction { Id=8, Code ="MET", Designation = "Responsable métrologie", Domaine = "Métrologie", Type = "Métier", TypeEntiteId = null, CreatedDate = new DateTime(2026, 2, 22, 12, 41, 55, 630, DateTimeKind.Utc).AddTicks(1110) }
           );
    }*/
}
