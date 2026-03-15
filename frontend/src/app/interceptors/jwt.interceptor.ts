import {
    HttpErrorResponse,
    HttpInterceptorFn,
    HttpResponse,
    HttpEvent
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError, map, Observable } from 'rxjs';
import { AuthService } from '../services/user/auth.service';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
    const token = localStorage.getItem('token');
    const router = inject(Router);
    const authService = inject(AuthService);

    //Ajout du token JWT si présent
    if (token) {
        req = req.clone({
            setHeaders: { Authorization: `Bearer ${token}` }
        });
    }

    return next(req).pipe(
        //UNWRAP DATA : Extraction automatique du contenu de "data"
        map((event: HttpEvent<any>) => {
            if (event instanceof HttpResponse) {
                const body = event.body;

                // Vérifie si le format est { data: ... }
                if (body && typeof body === 'object' && 'data' in body) {
                    return event.clone({
                        body: body.data
                    });
                }
            }
            return event;
        }),

        //GESTION DES ERREURS GLOBALES
        catchError((error: HttpErrorResponse) => {
            if (error.status === 401) {
                // Ne pas rediriger si c'est une erreur sur la tentative de login elle-même
                const isLoginRequest = req.url.toLowerCase().includes('login');

                if (!isLoginRequest) {
                    authService.sessionExpired.set(true);
                    localStorage.removeItem('token');
                    router.navigate(['/login']);
                }
            }            

            return throwError(() => error);
        })
    );
};