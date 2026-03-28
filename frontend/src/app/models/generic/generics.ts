// Modèle pour la réponse paginée
export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    isFullResult: boolean;
}

export interface PaginatedList<T> {
    items: T[];
    pageNumber: number;
    totalPages: number;
    totalCount: number;
    isFullResult: boolean;
}
