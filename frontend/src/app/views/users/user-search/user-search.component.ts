import { ChangeDetectionStrategy, Component, ElementRef, OnInit, ViewChild, inject, signal } from '@angular/core';
import { CommonModule, UpperCasePipe } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Observable, Subject, merge, of } from 'rxjs';
import { catchError, finalize, map, switchMap, tap } from 'rxjs/operators';
import { Table, TableLazyLoadEvent, TableModule } from 'primeng/table';

import { UserCreateComponent } from '../user-create/user-create.component';
import { NotificationService } from '../../../services/notification.service';
import { UserService } from '../../../services/user/user.service';
import { ApplicationUser, UserSearchCriteria } from '../../../models/user/user-models';

import {
  AlertModule,
  ButtonModule,
  CardModule,
  FormModule,
  GridModule,
  ModalModule,
  SpinnerModule,
  TooltipModule,
  UtilitiesModule
} from '@coreui/angular';
import { IconModule } from '@coreui/icons-angular';

@Component({
  selector: 'app-user-search',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    UserCreateComponent,
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
    ModalModule,
    UpperCasePipe
  ],
  templateUrl: './user-search.component.html',
  styleUrls: ['./user-search.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserSearchComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly userService = inject(UserService);
  private readonly notification = inject(NotificationService);

  @ViewChild('dt') table?: Table;
  @ViewChild('globalSearchInput') globalSearchInput!: ElementRef;

  readonly loading = signal<boolean>(false);
  readonly hasSearched = signal<boolean>(false);
  readonly totalRecords = signal<number>(0);
  readonly isLazyMode = signal<boolean>(true);
  readonly selectedUser = signal<ApplicationUser | null>(null);
  readonly isEditMode = signal<boolean>(false);
  readonly isUserModalOpen = signal<boolean>(false);

  private readonly searchAction$ = new Subject<UserSearchCriteria>();
  private readonly resetAction$ = new Subject<void>();

  searchForm!: FormGroup;

  readonly users$: Observable<ApplicationUser[]> = merge(
    this.searchAction$.pipe(
      tap(() => this.loading.set(true)),
      switchMap((criteria) =>
        this.userService.searchUsers(criteria).pipe(
          tap((res) => {
            this.totalRecords.set(res.totalCount);
            this.isLazyMode.set(!res.isFullResult);
          }),
          map((res) => res.items),
          catchError(() => {
            this.totalRecords.set(0);
            return of([]);
          }),
          finalize(() => this.loading.set(false))
        )
      )
    ),
    this.resetAction$.pipe(
      tap(() => {
        this.totalRecords.set(0);
        this.loading.set(false);
        this.isLazyMode.set(true);
      }),
      map(() => [])
    )
  );

  ngOnInit(): void {
    this.initForm();
  }

  private initForm(): void {
    this.searchForm = this.fb.group({
      nom: [''],
      prenom: ['']
    });
  }

  onSearch(): void {
    this.hasSearched.set(true);

    if (this.table) {
      if (this.table.first !== 0) {
        this.table.reset();
      } else {
        this.loadUsersLazy({
          first: 0,
          rows: this.table.rows || 10
        });
      }
      return;
    }

    this.searchAction$.next({ ...this.searchForm.value, pageNumber: 1, pageSize: 10 });
  }

  onReset(): void {
    this.searchForm.reset({ nom: '', prenom: '' });
    this.hasSearched.set(false);
    this.isLazyMode.set(true);

    if (this.globalSearchInput) {
      this.globalSearchInput.nativeElement.value = '';
    }

    if (!this.table) {
      this.resetAction$.next();
      return;
    }

    this.table.first = 0;
    this.table.filters = {};

    setTimeout(() => {
      if (this.table) {
        this.table.clear();
        this.resetAction$.next();
      }
    }, 0);
  }

  loadUsersLazy(event: TableLazyLoadEvent): void {
    if (!this.hasSearched()) {
      return;
    }

    const first = event.first ?? 0;
    const rows = event.rows ?? this.table?.rows ?? 10;

    if (!this.isLazyMode() && first !== 0) {
      return;
    }

    const pageNumber = first / rows + 1;

    const criteria: UserSearchCriteria = {
      ...this.searchForm.value,
      pageNumber,
      pageSize: rows
    };

    this.searchAction$.next(criteria);
  }

  onToggleUserStatus(user: ApplicationUser): void {
    this.userService.deleteUser(user.id).subscribe({
      next: () => {
        user.isLocked = !user.isLocked;
        this.notification.success('Statut mis a jour');
      }
    });
  }

  closeUserModal(): void {
    this.isUserModalOpen.set(false);
    if (!this.isUserModalOpen()) {
      this.selectedUser.set(null);
    }
  }

  onModalVisibilityChange(event: boolean): void {
    if (this.isUserModalOpen() !== event) {
      this.isUserModalOpen.set(event);

      if (!event) {
        this.selectedUser.set(null);
      }
    }
  }

  openCreateModal(): void {
    this.selectedUser.set(null);
    this.isEditMode.set(false);
    this.isUserModalOpen.set(true);
  }

  openEditModal(user: ApplicationUser): void {
    this.selectedUser.set(user);
    this.isEditMode.set(true);
    this.isUserModalOpen.set(true);
  }

  onUserCreatedSuccess(): void {
    this.isUserModalOpen.set(false);
    this.onSearch();
  }
}
