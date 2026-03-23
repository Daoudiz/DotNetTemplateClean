



namespace DotNetTemplateClean.Application;

public class EntiteService(
                            IApplicationDbContext context,
                            IOptions<SearchSettings> searchOptions,
                            IValidator<OrganizationUnitSaveDto> validator,
                              IMapper mapper ) : IEntiteService
{
    public async Task<IEnumerable<Entite>> GetAllEntities()  
        => await context.Entites.AsNoTracking().ToListAsync().ConfigureAwait(false) ;
    

    public async Task<ServiceResult<Entite>> GetEntiteById(int id)
    {
        var entite = await context.Entites.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id).ConfigureAwait(false);

        if (entite is null)
            return ServiceResult.Failure<Entite>(AppErrorMessages.Entite.EntiteNotFound, 404);

        return ServiceResult.Success(entite);
    }

    public async Task<ServiceResult<Entite>> GetDirectionById(int id)
    {
        var entite = await GetEntityByTypeAndIdAsync(EntityTypes.Direction, id).ConfigureAwait(false);

        if (entite is null)
            return ServiceResult.Failure<Entite>(AppErrorMessages.Entite.EntiteNotFound, 404);

        return ServiceResult.Success(entite);
    }

    public async Task<IEnumerable<Entite>> GetAllDirections()    
        => await GetEntitiesByType(EntityTypes.Direction)
                          .Select(e => new Entite
                          {
                              Id = e.Id,
                              Code = e.Code
                          })
                          .ToListAsync().ConfigureAwait(false);
    

    public async Task<ServiceResult<Entite>> GetDivisionById(int id)
    {
        var entite = await GetEntityByTypeAndIdAsync(EntityTypes.Division, id).ConfigureAwait(false);

        if (entite is null)
            return ServiceResult.Failure<Entite>(AppErrorMessages.Entite.EntiteNotFound, 404);

        return ServiceResult.Success(entite);
    }

    public async Task<IEnumerable<Entite>> GetAllDivisions()
        => await GetEntitiesByType(EntityTypes.Division).ToListAsync().ConfigureAwait(false);

    public async Task<ServiceResult<Entite>> GetServiceById(int id)
    {
        var entite = await GetEntityByTypeAndIdAsync(EntityTypes.Service, id).ConfigureAwait(false);

        if (entite is null)
            return ServiceResult.Failure<Entite>(AppErrorMessages.Entite.EntiteNotFound, 404);

        return ServiceResult.Success(entite);
    }

    public async Task<IEnumerable<Entite>> GetAllServices()
    => await GetEntitiesByType(EntityTypes.Service).ToListAsync().ConfigureAwait(false) ; 


    public async Task<IEnumerable<Entite>> GetDivisionsByDirection(int directionId)
        => await GetEntitiesByType(EntityTypes.Division)
            .Where(e => e.RattachementEntiteId == directionId)
            .Select(e => new Entite { Id = e.Id, Code = e.Code })
            .ToListAsync().ConfigureAwait(false);

    public async Task<IEnumerable<Entite>> GetServicesByRattachement(int rattachementId)
    =>await GetEntitiesByType(EntityTypes.Service)
            .Where(e => e.RattachementEntiteId == rattachementId)
            .Select(e => new Entite { Id = e.Id, Code = e.Code })
            .ToListAsync().ConfigureAwait(false);
    

    public async Task<PagedResult<OrganizationUnitResponseDto>> SearchUnitsAsync(OrganizationSearchFilters filters)
    {

        ArgumentNullException.ThrowIfNull(filters);

        //Préparation de la requête de base
        var query = context.Entites.AsNoTracking();

        //Application des filtres de recherche
        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            var search = filters.SearchTerm.Trim();

            query = query.Where(x =>
                EF.Functions.Like(x.Code!, $"%{search}%") ||
                EF.Functions.Like(x.Libelle!, $"%{search}%"));

            //var search = filters.SearchTerm.Trim().ToUpperInvariant();
            //query = query.Where(x => (x.Code != null && x.Code.Contains(search, StringComparison.InvariantCultureIgnoreCase)) ||
            //                         (x.Libelle != null && x.Libelle.Contains(search, StringComparison.InvariantCultureIgnoreCase)));
        }

        if (filters.TypeEntiteId.HasValue)
        {
            query = query.Where(x => x.TypeEntiteId == filters.TypeEntiteId.Value);
        }

        //Filtre Récursif par Parent (La touche de Génie)
        if (filters.ParentId.HasValue)
        {
            // On récupère tous les IDs de la branche (le parent + tous ses descendants)
            var allChildrenIds = await GetFlattenedChildEntityIds(filters.ParentId.Value).ConfigureAwait(false);
            query = query.Where(x => allChildrenIds.Contains(x.Id));
        }

        //Calcul du total des éléments filtrés
        var totalCount = await query.CountAsync().ConfigureAwait(false);

        //Préparation de la projection (sans exécution immédiate)
        var projection = query.OrderBy(x => x.Libelle)
                              .ProjectTo<OrganizationUnitResponseDto>(mapper.ConfigurationProvider);

        // Logique hybride : Chargement complet ou Paginé
        // On utilise la valeur du seuil (Threshold) depuis la configuration
        if (totalCount <= searchOptions.Value.ThresholdForFullLoad)
        {
            // On récupère tout d'un coup
            var allItems = await projection.ToListAsync().ConfigureAwait(false);
            return new PagedResult<OrganizationUnitResponseDto>(allItems, totalCount, isFull: true);
        }
        else
        {
            // On applique la pagination SQL standard
            var pagedItems = await projection
                .Skip((filters.PageNumber - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync().ConfigureAwait(false);

            return new PagedResult<OrganizationUnitResponseDto>(pagedItems, totalCount, isFull: false);
        }
    }

    public async Task<ServiceResult<string>> CreateEntiteAsync(OrganizationUnitSaveDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);

        //Validation centralisée
        var validationResult = await validator.ValidateAsync(dto, cancellationToken);

        if (!validationResult.IsValid)
        {
            return ServiceResult.Failure<string>(
                string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)),
                400);
        }
       
        if (!await IsParentRangGreaterAsync(dto.TypeEntiteId, dto.RattachementEntiteId!.Value).ConfigureAwait(false))
        {
            return ServiceResult.Failure<string>(AppErrorMessages.Entite.ParentRangInvalid, 400);
        }

        var entite = mapper.Map<Entite>(dto);

        await context.Entites.AddAsync(entite, cancellationToken);

        await  context.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success(entite.Id.ToString(CultureInfo.InvariantCulture));
    }

    public async Task<ServiceResult<string>> UpdateEntiteAsync(OrganizationUnitSaveDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);

        // Id is required for update
        if (!dto.Id.HasValue)
        {
            return ServiceResult.Failure<string>(AppErrorMessages.Entite.EntiteNotFound, 404 );
        }

        // Retrieve existing entity
        var existing = await context.Entites.FirstOrDefaultAsync(e => e.Id == dto.Id.Value, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.Failure<string>(AppErrorMessages.Entite.EntiteNotFound, 404);
        }

        // If libelle changed, ensure uniqueness
        if (!string.Equals(existing.Libelle, dto.Libelle, StringComparison.OrdinalIgnoreCase))
        {
            if (!await IsLibelleUniqueAsync(dto.Libelle, cancellationToken))
            {
                return ServiceResult.Failure<string>(string.Format(CultureInfo.InvariantCulture,
                                                                   AppErrorMessages.Entite.LibelleNotUnique, dto.Libelle),
                                                                            400);
            }
        }

        // If code changed, ensure uniqueness
        if (!string.Equals(existing.Code, dto.Code, StringComparison.OrdinalIgnoreCase))
        {
            if (!await IsCodeUniqueAsync(dto.Code, cancellationToken))
            {
                return ServiceResult.Failure<string>(string.Format(CultureInfo.InvariantCulture, AppErrorMessages.Entite.CodeNotUnique, dto.Code), 400);
            }
        }

        //Vérifier que la parent possède un rang hiearachiquement strictement supérieur au rang de l'entité à créer
        if (!await IsParentRangGreaterAsync(dto.TypeEntiteId, dto.RattachementEntiteId!.Value))
        {
            return ServiceResult.Failure<string>(AppErrorMessages.Entite.ParentRangInvalid, 400);
        }

        // Map incoming values onto the tracked entity
        mapper.Map(dto, existing);    
      

        await context.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success(existing.Id.ToString(CultureInfo.InvariantCulture));
    }

    public async Task<ServiceResult<string>> DeleteEntiteAsync(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return ServiceResult.Failure<string>("Identificant invalide", 400);
        }

        var existing = await context.Entites.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.Failure<string>(AppErrorMessages.Entite.EntiteNotFound, 404);
        }

        // Prevent delete when there are children (descendants)
        var descendantIds = await GetFlattenedChildEntityIds(id);
        if (descendantIds != null && descendantIds.Count > 1)
        {
            return ServiceResult.Failure<string>(AppErrorMessages.Entite.EntiteHasChildren, 409);
        }        

        // Perform delete (soft or hard depending on repository implementation)
        context.Entites.Remove(existing);

        await context.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success(id.ToString(CultureInfo.InvariantCulture));
    }

    public async Task<IEnumerable<TypeEntiteDto>> GetAllTypeEntite()
     => await context.TypeEntites
                .AsNoTracking()
                .OrderBy(t => t.Rang)
                .Select(e => new TypeEntiteDto
                {
                    Id = e.Id,
                    Libelle = e.Libelle ?? "Sans libellé" // Gestion du null pour éviter les surprises au Front
                })
                .ToListAsync();
    

    public async Task<List<TreeNodeDto>> GetOrganizationTreeAsync()
    {
        //Récupération de toutes les entités
        var allEntities = await context.Entites
            .AsNoTracking()
            .Select(e => new { e.Id, e.Libelle, e.RattachementEntiteId })
            .ToListAsync();

        //Création d'un dictionnaire pour un accès rapide (O(1))
        var nodesMap = allEntities.ToDictionary(
            e => e.Id,
            e => new TreeNodeDto
            {
                Label = e.Libelle,
                Data = e.Id
            }
        );

        var rootNodes = new List<TreeNodeDto>();

        //Construction de la hiérarchie
        foreach (var entity in allEntities)
        {
            var currentNode = nodesMap[entity.Id];

            if (entity.RattachementEntiteId == null)
            {
                // C'est une racine (ex: Direction Générale)
                rootNodes.Add(currentNode);
            }
            else if (nodesMap.TryGetValue(entity.RattachementEntiteId.Value, out var parentNode))
            {
                // On l'ajoute comme enfant de son parent
                parentNode.Children.Add(currentNode);
            }
        }

        return rootNodes;
    }

    #region Private Helpers
    private IQueryable<Entite> GetEntitiesByType(string type)
    => context.Entites
        .AsNoTracking()
        .Where(e => EF.Functions.Like(e.TypeEntite.Libelle, type));

    /// <summary>
    /// Récupère une entité par son type et son identifiant.
    /// </summary>
    private async Task<Entite?> GetEntityByTypeAndIdAsync(string type, int id)
        => await GetEntitiesByType(type)
            .FirstOrDefaultAsync(e => e.Id == id).ConfigureAwait(false) ;


    public async Task<List<int>> GetFlattenedChildEntityIds(int parentId)
    {
        // 1. On récupère toute la structure en UNE SEULE FOIS.
        // .AsNoTracking() est crucial ici pour la performance en lecture seule.
        var allEntities = await context.Entites
            .AsNoTracking()
            .Select(e => new { e.Id, e.RattachementEntiteId })
            .ToListAsync().ConfigureAwait(false);

        // 2. Initialisation du résultat et de la file d'attente (BFS)
        // On utilise un HashSet pour garantir l'unicité et une recherche en O(1)
        var resultIds = new HashSet<int> { parentId };
        var queue = new Queue<int>();
        queue.Enqueue(parentId);

        // 3. Parcours de l'arbre en mémoire
        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();

            // On cherche les enfants directs du niveau actuel
            var children = allEntities
                .Where(e => e.RattachementEntiteId == currentId)
                .Select(e => e.Id);

            foreach (var childId in children)
            {
                // .Add() renvoie false si l'ID existe déjà (protection contre les boucles infinies)
                if (resultIds.Add(childId))
                {
                    queue.Enqueue(childId);
                }
            }
        }

        return [.. resultIds];
    }

    public async Task<bool> IsLibelleUniqueAsync(string libelle, CancellationToken concellationTotekn)
    => !await context.Entites.AnyAsync(e => EF.Functions.Like(e.Libelle, libelle), concellationTotekn).ConfigureAwait(false);
    

    public async Task<bool> IsCodeUniqueAsync(string code, CancellationToken concellationTotekn)
    => !await context.Entites.AnyAsync(e => e.Code != null && EF.Functions.Like(e.Code, code), concellationTotekn).ConfigureAwait(false);

    public async Task<bool> IsParentRangGreaterAsync(int childTypeEntiteId, int parentId)
    {
        if (childTypeEntiteId <= 0 || parentId <= 0) return false;

        // Récupérer le rang du parent via l'entité parent
        var parentRankEntry = await context.Entites
            .AsNoTracking()
            .Where(e => e.Id == parentId)
            .Select(e => new { e.TypeEntite.Rang })
            .FirstOrDefaultAsync().ConfigureAwait(false);

        if (parentRankEntry is null || !parentRankEntry.Rang.HasValue) return false;

        // Récupérer le rang du TypeEntite enfant (passé en paramètre)
        var childTypeRank = await context.TypeEntites
            .AsNoTracking()
            .Where(t => t.Id == childTypeEntiteId)
            .Select(t => t.Rang)
            .FirstOrDefaultAsync().ConfigureAwait(false);

        if (!childTypeRank.HasValue) return false;

        return parentRankEntry.Rang.Value < childTypeRank.Value;
    }   
    #endregion
}
