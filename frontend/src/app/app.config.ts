import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { providePrimeNG } from 'primeng/config';
import { ConfirmationService } from 'primeng/api';
import Aura from '@primeng/themes/aura'; // Tu peux choisir Aura, Lara ou Nora
import { provideAnimations } from '@angular/platform-browser/animations'; // Important !
import { provideToastr } from 'ngx-toastr';
import { httpErrorInterceptor } from './interceptors/http-error.interceptor';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import {
  provideRouter,
  withEnabledBlockingInitialNavigation,
  withHashLocation,
  withInMemoryScrolling,
  withRouterConfig,
  withViewTransitions
} from '@angular/router';
import { IconSetService } from '@coreui/icons-angular';
import { routes } from './app.routes';
import { jwtInterceptor } from './interceptors/jwt.interceptor'; // Importe ton intercepteur
import { provideHttpClient, withInterceptors } from '@angular/common/http'; // Ajoute withInterceptors

export const appConfig: ApplicationConfig = {
  providers: [
    
    provideAnimations(), // Requis pour les animations du toast
    provideToastr({
      timeOut: 4000,
      positionClass: 'toast-bottom-left',
      preventDuplicates: true,
      closeButton: true 
    }),

    provideHttpClient(
      withInterceptors([httpErrorInterceptor]) // On l'ajoute ici
    ),

    provideRouter(routes,
      withRouterConfig({
        onSameUrlNavigation: 'reload'
      }),
      
      withInMemoryScrolling({
        scrollPositionRestoration: 'top',
        anchorScrolling: 'enabled'
      }),
      withEnabledBlockingInitialNavigation(),
      withViewTransitions(),
      withHashLocation()
    ),
    provideHttpClient(
      withInterceptors([jwtInterceptor]) // <--- On active l'intercepteur ici
    ),
    providePrimeNG({
      theme: {
        preset: Aura,
        options: {
          darkModeSelector: false, // Force le mode clair pour matcher CoreUI
          ripple: true
        }
      },
      // Personnalisation des z-index pour éviter les conflits avec CoreUI et afficher les p-treeselect au-dessus des autres éléments
      zIndex: {
        modal: 1100,    // Pour les modales PrimeNG
        overlay: 5000,  // Pour les dropdowns, treeselect, etc. <--- AUGMENTE CECI
        menu: 1000,
        tooltip: 1100
      }
    }),
    IconSetService,
    ConfirmationService,
    provideAnimationsAsync()

    
    
  ]
};

