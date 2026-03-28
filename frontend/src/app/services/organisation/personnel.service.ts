import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { WebUtilsService } from '../helpers/web-utils.service';
import { GetPersonnelsWithFiltersQuery, PersonnelListDto } from '../../models/organisation/personnel.model';
import { PaginatedList } from '../../models/generic/generics';

@Injectable({ providedIn: 'root' })
export class PersonnelService {
    private readonly http = inject(HttpClient);
    private readonly webUtils = inject(WebUtilsService);
    private readonly apiUrl = `${environment.apiUrl}/personnel`;

    getPersonnel(filters: GetPersonnelsWithFiltersQuery): Observable<PaginatedList<PersonnelListDto>> {
        const params = this.webUtils.toHttpParams(filters);

        return this.http.get<PaginatedList<PersonnelListDto>>(`${this.apiUrl}/personnel/GetPersonnels`, { params });
    }
}
