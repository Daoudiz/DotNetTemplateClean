namespace DotNetTemplateClean.Application;

public static class AppErrorMessages
{
   

    internal static class Entite
    {
        public const string EntiteNotFound = "L'entité demandée est introuvable.";
        public const string InvalidEntiteType = "Le type d'entité spécifié n'est pas valide.";
        public const string EntiteHasChildren = "Impossible de supprimer cette entité car elle possède des entités enfants.";
        public const string LibelleNotUnique = "Le libellé '{0}' est déjà utilisé par une autre entité.";
        public const string CodeNotUnique = "Le code '{0}' est déjà utilisé par une autre entité.";
        public const string ParentRangInvalid = "L'entité de rattachement doit être de rang supérieur au rang de l'entité à créer.";
    }
    internal static class Global
    {
        public const string DatabaseError = "Une erreur est survenue lors de l'enregistrement en base de données.";
        public const string UnexpectedError = "Une erreur imprévue est survenue. Veuillez contacter le support.";
        public const string Unauthorized = "Vous n'avez pas les droits nécessaires pour effectuer cette action.";
    }

    internal static class Role
    {
        public const string RoleNameMissed = "Le nom du rôle est obligatoire.";
        public const string RoleAlreadyExists = "Ce rôle existe déjà.";
        public const string RoleNotFound = "Rôle introuvable.";
    }
}
