/******************************************************************************************
* @file entite-search.component.ts
* @description Module de recherche avancée des entités.
* Intègre une gestion de filtres hiérarchiques et une réactivité basée sur les Signals.
* @module Gestion des unités organisationnelles
* @author Zakaria DAOUDI
* © 2026 - Hope
*******************************************************************************************/
import { Component, OnInit, inject, signal, viewChild, ChangeDetectionStrategy, ElementRef, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Observable, Subject, merge, of } from 'rxjs';
import { catchError, finalize, switchMap, tap, map } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { Table, TableModule, TableLazyLoadEvent } from 'primeng/table';
import { TreeSelectModule } from 'primeng/treeselect';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';

// CoreUI Modules
import {
  CardModule, GridModule, FormModule, ButtonModule,
  SpinnerModule, UtilitiesModule, AlertModule, ModalModule, TooltipModule
} from '@coreui/angular';
import { IconModule } from '@coreui/icons-angular';

// Models & Services
import { OrganizationService } from '../../../services/organisation/organisation.service';
import {  EntiteItemSearchResponse, 
          EntiteSearchRequest, 
          OrganizationTreeNode, 
          TypeEntite 
        } from '../../../models/organisation/organisation-model';
import { NotificationService } from '../../../services/notification.service';
import { EntiteCreateComponent } from '../entite-create/entite-create.component';


