

namespace DotNetTemplateClean.Application;

public interface IEntiteService
{
    Task<IEnumerable<Entite>> GetAllEntities();
    Task<ServiceResult<Entite>> GetEntiteById(int id);
    Task<ServiceResult<Entite>> GetDirectionById(int id);
    Task<IEnumerable<Entite>> GetAllDirections();
    Task<ServiceResult<Entite>> GetDivisionById(int id);
    Task<IEnumerable<Entite>> GetAllDivisions();
    Task<ServiceResult<Entite>> GetServiceById(int id);
    Task<IEnumerable<Entite>> GetAllServices();
    Task<IEnumerable<Entite>> GetDivisionsByDirection(int directionId);
    Task<IEnumerable<Entite>> GetServicesByRattachement(int rattachementId);
    Task<PagedResult<OrganizationUnitResponseDto>> SearchUnitsAsync(OrganizationSearchFilters filters);
    Task<ServiceResult<string>> CreateEntiteAsync(OrganizationUnitSaveDto dto, CancellationToken cancellationToken);
    Task<ServiceResult<string>> UpdateEntiteAsync(OrganizationUnitSaveDto dto, CancellationToken cancellationToken);
    Task<ServiceResult<string>> DeleteEntiteAsync(int id, CancellationToken cancellationToken);
    Task<IEnumerable<TypeEntiteDto>> GetAllTypeEntite();
    Task<List<TreeNodeDto>> GetOrganizationTreeAsync();

}
