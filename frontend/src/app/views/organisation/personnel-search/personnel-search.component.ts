/******************************************************************************************
 * @file personnel-search.component.ts
 * @description Module de recherche des personnels.
 * Utilise PrimeNG Table en lazy loading avec filtres simples et Signals.
 * @module Gestion des personnels
 * @author Zakaria DAOUDI
 * @copyright © 2026 - Hope
 *******************************************************************************************/
import { ChangeDetectionStrategy, Component, DestroyRef, ElementRef, OnInit, inject, signal, viewChild } from '@angular/core';
import { CommonModule, DatePipe, UpperCasePipe } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Observable, Subject, merge, of } from 'rxjs';
import { catchError, finalize, map, switchMap, tap } from 'rxjs/operators';

import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { Table, TableLazyLoadEvent, TableModule } from 'primeng/table';
import { TreeSelectModule } from 'primeng/treeselect';
import { TooltipModule as PrimeTooltipModule } from 'primeng/tooltip';

import {
  AlertModule,
  ButtonModule,
  CardModule,
  FormModule,
  GridModule,
  ModalModule,
  SpinnerModule,
  UtilitiesModule
} from '@coreui/angular';
import { IconModule } from '@coreui/icons-angular';

import { OrganizationService } from '../../../services/organisation/organisation.service';
import { PersonnelService } from '../../../services/organisation/personnel.service';
import { NotificationService } from '../../../services/notification.service';
import { PersonnelCreateComponent } from '../personnel-create/personnel-create.component';
import { GetPersonnelsWithFiltersQuery, PersonnelListDto } from '../../../models/organisation/personnel.model';
import { OrganizationTreeNode } from '../../../models/organisation/organisation-model';

