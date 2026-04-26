namespace DotNetTemplateClean.Domain;

public class ApplicationUser : IdentityUser, IAuditableEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // Audit implementation (no multiple inheritance possible with IdentityUser)
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
