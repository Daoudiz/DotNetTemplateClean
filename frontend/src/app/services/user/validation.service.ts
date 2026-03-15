import { Injectable } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Injectable({ providedIn: 'root' })
export class ValidationService {

    // Détecte si le champ est en erreur et a été touché
    isFieldInvalid(form: FormGroup, fieldName: string): boolean {
        const control = form.get(fieldName);

        // On enlève 'dirty' pour ne pas harceler l'utilisateur pendant la frappe
        // L'erreur ne s'affichera que lorsque l'utilisateur cliquera ailleurs (blur)
        return !!(control && control.invalid && control.touched);
    }

    // Centralise tous les messages d'erreur du projet
    getErrorMessage(form: FormGroup, fieldName: string): string {
        const control = form.get(fieldName);
        if (!control || !control.errors) return '';

        const errors = control.errors;

        if (errors['required']) return 'Ce champ est obligatoire.';
        if (errors['email']) return 'Format d\'adresse email invalide.';
        if (errors['minlength']) return `Minimum ${errors['minlength'].requiredLength} caractères requis.`;
        if (errors['mustMatch']) return 'Les mots de passe ne correspondent pas.';
        if (errors['pattern']) {
            // Si la valeur contient autre chose que des chiffres, on affiche le message spécifique
            const value = control.value;
            if (value && /[^0-9]/.test(value)) {
                return 'Ce champ doit contenir uniquement des chiffres.';
            }
            return 'Le format saisi est incorrect.';
        }
        return 'Champ invalide.';
    }
}