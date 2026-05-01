namespace DotNetTemplateClean.Infrastructure;

internal static class UserErrors
{
    public const string DuplicateMatricule = "Le matricule {0} est deja attribue a un autre agent.";
    public const string UserNotFound = "L'utilisateur demande est introuvable.";
    public const string RoleAssignmentFailed = "Impossible d'attribuer le role {0} a l'utilisateur.";
    public const string RoleNotFound = "Le role specifie n'existe pas dans le systeme.";
    public const string UserCreated = "Utilisateur cree avec succes";
    public const string UserDeleted = "Utilisateur supprime avec succes";
    public const string UserUpdated = "Utilisateur mis a jour avec succes";
    public const string PwdResetFailed = "La reinitialisation du mot de passe a echoue. Veuillez verifier le token et reessayer.";
    public const string PwdChangeFailed = "La modification du mot de passe a echoue. Veuillez verifier l'ancien mot de passe et reessayer.";
    public const string PwdChanged = "Mot de passe modifie avec succes.";
    public const string UserDesictivated = "Cet utilisateur est deja desactive.";
    public const string ErrorDesictivated = "Erreur lors de la desactivation de l'utilisateur.";
    public const string CannotDisableSelf = "Vous ne pouvez pas desactiver votre propre compte.";
}

internal static class Auth
{
    public const string InvalidCredentials = "Identifiants incorrects. Veuillez reessayer.";
    public const string AccountLocked = "Ce compte a ete verrouille.";
    public const string TokenExpired = "Votre session a expire, veuillez vous reconnecter.";
    public const string TwoFARequired = "Double authentification requise.";
    public const string PasswordChangeRequired = "Changement de mot de passe requis avant la connexion.";
}

public static class Role
{
    public const string RoleNameMissed = "Le nom du role est obligatoire.";
    public const string RoleAlreadyExists = "Ce role existe deja.";
    public const string RoleNotFound = "Role introuvable.";
}