@Component({
  selector: 'app-entite-search',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, EntiteCreateComponent, CardModule, GridModule,
    FormModule, ButtonModule, TableModule, SpinnerModule, TooltipModule,
    UtilitiesModule, IconModule, AlertModule, ModalModule, TreeSelectModule,  ConfirmDialogModule
  ],
  templateUrl: './entite-search.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EntiteSearchComponent implements OnInit {

  //#region --- INJECTIONS & RÉFÉRENCES ---
  private readonly fb = inject(FormBuilder);
  private readonly organisationService = inject(OrganizationService); 
  private readonly notification = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly confirmationService = inject(ConfirmationService);

  // ViewChild version Signal (Angular 17.2+)
  readonly table = viewChild<Table>('dt');
  readonly globalSearchInput  = viewChild<ElementRef>('globalSearchInput')  ;

  //#endregion

  //#region --- SIGNALS D'ÉTAT (UI) ---
  readonly typeEntite = signal<TypeEntite[]>([]);
  readonly nodes = signal<OrganizationTreeNode[]>([]);
  readonly loading = signal<boolean>(false);
  readonly hasSearched = signal<boolean>(false);
  readonly totalRecords = signal<number>(0); // Nouveau Signal pour le compteur
  readonly pageSize = signal<number>(10);    // Taille par défaut de page
  readonly isLazyMode = signal<boolean>(true);
  readonly selectedEntite = signal<EntiteItemSearchResponse | null>(null);
  readonly isEditMode = signal<boolean>(false);
  public isEntiteModalOpen = signal<boolean>(false); // État de la modale de création d'entité
  //#endregion

  //#region --- LOGIQUE DE RECHERCHE (RxJS) ---
 private readonly searchAction$ = new Subject<EntiteSearchRequest>();
private readonly resetAction$ = new Subject<void>();
   searchForm!: FormGroup;

  readonly entites$: Observable<EntiteItemSearchResponse[]> = merge(
      // --- ACTION DE RECHERCHE ---
      this.searchAction$.pipe(
        tap(() => this.loading.set(true)),
        switchMap(criteria => this.organisationService.searchEntites(criteria).pipe(
          tap(res => {
            this.totalRecords.set(res.totalCount);
  
            // LOGIQUE HYBRIDE : 
            // Si le serveur dit IsFullResult = true, on désactive le Lazy côté PrimeNG
            // Cela permet au filtre global de chercher dans TOUT le tableau reçu.
            this.isLazyMode.set(!res.isFullResult);
          }),
          map(res => res.items),
          catchError((error) => {
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

  //#region --- ACTIONS UTILISATEUR (Recherche & Reset) ---
  /**
    * Exécute la recherche en envoyant les critères actuels du formulaire
    * au flux searchAction$.
    */   

  onSearch(): void {
    this.hasSearched.set(true);
    
    this.table()?.reset(); 

  // On déclenche l'action de recherche (le pipe switchMap va s'occuper du reste)
    this.searchAction$.next(this.getParams());
  }
    
    onReset(): void {
      //Reset du formulaire et des signaux
      this.searchForm.reset();      
      this.hasSearched.set(false);
      this.isLazyMode.set(true);
  
      //Vider l'input visuel (ElementRef)
      const input = this.globalSearchInput();
      if (input) {
        input.nativeElement.value = '';
      }
      //Reset de la table
      const resultTable = this.table();
      if (resultTable) {
        // On remet la pagination à zéro
        resultTable.first = 0;
  
        // On vide les filtres internes proprement via l'API PrimeNG
        resultTable.filters = {};
  
        // Au lieu de .globalFilter, on utilise la méthode officielle
        //this.table.filterGlobal('', 'contains');
  
        // On laisse un petit cycle à Angular pour digérer
        setTimeout(() => {
          if (this.table) {
            this.table()?.clear(); // Nettoie le reste (tris, etc.)
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
  loadEntitesLazy(event: TableLazyLoadEvent): void {
  
      // On ne bloque QUE si on n'a jamais cherché.
      if (!this.hasSearched()) return;
  
      // Si on est en mode "Smart" (isLazyMode = false), on autorise l'appel 
      // uniquement si c'est le bouton Rechercher qui force (donc page 1).
      // Si c'est un changement de page auto de PrimeNG, on laisse le return.
      const first = event.first ?? 0;
      if (!this.isLazyMode() && first !== 0) return;

      const rows = event.rows ?? this.pageSize();
      this.pageSize.set(rows); 
     
      this.searchAction$.next(this.getParams());
    } 
    
  //#endregion

  //#region ---CYCLE DE VIE & INIT ---
  ngOnInit(): void {

    this.initForm();
    this.loadInitialData();
  }

  private initForm(): void {
    this.searchForm = this.fb.group({
      searchTerm: [''],
      typeEntiteId: [null],
      parentId: [null]        
    });
  }

  /**
  * Charge les données initiales nécessaires au formulaire de recherche (ex: Directions).
  */
  private loadInitialData(): void {
    this.organisationService.getAllTypes()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.typeEntite.set(data);
        },        
      });

    // Chargement de l'arbre d'organisation pour le filtre hiérarchique
    // On s'assure que chaque nœud possède une clé unique pour que PrimeNG
    // gère correctement la sélection, même après filtrage.
    this.organisationService.getOrganizationTree()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          const treeWithKeys = this.organisationService.addKeysToTree(data);         
         
          this.nodes.set(treeWithKeys);
        }
      });
  }

  //#endregion

  //#region  ---GESTION DES MODALES (Users) ---
  openCreateModal(): void {
    this.isEditMode.set(false);
    this.selectedEntite.set(null);
    this.isEntiteModalOpen.set(true);
  }

  openEditModal(unit: EntiteItemSearchResponse): void {
    this.isEditMode.set(true);
    this.selectedEntite.set(unit);
    this.isEntiteModalOpen.set(true);
  }

  closeEntiteModal(): void {
    this.isEntiteModalOpen.set(false);
    if (!this.isEntiteModalOpen()) {
      this.selectedEntite.set(null);
    }
  }

  onEntiteModalVisibilityChange(visible: boolean): void {
    if (this.isEntiteModalOpen() !== visible) {
      this.isEntiteModalOpen.set(visible);
      if (!visible) {
        this.selectedEntite.set(null);
      }
    }
  }

  onEntiteSavedSuccess(): void {
    this.isEntiteModalOpen.set(false);
    this.onSearch();
  }  
  //#endregion

  //#region  ---SUPPRESSION ENTITE ---

  onDeleteEntite(unit: EntiteItemSearchResponse): void {
    this.confirmationService.confirm({
      message: `Êtes-vous sûr de vouloir supprimer l'unité "<strong>${unit.libelle}</strong>" ?`,
      header: 'Confirmation de suppression',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Supprimer',
      rejectLabel: 'Annuler',
      acceptButtonStyleClass: 'p-button-danger',

      accept: () => {
        this.executeDelete(unit.id);
      }
    });
  }

  private executeDelete(id: number): void {
    this.loading.set(true);

    this.organisationService.deleteEntite(id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        
        finalize(() => this.loading.set(false))
      )
      .subscribe(() => {
        this.notification.success('Unité supprimée avec succès');
        this.onSearch(); // Rafraîchissement de la liste
      });
  }

  //#endregion

  //#region --- HELPERS & UTILITAIRES ---
  private getParams(): EntiteSearchRequest {
    const formValues = this.searchForm.value;
    const table = this.table();

    // Calcul de la pagination
    const rows = table?.rows ?? this.pageSize();
    const first = table?.first ?? 0;
    const pageNumber = Math.floor(first / rows) + 1;

    // NETTOYAGE CRUCIAL DU PARENTID
    let cleanParentId = null;
    if (formValues.parentId) {
      // On extrait la clé si c'est un objet PrimeNG, sinon on garde la valeur
      cleanParentId = formValues.parentId.key ?? formValues.parentId.data ?? formValues.parentId;
    }

    return {
      ...formValues,
      parentId: cleanParentId,
      pageNumber: pageNumber,
      pageSize: rows
    };
  }

  //#endregion

}


