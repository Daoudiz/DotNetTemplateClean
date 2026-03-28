export interface AffectationDto {
    id: number;
    dateDebutAffectation: string;
    dateFinAffectation?: string | null;
    fonctionLibelle?: string | null;
    entiteLibelle?: string | null;
}

export interface PersonnelListDto {
    matricule: string;
    nom: string;
    prenom: string;
    dateRecrutement?: string | null;
    dateNaissance?: string | null;
    statut?: string | null;
    grade?: string | null;
    affectations: AffectationDto[];
}

export interface GetPersonnelsWithFiltersQuery {
    searchTerm?: string;
    entiteId?: number | null;
    pageNumber: number;
    pageSize: number;
}

