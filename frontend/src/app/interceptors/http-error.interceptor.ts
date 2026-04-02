import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';

import { ErrorHandlerService } from '../services/errors/error-handler.service';

export const httpErrorInterceptor: HttpInterceptorFn = (req, next) => {
    const errorHandler = inject(ErrorHandlerService);

    return next(req).pipe(
        catchError((error: HttpErrorResponse) => {
            errorHandler.handle(error, req);
            return throwError(() => error);
        })
    );
};