@Component({
  selector: 'app-personnel-search',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    GridModule,
    FormModule,
    ButtonModule,
    TableModule,
    SpinnerModule,
    PrimeTooltipModule,
    UtilitiesModule,
    IconModule,
    AlertModule,
    ModalModule,
    TreeSelectModule,
    ConfirmDialogModule,
    PersonnelCreateComponent,
    UpperCasePipe,
    DatePipe
  ],
  templateUrl: './personnel-search.component.html',
  styleUrl: './personnel-search.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PersonnelSearchComponent implements OnInit {

  //#region --- INJECTIONS ET REFERENCES ---
  private readonly fb = inject(FormBuilder);
  private readonly personnelService = inject(PersonnelService);
  private readonly organisationService = inject(OrganizationService);
  private readonly notification = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly confirmationService = inject(ConfirmationService);

  readonly table = viewChild<Table>('dt');
  readonly globalSearchInput = viewChild<ElementRef>('globalSearchInput');
  //#endregion

  //#region --- SIGNALS D'ETAT (UI & DONNEES) ---
  readonly nodes = signal<OrganizationTreeNode[]>([]);
  readonly loading = signal<boolean>(false);
  readonly hasSearched = signal<boolean>(false);
  readonly totalRecords = signal<number>(0);
  readonly pageSize = signal<number>(10);
  readonly tooltipPersonnel = signal<PersonnelListDto | null>(null);
  readonly selectedPersonnel = signal<PersonnelListDto | null>(null);
  readonly isEditMode = signal<boolean>(false);
  readonly isPersonnelModalOpen = signal<boolean>(false);

  searchForm!: FormGroup;
  //#endregion

  //#region --- LOGIQUE DE RECHERCHE (RxJS) ---
  private readonly searchAction$ = new Subject<GetPersonnelsWithFiltersQuery>();
  private readonly resetAction$ = new Subject<void>();

  readonly personnels$: Observable<PersonnelListDto[]> = merge(
    this.searchAction$.pipe(
      tap(() => this.loading.set(true)),
      switchMap((criteria) => this.personnelService.getPersonnel(criteria).pipe(
        tap((response) => {
          this.totalRecords.set(response.totalCount);
        }),
        map((response) => response.items),
        catchError(() => {
          this.totalRecords.set(0);
          return of([]);
        }),
        finalize(() => this.loading.set(false))
      ))
    ),
    this.resetAction$.pipe(
      tap(() => {
        this.totalRecords.set(0);
        this.loading.set(false);
      }),
      map(() => [])
    )
  );
  //#endregion

  //#region --- ACTIONS UTILISATEUR (RECHERCHE & RESET) ---
  onSearch(): void {
    this.hasSearched.set(true);

    this.table()?.reset();
    //this.searchAction$.next(this.getParams());
  }

  onReset(): void {
    this.searchForm.reset({
      searchTerm: '',
      entiteId: null
    });
    this.hasSearched.set(false);

    const input = this.globalSearchInput();
    if (input) {
      input.nativeElement.value = '';
    }

    const resultTable = this.table();
    if (resultTable) {
      resultTable.first = 0;
      resultTable.filters = {};

      setTimeout(() => {
        this.table()?.clear();
        this.resetAction$.next();
      }, 0);
    } else {
      this.resetAction$.next();
    }
  }

  loadPersonnelLazy(event: TableLazyLoadEvent): void {
    if (!this.hasSearched()) {
      return;
    }

    const rows = event.rows ?? this.pageSize();
    this.pageSize.set(rows);

    this.searchAction$.next(this.getParams());
  }
  //#endregion

  //#region --- CYCLE DE VIE & INIT ---
  ngOnInit(): void {
    this.initForm();
    this.loadInitialData();
  }

  private initForm(): void {
    this.searchForm = this.fb.group({
      searchTerm: [''],
      entiteId: [null]
    });
  }

  private loadInitialData(): void {
    this.organisationService.getOrganizationTree()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.nodes.set(this.organisationService.addKeysToTree(data));
        }
      });
  }
  //#endregion

  //#region --- HELPERS & UTILITAIRES ---
  private getParams(): GetPersonnelsWithFiltersQuery {
    const formValues = this.searchForm.getRawValue();
    const table = this.table();
    const rows = table?.rows ?? this.pageSize();
    const first = table?.first ?? 0;

    let cleanEntiteId: number | null = null;
    if (formValues.entiteId) {
      cleanEntiteId = formValues.entiteId.key ?? formValues.entiteId.data ?? formValues.entiteId;
    }

    return {
      searchTerm: formValues.searchTerm || undefined,
      entiteId: cleanEntiteId,
      pageNumber: Math.floor(first / rows) + 1,
      pageSize: rows
    };
  }

  getAffectationLabels(personnel: PersonnelListDto): string[] {
    return personnel.affectations
      .map((affectation) => {
        const parts = [affectation.entiteLibelle, affectation.fonctionLibelle].filter(Boolean);
        return parts.join(' - ');
      })
      .filter((label) => label.length > 0);
  }

  setTooltipPersonnel(personnel: PersonnelListDto): void {
    this.tooltipPersonnel.set(personnel);
  }

  getAffectationSummary(personnel: PersonnelListDto): string {
    const labels = this.getAffectationLabels(personnel);

    if (labels.length === 0) {
      return 'Aucune affectation';
    }

    if (labels.length === 1) {
      return labels[0];
    }

    return `${labels[0]} (+${labels.length - 1})`;
  }
  //#endregion

  //#region --- GESTION DES MODALES (Personnel) ---
  openCreateModal(): void {
    this.isEditMode.set(false);
    this.selectedPersonnel.set(null);
    this.isPersonnelModalOpen.set(true);
  }

  openEditModal(personnel: PersonnelListDto): void {
    this.isEditMode.set(true);
    this.selectedPersonnel.set(personnel);
    this.isPersonnelModalOpen.set(true);
  }

  closePersonnelModal(): void {
    this.isPersonnelModalOpen.set(false);
    if (!this.isPersonnelModalOpen()) {
      this.selectedPersonnel.set(null);
    }
  }

  onPersonnelModalVisibilityChange(visible: boolean): void {
    if (this.isPersonnelModalOpen() !== visible) {
      this.isPersonnelModalOpen.set(visible);
      if (!visible) {
        this.selectedPersonnel.set(null);
      }
    }
  }

  onPersonnelSavedSuccess(): void {
    this.isPersonnelModalOpen.set(false);
    this.onSearch();
  }

  onDeletePersonnel(personnel: PersonnelListDto): void {
    this.confirmationService.confirm({
      message: `Etes-vous sur de vouloir supprimer le personnel "<strong>${personnel.nom} ${personnel.prenom}</strong>" ?`,
      header: 'Confirmation de suppression',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Supprimer',
      rejectLabel: 'Annuler',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.executeDelete(personnel.id);
      }
    });
  }

  private executeDelete(id: number): void {
    this.loading.set(true);

    this.personnelService.deletePersonnel(id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false))
      )
      .subscribe(() => {
        this.notification.success('Personnel supprime avec succes');
        this.onSearch();
      });
  }
  //#endregion
}
