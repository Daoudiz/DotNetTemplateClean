namespace DotNetTemplateClean.Application;

public interface IPersonnelMatriculeUniquenessService
{
    Task<bool> IsMatriculeUniqueAsync(string matricule, int? excludedPersonnelId, CancellationToken cancellationToken);
}

public class PersonnelMatriculeUniquenessService(IApplicationDbContext context) : IPersonnelMatriculeUniquenessService
{
    public async Task<bool> IsMatriculeUniqueAsync(string matricule, int? excludedPersonnelId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(matricule))
        {
            return true;
        }

        var query = context.Personnels
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Matricule == matricule);

        if (excludedPersonnelId.HasValue)
        {
            query = query.Where(p => p.Id != excludedPersonnelId.Value);
        }

        return !await query.AnyAsync(cancellationToken);
    }
}
