export interface Entite {
    id: number;
    code: string;
}
export interface ApplicationUser {
    id: string;
    firstName: string;
    lastName: string;
    matricule: number;
    dateRecrutement?: string;
    entiteId: number;
    entite?: any; // Optionnel avec '?'
    lockoutEnabled: boolean;
    lockoutEnd?: string; // Optionnel avec '?'
}

export interface SearchViewModel {
    matricule?: number;
    nom?: string;
    prenom?: string;
    dateRecrutementDebut?: string;
    dateRecrutementFin?: string;
    direction?: number;
    division?: number;
    service?: number;
}

export interface CreateUserViewModel {
    matricule: number;
    firstName: string;
    lastName: string;
    dateRecrutement: string; // ISO string pour les dates
    direction: number;
    division: number;
    service: number;
    userRole: string;
    roleId : string;
    email: string;
    userName: string;
    password: string;
    confirmPassword: string;
    twoFactorEnabled: boolean;
}

export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    isFullResult: boolean;
}

export interface UserSearchCriteria {
    matricule?: number | null;
    nom?: string | null;
    prenom?: string | null;
    dateRecrutementDebut?: string | null;
    dateRecrutementFin?: string | null;
    directionId?: number | null;
    divisionId?: number | null;
    serviceId?: number | null;
    pageNumber: number;
    pageSize: number;
}

