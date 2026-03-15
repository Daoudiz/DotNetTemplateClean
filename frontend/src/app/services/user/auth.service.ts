import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { LoginRequest, AuthResponse } from '../../models/user/auth.model';
import { jwtDecode } from 'jwt-decode';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
    
    // Le Signal qui surveille l'expiration
    sessionExpired = signal<boolean>(false);
    public loginError = signal<string | null>(null);
    
    private readonly apiUrl = `${environment.apiUrl}/Account`;

    // On crée un signal pour stocker le nom
    userName = signal<string | null>(this.getUserNameFromToken());

    // Un autre Signal utile pour savoir si on est connecté
    isAuthenticated = signal<boolean>(!!localStorage.getItem('token'));

    constructor(private http: HttpClient) { }

    // On précise que l'argument est de type LoginRequest
    // et que l'Observable retournera une AuthResponse
    login(credentials: LoginRequest): Observable<AuthResponse> {
        this.loginError.set(null);
        return this.http.post<AuthResponse>(`${this.apiUrl}/login`, credentials).pipe(
            tap(response => {
                if (response.token) {
                    localStorage.setItem('token', response.token);
                    localStorage.setItem('user', JSON.stringify(response));
                    this.userName.set(this.getUserNameFromToken());
                }
            })
        );
    }
    logout() {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        this.userName.set(null);
        this.isAuthenticated.set(false);
    }

    isLoggedIn(): boolean {
        return !!localStorage.getItem('token');
    }

    private getUserNameFromToken(): string | null {
        const token = localStorage.getItem('token');
        if (!token) return null;
        
        try {
            // On décode le token de façon typée
            const decoded: any = jwtDecode(token);

            // On cherche 'sub' car c'est ce que tu as mis dans ton AccountController
            return decoded.sub || null;
        } catch (error) {            
            return null;
        }
    }

    getUserRole(): string | null {
        const token = localStorage.getItem('token');
        if (!token) return null;

        try {
            // Décodage du payload du JWT
            const payload = JSON.parse(atob(token.split('.')[1]));
            // Note : Vérifie si ton backend envoie 'role' ou l'URL complète des claims Microsoft
            return payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
        } catch (e) {
            return null;
        }
    }

    // Appelle cette méthode dans ton composant de Login après un succès
    updateUserState() {
        this.userName.set(this.getUserNameFromToken());
        this.sessionExpired.set(false);
    }

    getUserId(): string | null {
        const token = localStorage.getItem('token'); // Ou le nom de ta clé

        if (!token) return null;

        try {
            const decoded: any = jwtDecode(token);

            // Dans ASP.NET Core Identity, l'ID est souvent dans ce claim spécifique :
            // "nameid" ou "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
            return decoded.nameid || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || null;
        } catch (error) {            
            return null;
        }
    }
}
