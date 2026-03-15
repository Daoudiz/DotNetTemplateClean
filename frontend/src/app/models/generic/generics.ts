// Modèle pour la réponse paginée
export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    isFullResult: boolean;
}