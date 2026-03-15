import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/user/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);   

    // Vérifie si le token existe dans le localStorage
    if (authService.isLoggedIn()) {
        return true;
    }

    // Sinon, redirection vers login avec l'URL de retour en paramètre (optionnel)
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};