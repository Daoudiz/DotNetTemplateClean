export interface EntiteSaveDto {
    id?: number;              // Optionnel pour la création
    libelle: string;
    code: string;
    typeEntiteId: number;
    rattachementEntiteId?: number;  // Nullable si c'est une entité racine
}

export interface EntiteSearchRequest {
    searchTerm?: string;          // Pour ta recherche globale Code/Libelle
    typeEntiteId?: number;
    parentId?: number;
    pageNumber: number;       // Pour la pagination
    pageSize: number;
}

export interface EntiteItemSearchResponse {
    id: number;
    libelle: string;
    code: string;
    typeEntiteLibelle: string; // On affiche le nom du type, pas juste l'ID
    rattachementEntiteLibelle?: string;    // Nom de l'entité parente    
}

export interface TypeEntite {
    id: number;
    libelle: string;
    
}

export interface OrganizationTreeNode {
    label: string;
    data: number; // L'ID de l'entité
    key?: string; // Identifiant unique utilisé par PrimeNG pour la sélection
    expandedIcon?: string;
    collapsedIcon?: string;
    children?: OrganizationTreeNode[];
}