export interface AffectationDto {
    id: number;
    dateDebutAffectation: string;
    dateFinAffectation?: string | null;
    fonctionLibelle?: string | null;
    entiteLibelle?: string | null;
}

export interface PersonnelListDto {
    id: number;
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

export interface CreateAffectationRequest {
    entiteId: number;
    fonctionId: number;
    dateDebutAffectation: string;
    dateDebut: string;
    nature: string;
}

export interface UpdateAffectationRequest {
    id: number;
    entiteId: number;
    fonctionId: number;
    dateDebutAffectation: string;
    dateDebut: string;
    nature: string;
}

export interface CreatePersonnelRequest {
    matricule: string;
    nom: string;
    prenom: string;
    dateRecrutement?: string | null;
    dateNaissance?: string | null;
    email: string;
    entiteId: number;
    statut?: string | null;
    grade?: string | null;
    createUser: boolean;
    userRole?: string | null;
    affectations: CreateAffectationRequest[];
}

export interface UpdatePersonnelRequest {
    matricule: string;
    nom: string;
    prenom: string;
    dateRecrutement?: string | null;
    dateNaissance?: string | null;
    statut?: string | null;
    grade?: string | null;
    affectations: UpdateAffectationRequest[];
}

export interface PersonnelEditAffectationDto {
    id: number;
    entiteId: number;
    fonctionId: number;
    dateDebutAffectation?: string;
    dateDebut?: string;
    nature: string;
}

export interface PersonnelDetailsDto {
    id: number;
    matricule: string;
    nom: string;
    prenom: string;
    dateRecrutement?: string | null;
    dateNaissance?: string | null;
    email: string;
    entiteId: number;
    statut?: string | null;
    grade?: string | null;
    affectations: PersonnelEditAffectationDto[];
}

export interface StatutPersonnel {
    value: string;
    displayName: string;
}

// Fonction Tree Models
export interface FonctionNodeData {
    id: number;
    code: string;
    designation: string;
    type?: string | null;
}

export interface PrimeNgTreeNode {
    key: string;
    label: string;
    data?: FonctionNodeData;
    children: PrimeNgTreeNode[];
    expandedIcon?: string;
    collapsedIcon?: string;
    selectable?: boolean;
}
