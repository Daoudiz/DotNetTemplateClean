
namespace DotNetTemplateClean.Application;

// Data payload attached to leaf nodes (a Fonction)
public record FonctionNodeData(int Id, string Code, string Designation, string? Type);

// Generic tree node shaped for PrimeNG p-treeselect
// PrimeNG p-treeselect node. Includes optional icons for expanded/collapsed states.
#pragma warning disable CA1002 // Do not use generic collection types in public APIs (but we need List for PrimeNG compatibility)
public record PrimeNgTreeNodeDto(
    string Key,
    string Label,
    FonctionNodeData? Data,
    List<PrimeNgTreeNodeDto> Children,
    string? ExpandedIcon = null,
    string? CollapsedIcon = null,
    bool Selectable = true
);



// Backwards compatible aliases (optional)
public record FonctionItemDto(int Id, string Code, string Designation, string? Type) : FonctionNodeData(Id, Code, Designation, Type);

public record FonctionDomaineDto(string Domaine, List<FonctionItemDto> Fonctions);
#pragma warning restore CA1002
