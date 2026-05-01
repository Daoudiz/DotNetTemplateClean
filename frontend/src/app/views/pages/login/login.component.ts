import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BehaviorSubject } from 'rxjs';
import {
  ContainerComponent, RowComponent, ColComponent, CardGroupComponent,
  CardComponent, CardBodyComponent, InputGroupComponent, InputGroupTextDirective,
  FormControlDirective, ButtonDirective, SpinnerComponent
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';
import { AuthService } from '../../../services/user/auth.service';
import { LoginRequest } from '../../../models/user/auth.model';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  standalone: true,
  imports: [
    FormsModule,
    ContainerComponent,
    RowComponent,
    ColComponent,
    CardGroupComponent,
    CardComponent,
    CardBodyComponent,
    InputGroupComponent,
    InputGroupTextDirective,
    FormControlDirective,
    ButtonDirective,
    IconDirective,
    SpinnerComponent,
    AsyncPipe
  ]
})
export class LoginComponent {
  public authService = inject(AuthService);
  private router = inject(Router);

  public loginData: LoginRequest = {
    userName: '',
    password: ''
  };

  public isLoading$ = new BehaviorSubject<boolean>(false);

  constructor() {
    this.authService.loginError.set(null);
  }

  onLogin(): void {
    if (this.isLoading$.value) return;

    this.isLoading$.next(true);

    this.authService.login(this.loginData).subscribe({
      next: (response) => {
        this.isLoading$.next(false);

        if (response.passwordChangeRequired) {
          this.authService.loginError.set('Vous devez changer votre mot de passe lors de la première connexion.');
          this.router.navigate(['/first-login-change-password']);
          return;
        }

        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.isLoading$.next(false);
        if (err.status === 401) {
          const errorMessage = err.error?.message || 'Identifiants ou mot de passe incorrects';
          this.authService.loginError.set(errorMessage);
        }
      }
    });
  }
}
