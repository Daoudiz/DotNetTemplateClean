

namespace DotNetTemplateClean.Infrastructure;

public static class  ReferenceDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        // On ne vérifie qu'une seule chose : si la base est vide
        if (await db.TypeEntites.AnyAsync()) return;

        var seedDate = new DateTime(2026, 2, 22, 12, 41, 55, DateTimeKind.Utc);

        //LES TYPES
        var dgType = new TypeEntite { Code = "DG", Libelle = "Direction générale", Rang = 1, CreatedDate = seedDate };
        var dirType = new TypeEntite { Code = "DIR", Libelle = "Direction", Rang = 2, CreatedDate = seedDate };
        var divType = new TypeEntite { Code = "DIV", Libelle = "Division", Rang = 3, CreatedDate = seedDate };
        var srvType = new TypeEntite { Code = "SRV", Libelle = "Service", Rang = 4, CreatedDate = seedDate };
        var labType = new TypeEntite { Code = "LAB", Libelle = "Laboratoire", Rang = 5, CreatedDate = seedDate };

        //LES ENTITÉS (Rattachées directement aux variables ci-dessus)
        var dg = new Entite { Code = "DG", Libelle = "Direction générale", TypeEntite = dgType, CreatedDate = seedDate };
        var ds = new Entite { Code = "DS", Libelle = "Direction support", Rattachement = dg, TypeEntite = dirType, CreatedDate = seedDate };
        var daf = new Entite { Code = "DAF", Libelle = "Division administrative et financière", Rattachement = ds, TypeEntite = divType, CreatedDate = seedDate };
        var dtl = new Entite { Code = "DTL", Libelle = "Division transport et logistique", Rattachement = ds, TypeEntite = divType, CreatedDate = seedDate };
        var ssi = new Entite { Code = "SSI", Libelle = "Service système d'information", Rattachement = dtl, TypeEntite = srvType, CreatedDate = seedDate };

        //LES FONCTIONS
        var fonctions = new List<Fonction>
         {
             new() { Code = "DG", Designation = "Directeur Générale", Domaine = "Management", Type = "Management", TypeEntite = dgType, CreatedDate = seedDate },
             new() { Code = "DR", Designation = "Directeur", Domaine = "Management", Type = "Management", TypeEntite = dirType, CreatedDate = seedDate },
             new() { Code = "CDIV", Designation = "Chef de division", Domaine = "Management", Type = "Management", TypeEntite = divType, CreatedDate = seedDate },
             new() { Code = "CDS", Designation = "Chef de service", Domaine = "Management", Type = "Management", TypeEntite = srvType, CreatedDate = seedDate },
             new() { Code = "RLAB", Designation = "Responsable laboratoire", Domaine = "Technique", Type = "Métier", TypeEntite = labType, CreatedDate = seedDate },
             new() { Code = "AQ", Designation = "Attaché qualité", Domaine = "Qualité", Type = "Métier", CreatedDate = seedDate },
             new() { Code = "OPL", Designation = "Opérateur laboratoire", Domaine = "Technique", Type = "Métier", CreatedDate = seedDate },
             new() { Code = "MET", Designation = "Responsable métrologie", Domaine = "Métrologie", Type = "Métier", CreatedDate = seedDate }
         };

        // On ajoute tout dans le contexte
        db.TypeEntites.AddRange(dgType, dirType, divType, srvType, labType);
        db.Entites.AddRange(dg, ds, daf, dtl, ssi);
        db.Fonctions.AddRange(fonctions);

        // Une seule transaction : Tout passe ou tout casse
        await db.SaveChangesAsync();
    }

}
