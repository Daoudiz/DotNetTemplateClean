using CsvHelper;
using CsvHelper.Configuration;
using System.Text.Json;
using System.Globalization;

namespace DotNetTemplateClean.Infrastructure;


public static class DevSeeder
{

    public static async Task SeedAsync(ApplicationDbContext db)
    {
        // On ne vérifie qu'une seule chose : si la base est vide
        if (await db.TypeEntites.AnyAsync()) return;

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

    //public static async Task SeedFromCsvAsync(ApplicationDbContext db)
    //{
    //    if (await db.TypeEntites.AnyAsync()) return;

    //    // Chemin vers ton fichier (assure-toi qu'il est copié dans le dossier de build)
    //    var filePath = Path.Combine(AppContext.BaseDirectory, "SeedData", "TypeEntites.csv");

    //    var lines = await File.ReadAllLinesAsync(filePath);

    //    // On saute la première ligne (l'entête)
    //    foreach (var line in lines.Skip(1))
    //    {
    //        var columns = line.Split(';'); // On sépare par point-virgule

    //        db.TypeEntites.Add(new TypeEntite
    //        {
    //            Code = columns[0],
    //            Libelle = columns[1],
    //            Rang = int.Parse(columns[2]),
    //            CreatedDate = DateTime.UtcNow
    //        });
    //    }

    //    await db.SaveChangesAsync();
    //}

    //public static async Task SeedUsingCsvHelperAsync(ApplicationDbContext db, string fileName)
    //{
    //    //Appliquer les migrations d'abord (Indispensable pour la Prod)
    //    await db.Database.MigrateAsync();

    //    // Config pour lire le format CSV français (souvent avec des ;)
    //    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
    //    {
    //        Delimiter = ";", // Important pour les exports Excel FR
    //        PrepareHeaderForMatch = args => args.Header.ToLower(), // Ignore la casse
    //    };

    //    //SEED DES TYPES D'ENTITÉS
    //    if (!await db.TypeEntites.AnyAsync())
    //    {
    //        using var reader = new StreamReader(Path.Combine(AppContext.BaseDirectory, "SeedData", fileName));
    //        using var csv = new CsvReader(reader, config);

    //        var records = csv.GetRecords<TypeEntite>().ToList();
    //        records.ForEach(r => r.CreatedDate = DateTime.UtcNow);

    //        db.TypeEntites.AddRange(records);
    //        await db.SaveChangesAsync();
    //    }

    //    //SEED DES FONCTIONS
    //    if (!await db.Fonctions.AnyAsync())
    //    {
    //        using var reader = new StreamReader(Path.Combine(AppContext.BaseDirectory, "SeedData", "Fonctions.csv"));
    //        using var csv = new CsvReader(reader, config);

    //        var records = csv.GetRecords<Fonction>().ToList();
    //        records.ForEach(r => r.CreatedDate = DateTime.UtcNow);

    //        db.Fonctions.AddRange(records);
    //        await db.SaveChangesAsync();
    //    }
    //}

    //public static async Task SeedFromJsonAsync(ApplicationDbContext db)
    //{
    //    // 1. On migre d'abord
    //    await db.Database.MigrateAsync();

    //    // 2. On charge les fichiers JSON
    //    await SeedTableAsync<TypeEntite>(db, "TypeEntites.json");
    //    await SeedTableAsync<Entite>(db, "Entities.json");
    //    await SeedTableAsync<Fonction>(db, "Fonctions.json");
    //    // Ajouter une 11ème table prend littéralement 1 ligne de code ici.

    //    await db.SaveChangesAsync();
    //}

    //private static async Task SeedTableAsync<T>(ApplicationDbContext db, string fileName) where T : class
    //{
    //    if (await db.Set<T>().AnyAsync()) return; // Idempotence automatique

    //    var path = Path.Combine(AppContext.BaseDirectory, "SeedData", fileName);
    //    var json = await File.ReadAllTextAsync(path);
    //    var data = JsonSerializer.Deserialize<List<T>>(json);

    //    if (data != null) await db.Set<T>().AddRangeAsync(data);
    //}



    //public static async Task SeedOrganizationAsync(ApplicationDbContext db)
    //{
    //    await db.Database.MigrateAsync();

    //    // --- IMPORT DES TYPES ---
    //    if (!await db.TypeEntites.AnyAsync())
    //    {
    //        var typeRows = await LoadCsvAsync<TypeEntite>("Types.csv");
    //        db.TypeEntites.AddRange(typeRows);
    //        await db.SaveChangesAsync();
    //    }

    //    // --- IMPORT DE L'ORGANIGRAMME (Avec tes 2 règles) ---
    //    if (!await db.Entites.AnyAsync())
    //    {
    //        var rows = await LoadCsvAsync<EntiteRow>("Organigramme.csv");
    //        var types = await db.TypeEntites.ToDictionaryAsync(t => t.Code);
    //        var cache = new Dictionary<string, Entite>();

    //        // 1. Unicité & Création mémoire
    //        foreach (var r in rows)
    //        {
    //            cache.Add(r.Code, new Entite { Code = r.Code, Libelle = r.Libelle, TypeEntite = types[r.TypeCode] });
    //        }

    //        // 2. Règle des Rangs (Hiérarchie)
    //        foreach (var r in rows.Where(r => !string.IsNullOrEmpty(r.ParentCode)))
    //        {
    //            var parent = cache[r.ParentCode!];
    //            var enfant = cache[r.Code];

    //            if (parent.TypeEntite.Rang >= enfant.TypeEntite.Rang)
    //                throw new Exception($"Erreur Hiérarchie : {parent.Code} ne peut pas être parent de {enfant.Code}");

    //            enfant.Rattachement = parent;
    //        }

    //        db.Entites.AddRange(cache.Values);
    //        await db.SaveChangesAsync();
    //    }
    //}

    //private static async Task<List<EntiteRow>> LoadCsvAsync()
    //{
    //    var path = Path.Combine(AppContext.BaseDirectory, "SeedData", "Organigramme.csv");

    //    var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" };

    //    using var reader = new StreamReader(path);
    //    using var csv = new CsvReader(reader, config);

    //    // On transforme le CSV en une liste d'objets simples (EntiteRow)
    //    var records = csv.GetRecords<EntiteRow>().ToList();
    //    return await Task.FromResult(records);
    //}

    //private static async Task<List<T>> LoadCsvAsync<T>(string fileName)
    //{
    //    var path = Path.Combine(AppContext.BaseDirectory, "SeedData", fileName);

    //    if (!File.Exists(path))
    //    {
    //        Console.WriteLine($"⚠️ Fichier {fileName} introuvable.");
    //        return new List<T>();
    //    }

    //    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
    //    {
    //        Delimiter = ";",
    //        PrepareHeaderForMatch = args => args.Header.ToLower(), // Ignore la casse (Libelle vs libelle)
    //        MissingFieldFound = null // Évite de planter s'il manque une colonne non critique
    //    };

    //    using var reader = new StreamReader(path);
    //    using var csv = new CsvReader(reader, config);

    //    var records = csv.GetRecords<T>().ToList();
    //    return await Task.FromResult(records);
    //}

    // Une petite classe interne pour mapper proprement le CSV
    public class EntiteRow
    {
        public required string Code { get; set; }
        public required string Libelle { get; set; }
        public required string TypeCode { get; set; }
        public  string? ParentCode { get; set; }
    }

}
