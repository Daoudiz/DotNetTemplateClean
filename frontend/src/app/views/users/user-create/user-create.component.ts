/**************************
 * UserCreateComponent
 * 
 * Ce composant gère à la fois la création et l'édition d'un utilisateur.
 * Il hérite de EntiteBaseComponent pour bénéficier de la logique partagée de gestion des listes de directions, divisions, services et rôles.
 * 
 * Fonctionnalités principales :    
 * - Initialisation du formulaire avec des validateurs adaptés à la création ou à l'édition.
 * - Gestion des changements dans les sélecteurs de direction et division pour charger dynamiquement les listes associées.
 * - Préparation d'un payload unifié pour les appels API de création et d'édition, avec une logique conditionnelle pour les champs spécifiques à chaque mode.
 * - Affichage de notifications de succès après la création ou la mise à jour d'un utilisateur.
 **************************/

import { Component, OnInit, inject, Output, EventEmitter, signal, Input } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { EntiteBaseComponent } from '../shared/entite-base.component';
import { UserService } from '../../../services/user/user.service';
import { CreateUserViewModel } from '../../../models/user/user-models';
import { MustMatch } from '../../../validators/user-validators';
import { ValidationService } from '../../../services/user/validation.service';
import  Swal from 'sweetalert2';
import { NotificationService } from '../../../services/notification.service';

@Component({
  selector: 'app-user-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user-create.component.html'
})
export class UserCreateComponent extends EntiteBaseComponent implements OnInit {
  
  constructor(userService: UserService) {
    super(userService);
  }

//#region --- INJECTIONS & PROPRIÉTÉS ---
  private fb = inject(FormBuilder);
  public valService = inject(ValidationService);
  private notification = inject(NotificationService);

  public userForm!: FormGroup;
  public isEditMode = false;
  private _userToEdit: any;

  errorMessage = signal<string>(''); 

  @Output() cancel = new EventEmitter<void>(); 
  @Output() userCreated = new EventEmitter<void>();
  
 //#endregion

//#region --- GESTION DE L'INPUT D'UTILISATEUR À ÉDITER ---
  @Input() set userToEdit(user: any | null) {
    this._userToEdit = user;
    if (!this.userForm) return;
   
  this.configureFormForMode(user);
  
    }

  private configureFormForMode(user: any | null): void {
    this.userForm.reset();
    this.divisions.set([]);
    this.services.set([]);

    if (!user) {
      this.isEditMode = false;
      this.togglePasswordValidators(true);
      this.userForm.patchValue({
        dateRecrutement: this.getToday()
      });
      return;
    }

    this.isEditMode = true;
    this.togglePasswordValidators(false);
    this.patchUserData(user);
    this.loadCascadeData(user);
  }

  private patchUserData(user: any): void {
    this.userForm.patchValue({
      matricule: user.matricule,
      firstName: user.prenom,
      lastName: user.nom,
      userName: user.userName,
      email: user.email,
      userRole: user.roleId,
      direction: user.directionId,
      division: user.divisionId,
      service: user.serviceId,
      dateRecrutement: this.formatDate(user.dateRecrutement)
    });
  }


//#endregion  

//#region --- INITIALISATION ---
  ngOnInit(): void {
    this.initDirections();
    this.initRoles();

    this.userForm = this.fb.group({
      // Identité
      matricule: ['', [Validators.required, Validators.pattern("^[0-9]*$")]],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      userRole: [null, Validators.required],
      dateRecrutement: [new Date().toISOString().substring(0, 10), [Validators.required]],

      // Hiérarchie
      direction: [null, [Validators.required]],
      division: [null],
      service: [null],
      

      // Sécurité
      userName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required],
      
      twoFactorEnabled: [false]
    }, { validators: MustMatch('password', 'confirmPassword') });

   
    // 2. Si un utilisateur était déjà en attente (clic sur Crayon), on l'applique maintenant
    if (this._userToEdit) {
      this.isEditMode = true;
      this.userForm.patchValue(this._userToEdit);
      this.togglePasswordValidators(false);
    }
  }
//#endregion

//#region --- ACTIONS PRINCIPALES (Submit, Cancel) ---
  
