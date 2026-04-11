import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { WebUtilsService } from '../helpers/web-utils.service';
import {
    CreatePersonnelRequest,
    GetPersonnelsWithFiltersQuery,
    NatureFonction,
    PersonnelDetailsDto,
    PersonnelListDto,
    UpdatePersonnelRequest,
    StatutPersonnel,
    PrimeNgTreeNode
} from '../../models/organisation/personnel.model';
import { PaginatedList } from '../../models/generic/generics';

@Injectable({ providedIn: 'root' })
export class PersonnelService {
    private readonly http = inject(HttpClient);
    private readonly webUtils = inject(WebUtilsService);
    private readonly apiUrl = `${environment.apiUrl}/personnel`;

    getPersonnel(filters: GetPersonnelsWithFiltersQuery): Observable<PaginatedList<PersonnelListDto>> {
        const params = this.webUtils.toHttpParams(filters);

        return this.http.get<PaginatedList<PersonnelListDto>>(`${this.apiUrl}`, { params });
    }

    getPersonnelById(id: number): Observable<PersonnelDetailsDto> {
        return this.http.get<PersonnelDetailsDto>(`${this.apiUrl}/${id}`);
    }

    createPersonnel(data: CreatePersonnelRequest): Observable<number> {
        return this.http.post<number>(this.apiUrl, data);
    }

    updatePersonnel(id: number, data: UpdatePersonnelRequest): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${id}`, { ...data, id });
    }

    deletePersonnel(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }

    /**
     * Retrieves the list of personnel statuses from the backend.
     */
    getStatutPersonnel(): Observable<StatutPersonnel[]> {
        return this.http.get<StatutPersonnel[]>(`${this.apiUrl}/statutpersonnel`);
    }

    /**
     * Fetches the Fonctions tree structure compatible with PrimeNG p-treeselect
     * @returns Observable of array of PrimeNgTreeNode representing the hierarchy
     */
    getFonctionsTree(): Observable<PrimeNgTreeNode[]> {
        return this.http.get<PrimeNgTreeNode[]>(`${environment.apiUrl}/fonctions/tree`);
    }

    getNatureFonctions(): Observable<NatureFonction[]> {
        return this.http
            .get<NatureFonction[]>(`${environment.apiUrl}/fonctions/naturefonction`)
            .pipe(catchError((error) => throwError(() => error)));
    }
}
