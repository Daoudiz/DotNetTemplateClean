import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { NotificationService } from '../../../services/notification.service';
import { UserService } from '../../../services/user/user.service';
import { ValidationService } from '../../../services/user/validation.service';
import { MustMatch } from '../../../validators/user-validators';
import { ApplicationUser, CreateUserViewModel, UpdateUserViewModel } from '../../../models/user/user-models';

@Component({
  selector: 'app-user-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user-create.component.html',
  styleUrls: ['./user-create.component.scss']
})
export class UserCreateComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly userService = inject(UserService);
  private readonly notification = inject(NotificationService);

  public valService = inject(ValidationService);
  public userForm!: FormGroup;
  public isEditMode = false;
  public errorMessage = signal<string>('');

  private _userToEdit: ApplicationUser | null = null;

  @Output() cancel = new EventEmitter<void>();
  @Output() userCreated = new EventEmitter<void>();

  @Input() set userToEdit(user: ApplicationUser | null) {
    this._userToEdit = user;

    if (this.userForm) {
      this.configureFormForMode(user);
    }
  }

  ngOnInit(): void {
    this.initForm();
    this.loadRoles();

    if (this._userToEdit) {
      this.configureFormForMode(this._userToEdit);
    }
  }

  readonly roles = signal<{ id: string; name: string }[]>([]);

  private initForm(): void {
    this.userForm = this.fb.group(
      {
        firstName: ['', Validators.required],
        lastName: ['', Validators.required],
        userRole: [null, Validators.required],
        userName: ['', Validators.required],
        email: ['', [Validators.required, Validators.email]],
        password: ['', [Validators.required, Validators.minLength(6)]],
        confirmPassword: ['', Validators.required],
        twoFactorEnabled: [false]
      },
      { validators: MustMatch('password', 'confirmPassword') }
    );
  }

  private loadRoles(): void {
    this.userService.getRoles().subscribe({
      next: (data) => this.roles.set(data)
    });
  }

  private configureFormForMode(user: ApplicationUser | null): void {
    this.userForm.reset({ twoFactorEnabled: false });

    if (!user) {
      this.isEditMode = false;
      this.togglePasswordValidators(true);
      return;
    }

    this.isEditMode = true;
    this.togglePasswordValidators(false);
    this.userForm.patchValue({
      firstName: user.firstName,
      lastName: user.lastName,
      userName: user.userName,
      email: user.email,
      userRole: user.roleId ?? null,
      twoFactorEnabled: false
    });
  }

  private togglePasswordValidators(isRequired: boolean): void {
    const password = this.userForm.get('password');
    const confirmPassword = this.userForm.get('confirmPassword');

    if (isRequired) {
      password?.setValidators([Validators.required, Validators.minLength(6)]);
      confirmPassword?.setValidators([Validators.required]);
    } else {
      password?.clearValidators();
      confirmPassword?.clearValidators();
    }

    password?.updateValueAndValidity();
    confirmPassword?.updateValueAndValidity();
  }

  onSubmit(): void {
    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      return;
    }

    if (this.isEditMode) {
      this.updateUser();
      return;
    }

    this.createUser();
  }

  private createUser(): void {
    const raw = this.userForm.getRawValue();

    const payload: CreateUserViewModel = {
      firstName: raw.firstName,
      lastName: raw.lastName,
      userRole: raw.userRole,
      email: raw.email,
      userName: raw.userName,
      password: raw.password,
      confirmPassword: raw.confirmPassword,
      twoFactorEnabled: !!raw.twoFactorEnabled
    };

    this.userService.createUser(payload).subscribe({
      next: () => {
        this.notification.success("L'utilisateur a ete cree avec succes.", 'Creation reussie');
        this.userCreated.emit();
        this.onCancel();
      }
    });
  }

  private updateUser(): void {
    if (!this._userToEdit) {
      return;
    }

    const raw = this.userForm.getRawValue();

    const payload: UpdateUserViewModel = {
      userId: this._userToEdit.id,
      firstName: raw.firstName,
      lastName: raw.lastName,
      userRole: raw.userRole,
      email: raw.email,
      userName: raw.userName
    };

    this.userService.updateUser(this._userToEdit.id, payload).subscribe({
      next: () => {
        this.notification.success('Utilisateur mis a jour avec succes.', 'Mise a jour reussie');
        this.userCreated.emit();
      }
    });
  }

  onCancel(): void {
    this.userForm.reset({ twoFactorEnabled: false });
    this.errorMessage.set('');
    this.cancel.emit();
  }
}
