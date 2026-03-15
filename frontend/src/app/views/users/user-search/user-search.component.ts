/******************************************************************************************
 * @file user-search.component.ts
 * @description Module de recherche avancée des utilisateurs.
 * Intègre une gestion de filtres hiérarchiques et une réactivité basée sur les Signals.
 * @module UserManagement
 * @author Zakaria DAOUDI
 * @copyright © 2026 - Hope
 *******************************************************************************************/
import { Component, OnInit, inject, signal, ChangeDetectionStrategy, computed, ViewChild ,ElementRef } from '@angular/core';
import { CommonModule, UpperCasePipe, DatePipe } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, Subject, of, merge } from 'rxjs';
import { catchError, finalize, switchMap, tap, map } from 'rxjs/operators';
import { UserCreateComponent } from '../user-create/user-create.component'; 
import { Table, TableModule } from 'primeng/table';
import { NotificationService } from '../../../services/notification.service';

// CoreUI Modules
import {
  CardModule, GridModule, FormModule, ButtonModule,
  SpinnerModule, UtilitiesModule, AlertModule, ModalModule, TooltipModule
} from '@coreui/angular';
import { IconModule } from '@coreui/icons-angular'; 

// Models & Services
import { UserService } from '../../../services/user/user.service';
import { ApplicationUser, Entite, UserSearchCriteria } from '../../../models/user/user-models';
import { Modal } from 'bootstrap';


@Component({
  selector: 'app-user-search',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, UserCreateComponent, CardModule, GridModule,
    FormModule, ButtonModule, TableModule, SpinnerModule, TooltipModule,
    UtilitiesModule, IconModule, AlertModule, ModalModule, UpperCasePipe, DatePipe
  ],
  templateUrl: './user-search.component.html',
  styleUrls: ['./user-search.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush // Très important avec les Signals
})


export class UserSearchComponent implements OnInit {

//#region --- INJECTIONS ET RÉFÉRENCES ---
  private readonly fb = inject(FormBuilder);
  private readonly userService = inject(UserService);
  private notification = inject(NotificationService);
  
  @ViewChild('dt') table?: Table;
  @ViewChild('globalSearchInput') globalSearchInput!: ElementRef;
  //#endregion

//#region --- SIGNALS D'ÉTAT (UI & Données) ---
  readonly directions = signal<Entite[]>([]);
  readonly divisions = signal<Entite[]>([]);
  readonly services = signal<Entite[]>([]);
  
  readonly loading = signal<boolean>(false);
  readonly hasSearched = signal<boolean>(false); 
  readonly totalRecords = signal<number>(0); // Nouveau Signal pour le compteur
  readonly pageSize = signal<number>(10);    // Taille par défaut de page
  readonly isLazyMode = signal<boolean>(true);
  readonly selectedUser = signal<any | null>(null);
  readonly isEditMode = signal<boolean>(false);
  public isUserModalOpen = signal<boolean>(false); // État de la modale de création d'utilisateur
  //#endregion

//#region  --- LOGIQUE DE RECHERCHE (RxJS) ----------
  private readonly searchAction$ = new Subject<UserSearchCriteria>();
  private readonly resetAction$ = new Subject<void>();
  searchForm!: FormGroup;
   
  /** * Flux principal des utilisateurs.
   * Fusionne les actions de recherche et de réinitialisation.
   */
  readonly users$: Observable<ApplicationUser[]> = merge(
    // --- ACTION DE RECHERCHE ---
    this.searchAction$.pipe(
      tap(() => this.loading.set(true)),
      switchMap(criteria => this.userService.searchUsers(criteria).pipe(
        tap(res => {
          this.totalRecords.set(res.totalCount);

          // LOGIQUE HYBRIDE : 
          // Si le serveur dit IsFullResult = true, on désactive le Lazy côté PrimeNG
          // Cela permet au filtre global de chercher dans TOUT le tableau reçu.
          this.isLazyMode.set(!res.isFullResult);
        }),
        map(res => res.items),
        catchError(() => {
          this.totalRecords.set(0);
          return of([]);
        }),
        finalize(() => this.loading.set(false))
      ))
    ),

    // --- ACTION DE RESET ---
    this.resetAction$.pipe(
      tap(() => {
        this.totalRecords.set(0);
        this.loading.set(false);
        this.isLazyMode.set(true); // On remet en Lazy par défaut pour la prochaine fois
      }),
      map(() => [])
    )
  ); 
  //#endregion

//#region ---ACTIONS UTILISATEUR (Recherche & Reset) ---
  /**
  * Exécute la recherche en envoyant les critères actuels du formulaire
  * au flux searchAction$.
  */
  onSearch(): void {
    this.hasSearched.set(true);

    if (this.table) {
      // Si on est sur une page autre que la première (ex: page 2), 
      // reset() va nous ramener à la page 1 et déclencher onLazyLoad.
      if (this.table.first !== 0) {
        this.table.reset();
      } else {
        // Si on est DÉJÀ sur la page 1, reset() ne fera rien.
        // On force donc l'appel manuellement.
        this.loadUsersLazy({
          first: 0,
          rows: this.table.rows || 10
        });
      }
    } else {
      this.searchAction$.next({ ...this.searchForm.value, pageNumber: 1, pageSize: 10 });
    }
  }
  
