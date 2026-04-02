import { Injectable } from '@angular/core';
import { HttpErrorResponse, HttpRequest } from '@angular/common/http';
import { Router } from '@angular/router';

import { ProblemDetails } from '../../models/errors/problem-details.model';
import { NotificationService } from '../../services/notification.service';

@Injectable({ providedIn: 'root' })
export class ErrorHandlerService {

    constructor(
        private notifier: NotificationService,
        private router: Router
    ) { }

    handle(error: HttpErrorResponse, req: HttpRequest<unknown>): void {
        if (error.status === 0) {
            this.notifier.error(
                'Le serveur est injoignable. Veuillez vérifier votre connexion ou l’état du service.',
                'Réseau / Serveur'
            );
            return;
        }

        const problem = this.extractProblemDetails(error);
        const message = this.buildMessage(error, problem);
        const title = problem?.title?.trim();


        switch (error.status) {
            case 400:
                this.notifier.warn(message, 'Requête invalide');
                break;

            case 401:
                if (!this.isLoginRequest(req)) {
                    this.notifier.error(
                        message || 'Session expirée, veuillez vous reconnecter',
                        title || 'Authentification'
                    );
                    this.router.navigate(['/login']);
                }
                break;

            case 403:
                this.notifier.error(message, title || 'Sécurité');
                break;

            case 404:
                this.notifier.warn(message, title || 'Ressource introuvable');
                break;

            case 409:
                this.notifier.warn(message, title || 'Conflit métier');
                break;

            case 500:
                this.notifier.error(message, title || 'Serveur');
                break;

            default:
                this.notifier.error(message, title || 'Erreur');
                break;
        }

        if (problem?.traceId) {
            console.error('TraceId backend:', problem.traceId);
        }

        if (problem?.errors && Object.keys(problem.errors).length > 0) {
            console.error('Validation errors:', problem.errors);
        }
    }

    private extractProblemDetails(error: HttpErrorResponse): ProblemDetails | undefined {
        const payload = error.error;

        if (!payload || typeof payload !== 'object') {
            return undefined;
        }

        return payload as ProblemDetails;
    }

    private buildMessage(error: HttpErrorResponse, problem?: ProblemDetails): string {
        const payload = error.error;

        if (problem?.errors && Object.keys(problem.errors).length > 0) {
            return this.flattenValidationErrors(problem.errors);
        }

        if (problem?.detail?.trim()) {
            return problem.detail;
        }

        if (problem?.title?.trim() && error.status !== 500) {
            return problem.title;
        }

        if (payload && typeof payload === 'object') {
            const objectPayload = payload as Record<string, unknown>;
            const errorMessage = this.asNonEmptyString(objectPayload['message'])
                || this.asNonEmptyString(objectPayload['error']);

            if (errorMessage) {
                return errorMessage;
            }
        }

        if (typeof payload === 'string' && payload.trim()) {
            return payload;
        }

        return this.getDefaultMessage(error.status);
    }

    private flattenValidationErrors(errors: Record<string, string[]>): string {
        const messages = Object.entries(errors)
            .flatMap(([field, fieldErrors]) => {
                const prefix = field ? `${field}: ` : '';
                return fieldErrors.map(error => `${prefix}${error}`);
            })
            .filter(message => message.trim().length > 0);

        return messages.length > 0
            ? messages.join(' | ')
            : 'Des erreurs de validation ont été détectées.';
    }

    private getDefaultMessage(status: number): string {
        switch (status) {
            case 400:
                return 'La requête envoyée est invalide.';
            case 401:
                return 'Vous devez vous authentifier pour continuer.';
            case 403:
                return 'Vous n’êtes pas autorisé à effectuer cette action.';
            case 404:
                return 'La ressource demandée est introuvable.';
            case 409:
                return 'Un conflit empêche l’exécution de cette opération.';
            case 500:
                return 'Une erreur interne du serveur est survenue.';
            default:
                return 'Une erreur inattendue est survenue.';
        }
    }

    private isLoginRequest(req: HttpRequest<unknown>): boolean {
        return req.url.includes('/auth/login') || req.url.includes('/Account/login');
    }

    private asNonEmptyString(value: unknown): string | null {
        return typeof value === 'string' && value.trim().length > 0 ? value : null;
    }
}