onSubmit(): void {

      if (this.userForm.invalid) {
        this.userForm.markAllAsTouched();
        return;
      }

      const payload = this.buildPayload();

      if (this.isEditMode) {
        this.updateUser(payload);
      } else {
        this.createUser(payload);
      }

    /*if (this.userForm.valid) {
      const rawData = this.userForm.value;

      //Préparation du Payload commun à lédition et la création, avec des conversions nécessaires
      const payload: any = {
        matricule: rawData.matricule,
        firstName: rawData.firstName,
        lastName: rawData.lastName,
        userName: rawData.userName,
        email: rawData.email,
        userRole: rawData.userRole,
        direction: Number(rawData.direction),
        division: rawData.division ? Number(rawData.division) : undefined,
        service: rawData.service ? Number(rawData.service) : undefined,
        dateRecrutement: rawData.dateRecrutement,
        twoFactorEnabled: rawData.twoFactorEnabled === true || rawData.twoFactorEnabled === 'true'
      };

      // On récupère l'ID depuis l'objet initial (stocké lors du patchValue)
      if (this.isEditMode) {        
        
        const userId = this._userToEdit.id;       
        const payloadWithId = {
          ...payload,
          userId: userId 
        };        

        this.userService.updateUser(userId, payloadWithId).subscribe({
          next: () => {
            this.notification.success("Utilisateur mis à jour avec succès.", "Mise à jour réussie");
            this.userCreated.emit(); // On déclenche le rafraîchissement du tableau
            //this.onCancel();
          }
        });

      // On ajoute les mots de passe uniquement en création
      } else {        
        
        payload.password = rawData.password;
        payload.confirmPassword = rawData.confirmPassword;

        this.userService.createUser(payload).subscribe({
          next: () => {
            this.notification.success("L'utilisateur a été créé avec succès.", "Création réussie");
            this.userCreated.emit();
            this.onCancel();
          }
        });
      }
    } else {
      this.userForm.markAllAsTouched();
    }*/
  }

  onCancel(): void {
    this.userForm.reset(); //Vide tous les champs
    
    //Remet les listes Division/Service à zéro pour éviter les restes de cascades
    this.divisions.set([]);
    this.services.set([]);
    
    //Ferme la modale
    this.cancel.emit();

    this.errorMessage.set(''); 
  }
  
  private createUser(payload: any): void {
    const raw = this.userForm.value;

    this.userService.createUser({
      ...payload,
      password: raw.password,
      confirmPassword: raw.confirmPassword
    }).subscribe(() => {
      this.notification.success("L'utilisateur a été créé avec succès.", "Création réussie");
      this.userCreated.emit();
      this.onCancel();
    });
  }  

  private updateUser(payload: any): void {
    const userId = this._userToEdit.id;

    this.userService.updateUser(userId, {
      ...payload,
      userId
    }).subscribe(() => {
      this.notification.success("Utilisateur mis à jour avec succès.", "Mise à jour réussie");
      this.userCreated.emit();
    });
  }

  private buildPayload(): any {
    const raw = this.userForm.value;

    return {
      matricule: raw.matricule,
      firstName: raw.firstName,
      lastName: raw.lastName,
      userName: raw.userName,
      email: raw.email,
      userRole: raw.userRole,
      direction: Number(raw.direction),
      division: raw.division ? Number(raw.division) : undefined,
      service: raw.service ? Number(raw.service) : undefined,
      dateRecrutement: raw.dateRecrutement,
      twoFactorEnabled: !!raw.twoFactorEnabled
    };
  }


//#endregion

//#region --- CASCADES DYNAMIQUES (Direction -> Division -> Service) ---
  onDirChange() { this.handleDirectionChange(this.userForm); }
  onDivChange() { this.handleDivisionChange(this.userForm); }
//#endregion

//#region --- FONCTIONS UTILITAIRES ---
  
// fonction utilitaire pour activer/désactiver les validateurs de mot de passe selon le mode (création vs édition)
  private togglePasswordValidators(isRequired: boolean) {
    const psw = this.userForm.get('password');
    const conf = this.userForm.get('confirmPassword');

    if (isRequired) {
      psw?.setValidators([Validators.required, Validators.minLength(6)]);
      conf?.setValidators([Validators.required]);
    } else {
      psw?.clearValidators();
      conf?.clearValidators();
    }
    psw?.updateValueAndValidity();
    conf?.updateValueAndValidity();
  }  

  private loadCascadeData(user: any): void {
    if (!user.directionId) return;

    this.userService.getDivisions(user.directionId).subscribe(divs => {
      this.divisions.set(divs);

      if (!user.divisionId) return;

      this.userService.getServices(user.divisionId).subscribe(servs => {
        this.services.set(servs);
      });
    });
  }

  private getToday(): string {
    return new Date().toISOString().split('T')[0];
  }

  private formatDate(date?: string | Date | null): string {
    if (!date) return '';

    const d = new Date(date);

    // Correction du décalage en UTC
    const tzOffset = d.getTimezoneOffset() * 60000; // en ms
    const localDate = new Date(d.getTime() - tzOffset);

    // Format YYYY-MM-DD
    return localDate.toISOString().split('T')[0];
  }


//#endregion
}