  onReset(): void {
    // 1. Reset du formulaire et des signaux
    this.searchForm.reset();
    this.resetCascade('all');
    this.hasSearched.set(false);
    this.isLazyMode.set(true);

    // 2. Vider l'input visuel (ElementRef)
    if (this.globalSearchInput) {
      this.globalSearchInput.nativeElement.value = '';
    }

    // 3. Reset de la table
    if (this.table) {
      // On remet la pagination à zéro
      this.table.first = 0;

      // On vide les filtres internes proprement via l'API PrimeNG
      this.table.filters = {};

      // Au lieu de .globalFilter, on utilise la méthode officielle
      //this.table.filterGlobal('', 'contains');

      // On laisse un petit cycle à Angular pour digérer
      setTimeout(() => {
        if (this.table) {
          this.table.clear(); // Nettoie le reste (tris, etc.)
          this.resetAction$.next(); // Déclenche ton chargement de données
        }
      }, 0);
    }
  }

  /**
     * Methode appelée par PrimeNG lors du chargement lazy loading de la table.
     * @param event 
     * @returns 
     */
  loadUsersLazy(event: any): void {

    // On ne bloque QUE si on n'a jamais cherché.
    if (!this.hasSearched()) return;

    // Si on est en mode "Smart" (isLazyMode = false), on autorise l'appel 
    // uniquement si c'est le bouton Rechercher qui force (donc page 1).
    // Si c'est un changement de page auto de PrimeNG, on laisse le return.
    if (!this.isLazyMode() && event.first !== 0) return;

    const pageNumber = (event.first / event.rows) + 1;
    const pageSize = event.rows;

    const criteria: UserSearchCriteria = {
      ...this.searchForm.value,
      pageNumber: pageNumber,
      pageSize: pageSize
    };

    this.searchAction$.next(criteria);
  }

  //Permet de vérouiller/déverouiller un utilisateur
  onToggleUserStatus(user: any): void {

    const userId = user.id;

    this.userService.deleteUser(userId).subscribe({
      next: () => {

        // On change l'état LOCALEMENT pour éviter de recharger toute la page       
        user.isLocked = !user.isLocked;

        this.notification.success('Statut mis à jour');
      }
    });
  }

//#endregion

//#region ---CYCLE DE VIE & INIT ---
  ngOnInit(): void {

    this.initForm();
    this.loadInitialData();
  }

  private initForm(): void {
    this.searchForm = this.fb.group({
      matricule: [null],
      nom: [''],
      prenom: [''],
      dateRecrutementDebut: [null],
      dateRecrutementFin: [null],
      directionId: [null],
      divisionId: [null],
      serviceId: [null]
    });
  }

  /**
  * Charge les données initiales nécessaires au formulaire de recherche (ex: Directions).
  */
  private loadInitialData(): void {
    this.userService.getDirections().subscribe({
      next: (data) => this.directions.set(data)
    });
  }

//#endregion
 
//#region --- CASCADING DROPDOWNS ---
  onDirectionChange(): void {
    const dirId = this.searchForm.get('directionId')?.value;
    this.resetCascade('division');

    if (dirId) {
      this.userService.getDivisions(dirId).subscribe(data => this.divisions.set(data));
    }
  }

  onDivisionChange(): void {
    const divId = this.searchForm.get('divisionId')?.value;
    this.resetCascade('service');

    if (divId) {
      this.userService.getServices(divId).subscribe(data => this.services.set(data));
    }
  }
    
  /**
   * méthode utilitaire pour réinitialiser les listes déroulantes en cascade.
   * @param level 
   */
  private resetCascade(level: 'division' | 'service' | 'all'): void {
    if (level === 'division' || level === 'all') {
      this.divisions.set([]);
      this.searchForm.patchValue({ divisionId: null });
    }
    if (level === 'service' || level === 'all') {
      this.services.set([]);
      this.searchForm.patchValue({ serviceId: null });
    }
  }

  //#endregion

//#region  ---GESTION DES MODALES (Users) ---
  /**
   * Méthode pour ouvrir/fermer la modale de création/édition d'utilisateur.
   * 
   */
  closeUserModal() {
    this.isUserModalOpen.set(false);
    // Si on est en train de fermer la modal, on reset l'utilisateur
    if (!this.isUserModalOpen()) {
      this.selectedUser.set(null);
    }
  }
  /**
   * methode appelée par la modale de création/édition d'utilisateur pour indiquer que l'opération a réussi.
   * @param event 
   */
  onModalVisibilityChange(event: any) {
    // On vérifie si la valeur a changé pour éviter les boucles
    if (this.isUserModalOpen() !== event) {
      this.isUserModalOpen.set(event);

      // Si la modale est fermée (via ESC ou clic extérieur), on nettoie
      if (!event) {
        this.selectedUser.set(null);
      }
    }
  }
  openCreateModal() {
    this.selectedUser.set(null); // On vide
    this.isEditMode.set(false);
    this.isUserModalOpen.set(true);
    
  }

  openEditModal(user: any) {
    this.selectedUser.set(user); // On passe l'user sélectionné
    this.isEditMode.set(true);
    this.isUserModalOpen.set(true);
  }

  onUserCreatedSuccess() {
    this.isUserModalOpen.set(false); // Ferme la modale
    this.onSearch(); // Rafraîchit la liste
  }  

 
//#endregion
 
 
}