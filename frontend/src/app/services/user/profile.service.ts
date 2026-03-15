// Service to fetch user profile data from the backend API
import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserProfile } from '../../models/user/auth.model';
import { environment } from '../../../environments/environment';
import { ChangePasswordDto } from '../../models/user/auth.model';

@Injectable({ providedIn: 'root' })
export class ProfileService {
    private http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}`;
   

    getProfile(): Observable<UserProfile> {
        return this.http.get<UserProfile>(`${this.apiUrl}/account/profile`);
    }

    changePassword(model: ChangePasswordDto): Observable<any> {
        // L'intercepteur JWT ajoutera automatiquement le token
        return this.http.post(`${this.apiUrl}/user/change-password`, model);
    }
}