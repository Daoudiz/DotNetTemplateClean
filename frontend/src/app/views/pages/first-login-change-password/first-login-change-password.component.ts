import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import {
  ContainerComponent,
  RowComponent,
  ColComponent,
  CardComponent,
  CardBodyComponent,
  ButtonDirective,
  SpinnerComponent
} from '@coreui/angular';
import { BehaviorSubject } from 'rxjs';
import { AuthService } from '../../../services/user/auth.service';

@Component({
  selector: 'app-first-login-change-password',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ContainerComponent,
    RowComponent,
    ColComponent,
    CardComponent,
    CardBodyComponent,
    ButtonDirective,
    SpinnerComponent
  ],
  templateUrl: './first-login-change-password.component.html',
  styleUrls: ['./first-login-change-password.component.scss']
})
export class FirstLoginChangePasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly isLoading$ = new BehaviorSubject<boolean>(false);
  errorMessage: string | null = null;
  readonly pendingUserName = this.authService.pendingFirstLoginUserName();

  readonly form = this.fb.nonNullable.group({
    oldPassword: ['', [Validators.required]],
    newPassword: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', [Validators.required]]
  });

  constructor() {
    if (!this.pendingUserName) {
      this.authService.loginError.set('Veuillez vous reconnecter pour changer votre mot de passe.');
      this.router.navigate(['/login']);
    }
  }

  onSubmit(): void {
    if (this.isLoading$.value || this.form.invalid || !this.pendingUserName) {
      this.form.markAllAsTouched();
      return;
    }

    const { oldPassword, newPassword, confirmPassword } = this.form.getRawValue();

    if (newPassword !== confirmPassword) {
      this.errorMessage = 'La confirmation du mot de passe ne correspond pas.';
      return;
    }

    this.errorMessage = null;
    this.isLoading$.next(true);

    this.authService.completeFirstLoginPasswordChange({
      userName: this.pendingUserName,
      oldPassword,
      newPassword,
      confirmPassword
    }).subscribe({
      next: () => {
        this.isLoading$.next(false);
        this.router.navigate(['/dashboard']);
      },
      error: (error) => {
        this.isLoading$.next(false);
        this.errorMessage = error?.error?.detail || error?.error?.message || 'Erreur lors du changement de mot de passe.';
      }
    });
  }
}
