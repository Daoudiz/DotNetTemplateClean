namespace DotNetTemplateClean.Application;

public class OrganizationUnitSaveDto
{
    public int? Id { get; set; } // Null en création, obligatoire en update       
    public string Code { get; set; } = string.Empty;        
    public string Libelle { get; set; } = string.Empty;       
    public int TypeEntiteId { get; set; }
    public int? RattachementEntiteId { get; set; }
}

public class OrganizationUnitResponseDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;

    // Aplatissement du Type
    public int TypeEntiteId { get; set; }
    public string TypeEntiteLibelle { get; set; } = string.Empty;

    // Aplatissement du Parent
    public int? RattachementEntiteId { get; set; }
    public string? RattachementEntiteLibelle { get; set; }

}

public class OrganizationSearchFilters
{
    public string? SearchTerm { get; set; } // Cherche dans Code OU Libelle
    public int? TypeEntiteId { get; set; }

    public int? ParentId { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}


public class TypeEntiteDto
{
    public int Id { get; set; }
    public required string Libelle { get; set; }
}

public class TreeNodeDto
{
    public string Label { get; set; } = string.Empty;
    public int Data { get; set; } // L'ID de l'entité
    public string ExpandedIcon { get; set; } = "pi pi-folder-open";
    public string CollapsedIcon { get; set; } = "pi pi-folder";
    public ICollection<TreeNodeDto> Children { get; } = new List<TreeNodeDto>();
}
