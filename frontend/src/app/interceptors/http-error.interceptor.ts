import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject, Injector } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ErrorHandlerService } from '../services/errors/error-handler.service';

export const httpErrorInterceptor: HttpInterceptorFn = (req, next) => {
    // 1. On injecte l'Injecteur global (autorisé ici car on est dans le contexte d'initialisation)
    const injector = inject(Injector);

    return next(req).pipe(
        catchError((error: HttpErrorResponse) => {
            // 2. On récupère le service manuellement via l'injecteur au moment de l'erreur
            // Cela évite l'erreur NG0203 (car injector est déjà capturé)
            // Et cela évite NG0200 (car on ne demande ErrorHandlerService qu'à la demande)
            const errorHandler = injector.get(ErrorHandlerService);

            errorHandler.handle(error,req); 

            return throwError(() => error);
        })
    );
};