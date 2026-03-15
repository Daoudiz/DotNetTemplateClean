import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/user/auth.service';


export const roleGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    const userRole = authService.getUserRole();
    const allowedRoles = route.data['roles'] as string[];

    if (authService.isLoggedIn() && allowedRoles.includes(userRole!)) {
        return true;
    }

    // Si Manager essaie d'accéder à un truc d'Admin
    alert("Accès refusé : Droits insuffisants.");
    return router.createUrlTree(['/dashboard']);
};