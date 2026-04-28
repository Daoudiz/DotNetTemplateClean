namespace DotNetTemplateClean.Application;

public interface IEntiteRulesService
{
    Task<ServiceResult<string>?> ValidateCreateUniquenessAsync(string code, string libelle, CancellationToken cancellationToken);
    Task<ServiceResult<string>?> ValidateUpdateUniquenessAsync(Entite existing, OrganizationUnitSaveDto dto, CancellationToken cancellationToken);
    Task<ServiceResult<string>?> ValidateParentRankAsync(int childTypeEntiteId, int? parentId, CancellationToken cancellationToken);
}

public class EntiteRulesService(IApplicationDbContext context) : IEntiteRulesService
{
    public async Task<ServiceResult<string>?> ValidateCreateUniquenessAsync(string code, string libelle, CancellationToken cancellationToken)
    {
        if (!await IsCodeUniqueAsync(code, cancellationToken).ConfigureAwait(false))
        {
            return ServiceResult.Failure<string>("Le code existe déjà.", 400);
        }

        if (!await IsLibelleUniqueAsync(libelle, cancellationToken).ConfigureAwait(false))
        {
            return ServiceResult.Failure<string>("Le libellé existe déjà.", 400);
        }

        return null;
    }

    public async Task<ServiceResult<string>?> ValidateUpdateUniquenessAsync(Entite existing, OrganizationUnitSaveDto dto, CancellationToken cancellationToken)
    {
        if (!string.Equals(existing.Libelle, dto.Libelle, StringComparison.OrdinalIgnoreCase))
        {
            if (!await IsLibelleUniqueAsync(dto.Libelle, cancellationToken).ConfigureAwait(false))
            {
                return ServiceResult.Failure<string>(
                    string.Format(CultureInfo.InvariantCulture, AppErrorMessages.Entite.LibelleNotUnique, dto.Libelle),
                    400);
            }
        }

        if (!string.Equals(existing.Code, dto.Code, StringComparison.OrdinalIgnoreCase))
        {
            if (!await IsCodeUniqueAsync(dto.Code, cancellationToken).ConfigureAwait(false))
            {
                return ServiceResult.Failure<string>(
                    string.Format(CultureInfo.InvariantCulture, AppErrorMessages.Entite.CodeNotUnique, dto.Code),
                    400);
            }
        }

        return null;
    }

    public async Task<ServiceResult<string>?> ValidateParentRankAsync(int childTypeEntiteId, int? parentId, CancellationToken cancellationToken)
    {
        if (!parentId.HasValue)
        {
            return null;
        }

        if (!await IsParentRangGreaterAsync(childTypeEntiteId, parentId.Value, cancellationToken).ConfigureAwait(false))
        {
            return ServiceResult.Failure<string>(AppErrorMessages.Entite.ParentRangInvalid, 400);
        }

        return null;
    }

    private async Task<bool> IsLibelleUniqueAsync(string libelle, CancellationToken cancellationToken)
        => !await context.Entites.AnyAsync(e => EF.Functions.Like(e.Libelle, libelle), cancellationToken).ConfigureAwait(false);

    private async Task<bool> IsCodeUniqueAsync(string code, CancellationToken cancellationToken)
        => !await context.Entites.AnyAsync(e => e.Code != null && EF.Functions.Like(e.Code, code), cancellationToken).ConfigureAwait(false);

    private async Task<bool> IsParentRangGreaterAsync(int childTypeEntiteId, int parentId, CancellationToken cancellationToken)
    {
        if (childTypeEntiteId <= 0 || parentId <= 0)
        {
            return false;
        }

        var parentRankEntry = await context.Entites
            .AsNoTracking()
            .Where(e => e.Id == parentId)
            .Select(e => new { e.TypeEntite.Rang })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (parentRankEntry is null || !parentRankEntry.Rang.HasValue)
        {
            return false;
        }

        var childTypeRank = await context.TypeEntites
            .AsNoTracking()
            .Where(t => t.Id == childTypeEntiteId)
            .Select(t => t.Rang)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!childTypeRank.HasValue)
        {
            return false;
        }

        return parentRankEntry.Rang.Value < childTypeRank.Value;
    }
}
