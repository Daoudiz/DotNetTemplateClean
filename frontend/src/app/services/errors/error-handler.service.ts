import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { ProblemDetails } from '../../models/errors/problem-details.model';
import { NotificationService } from '../../services/notification.service';
import { HttpRequest, HttpErrorResponse } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class ErrorHandlerService {

    constructor(
        private notifier: NotificationService,
        private router: Router 
    ) { }

    handle(error: HttpErrorResponse, req: HttpRequest<any>): void {

        let message = 'Une erreur inattendue est survenue';
        const backendResult = error.error;
        let title = 'Erreur';
        let problem: ProblemDetails | undefined;
       
        //Gestion prioritaire du code 0 (Réseau/Serveur éteint)
        if (error.status === 0) {
            message = 'Le serveur est injoignable. Veuillez vérifier votre connexion ou l\'état du service.';
            title = 'Réseau / Serveur';
        }
        else {

        message =
            backendResult?.detail ||
            backendResult?.error ||
            backendResult?.message ||
            (typeof backendResult === 'string' ? backendResult : null) ||
            'Une erreur inattendue est survenue';
        
        //Identification du titre ou de l'objet ProblemDetails complet
            if (backendResult && typeof backendResult === 'object') {
                problem = backendResult as ProblemDetails;
            }
        }

        switch (error.status) {

            case 0: // AJOUT : Gérer le code 0 explicitement
                this.notifier.error(message, 'Serveur hors-ligne');
                break;

            case 400:
                this.notifier.warn(message, 'Requête invalide');
                break;

            case 401:               
                // On vérifie si l'erreur vient de l'endpoint de connexion
                if (req.url.includes('/auth/login') || req.url.includes('/Account/login')) {
                   
                    
                } else {
                    // C'est une vraie expiration de session sur une autre page
                    this.notifier.error(
                        'Session expirée, veuillez vous reconnecter',
                        'Authentification'
                    );                    
                    this.router.navigate(['/login']);
                }
                break;

            case 403:
                this.notifier.error('Accès interdit', 'Sécurité');
                break;

            case 409:
                this.notifier.warn(message, 'Conflit métiers');                
                break;

            case 500:
                this.notifier.error(
                    'Erreur interne du serveur',
                    'Serveur'
                );
                break;

            default:
                // On ne déclenche le default que si ce n'est pas une erreur réseau (0)
                if (error.status !== 0) {
                    this.notifier.error(message);
                }
                break;
        }

        // Utile pour le support / debug backend
        if (problem?.traceId) {
            console.error('TraceId backend:', problem.traceId);
        }
    }
}
