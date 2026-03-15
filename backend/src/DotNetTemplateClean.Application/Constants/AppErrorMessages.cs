namespace DotNetTemplateClean.Application;

public static class AppErrorMessages
{
    internal static class Auth
    {
        public const string InvalidCredentials = "Identifiants incorrects. Veuillez réessayer.";
        public const string AccountLocked = "Ce compte a été verrouillé.";
        public const string TokenExpired = "Votre session a expiré, veuillez vous reconnecter.";
        public const string TwoFARequired = "Double authentification requise.";
    }

    internal static class User
    {
        public const string DuplicateMatricule = "Le matricule {0} est déjà attribué à un autre agent.";
        public const string UserNotFound = "L'utilisateur demandé est introuvable.";
        public const string RoleAssignmentFailed = "Impossible d'attribuer le rôle {0} à l'utilisateur.";
        public const string RoleNotFound = "Le rôle spécifié n'existe pas dans le système.";
        public const string UserCreated = "Utilisateur crée avec succé";
        public const string UserDeleted = "Utilisateur supprimé avec succé";
        public const string UserUpdated = "Utilisateur mis à jour avec succé";
        public const string PwdResetFailed = "La réinitialisation du mot de passe a échoué. Veuillez vérifier le token et réessayer.";
        public const string PwdChangeFailed = "La modification du mot de passe a échoué. Veuillez vérifier l'ancien mot de passe et réessayer.";
        public const string PwdChanged = "Mot de passe modifié avec succès.";
        public const string UserDesictivated = "Cet utilisateur est déjà désactivé.";
        public const string ErrorDesictivated = "Erreur lors de la désactivation de l'utilisateur.";
        public const string CannotDisableSelf = "Vous ne pouvez pas désactiver votre propre compte.";


    }

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
