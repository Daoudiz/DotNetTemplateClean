import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApplicationUser, SearchViewModel, CreateUserViewModel, PagedResult } from '../../models/user/user-models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' }) 
export class UserService {   
   
    private readonly apiUrl = `${environment.apiUrl}`;

    constructor(private http: HttpClient) { }

   
    searchUsers(criteria: any): Observable<PagedResult<ApplicationUser>> {
        return this.http.post<PagedResult<ApplicationUser>>(`${this.apiUrl}/user/UserSearch`, criteria);
    }

    getDirections(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/entite/directions`);
    }

    getDivisions(directionId: number): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/entite/divisions/${directionId}`);
    }

    getServices(divisionId: number): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/entite/services/${divisionId}`);
    }

    // Méthode pour créer un nouvel utilisateur    
    createUser(userData: CreateUserViewModel): Observable<any> {
        return this.http.post(`${this.apiUrl}/user/CreateUser`, userData);
    }

    getRoles(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/roles`);
    }

    deleteUser(id: string): Observable<any> {
        return this.http.delete(`${this.apiUrl}/user/${id}`);
    }

    updateUser(id: string, userData: any): Observable<any> {
        return this.http.put(`${this.apiUrl}/user/${id}`, userData);
    }
}

