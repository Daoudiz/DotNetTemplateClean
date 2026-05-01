import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import {
  AdminResetPasswordRequest,
  AdminResetPasswordResponse,
  ApplicationUser,
  CreateUserViewModel,
  PagedResult,
  RoleOption,
  UpdateUserViewModel,
  UserSearchCriteria,
  UserSearchResultDto
} from '../../models/user/user-models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly apiUrl = `${environment.apiUrl}`;

  constructor(private http: HttpClient) {}

  searchUsers(criteria: UserSearchCriteria): Observable<PagedResult<ApplicationUser>> {
    return this.http
      .post<PagedResult<UserSearchResultDto>>(`${this.apiUrl}/user/UserSearch`, criteria)
      .pipe(
        map((response) => ({
          ...response,
          items: response.items.map((item) => ({
            id: item.id,
            userName: item.userName,
            email: item.email,
            firstName: item.prenom,
            lastName: item.nom,
            roles: item.roles,
            roleId: item.roleId,
            isLocked: item.isLocked
          }))
        }))
      );
  }

  createUser(userData: CreateUserViewModel): Observable<unknown> {
    return this.http.post(`${this.apiUrl}/user/CreateUser`, userData);
  }

  getRoles(): Observable<RoleOption[]> {
    return this.http.get<RoleOption[]>(`${this.apiUrl}/roles`);
  }

  deleteUser(id: string): Observable<unknown> {
    return this.http.delete(`${this.apiUrl}/user/${id}`);
  }

  updateUser(id: string, userData: UpdateUserViewModel): Observable<unknown> {
    return this.http.put(`${this.apiUrl}/user/${id}`, userData);
  }

  resetUserPassword(id: string, payload: AdminResetPasswordRequest): Observable<AdminResetPasswordResponse> {
    return this.http
      .post<{ data: AdminResetPasswordResponse }>(`${this.apiUrl}/user/${id}/reset-password`, payload)
      .pipe(map((response) => response.data));
  }
}
