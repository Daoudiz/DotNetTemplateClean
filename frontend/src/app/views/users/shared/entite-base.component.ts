/**************************
 * EntiteBaseComponent
 * 
 * Ce composant abstrait centralise la logique de gestion des listes de directions, divisions, services et rôles.
 * Il fournit des méthodes pour initialiser ces listes et gérer les changements dans les sélecteurs de direction et division.
 * Les composants enfants (UserCreateComponent et UserEditComponent) héritent de cette classe pour bénéficier de cette logique partagée.
 **************************/

import { Directive, signal } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { UserService } from '../../../services/user/user.service';

@Directive()
export abstract class EntiteBaseComponent {
    // Signaux pour stocker les listes
    directions = signal<any[]>([]);
    divisions = signal<any[]>([]);
    services = signal<any[]>([]);
    roles = signal<any[]>([]);

    constructor(protected userService: UserService) { }

    // Charger toutes les directions 
    protected initDirections(): void {
        this.userService.getDirections().subscribe(data => this.directions.set(data));
    }

    //charger tous les rôles
    protected initRoles(): void {
        this.userService.getRoles().subscribe({
            next: (data) => this.roles.set(data)
            
        });
    }    

    protected handleDirectionChange(form: FormGroup): void {
        const dirId = form.get('direction')?.value;
        

        // 1. Nettoyage immédiat de TOUTE la descendance
        this.divisions.set([]);
        this.services.set([]);

        // 2. Reset des contrôles dans le formulaire
        // On utilise null pour s'assurer que l'état du formulaire est propre
        form.patchValue({
            division: null,
            service: null
        });

        // 3. Vérification stricte de l'ID avant l'appel API
        // On élimine 0, null, undefined et la string "null"
        if (dirId && dirId !== 'null' && dirId !== 0) {
            this.userService.getDivisions(dirId).subscribe({
                next: (data) => {
                    this.divisions.set(data);
                },
                error: (err) => {
                    console.error("Erreur chargement divisions :", err);
                    this.divisions.set([]);
                }
            });
        }
    }

    // Gérer le changement de Division
    protected handleDivisionChange(form: FormGroup): void {
        const divId = form.get('division')?.value;

        this.services.set([]);
        form.get('service')?.setValue(null);

        if (divId && divId !== 'null' && divId !== 0) {
            this.userService.getServices(divId).subscribe({
                next: (data) => this.services.set(data),                
            });
        }
    }
}