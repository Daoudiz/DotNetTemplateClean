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

import { Table, TableLazyLoadEvent, TableModule } from 'primeng/table';
import { TreeSelectModule } from 'primeng/treeselect';

import {
  AlertModule,
  ButtonModule,
  CardModule,
  FormModule,
  GridModule,
  SpinnerModule,
  TooltipModule,
  UtilitiesModule
} from '@coreui/angular';
import { IconModule } from '@coreui/icons-angular';

import { OrganizationService } from '../../../services/organisation/organisation.service';
import { PersonnelService } from '../../../services/organisation/personnel.service';
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
    TooltipModule,
    UtilitiesModule,
    IconModule,
    AlertModule,
    TreeSelectModule,
    UpperCasePipe,
    DatePipe
  ],
  templateUrl: './personnel-search.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PersonnelSearchComponent implements OnInit {

  //#region --- INJECTIONS ET REFERENCES ---
  private readonly fb = inject(FormBuilder);
  private readonly personnelService = inject(PersonnelService);
  private readonly organisationService = inject(OrganizationService);
  private readonly destroyRef = inject(DestroyRef);

  readonly table = viewChild<Table>('dt');
  readonly globalSearchInput = viewChild<ElementRef>('globalSearchInput');
  //#endregion

  //#region --- SIGNALS D'ETAT (UI & DONNEES) ---
  readonly nodes = signal<OrganizationTreeNode[]>([]);
  readonly loading = signal<boolean>(false);
  readonly hasSearched = signal<boolean>(false);
  readonly totalRecords = signal<number>(0);
  readonly pageSize = signal<number>(10);

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

    console.log('Params:', this.getParams()); 

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
  //#endregion
}
