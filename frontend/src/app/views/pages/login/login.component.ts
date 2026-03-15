import { Component, inject , ChangeDetectorRef } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BehaviorSubject } from 'rxjs';
import {
  ContainerComponent, RowComponent, ColComponent, CardGroupComponent,
  CardComponent, CardBodyComponent, InputGroupComponent, InputGroupTextDirective,
  FormControlDirective, ButtonDirective, SpinnerComponent, AlertComponent
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
    RouterLink,
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
 
  // Utilisation de l'interface modèle
  public loginData: LoginRequest = {
    userName: '',
    password: ''
  };
    
  public isLoading$ = new BehaviorSubject<boolean>(false);
 
  onLogin(): void {

    if (this.isLoading$.value) return;    
        
    this.isLoading$.next(true);
   
    this.authService.login(this.loginData).subscribe({
      next: (response) => {
        //this.isLoading = false; // Arrêt du chargement en cas de succès
        this.isLoading$.next(false);        
        this.router.navigate(['/dashboard']);        
      },
      
      error: (err) => {              
        this.isLoading$.next(false); // Arrêt du chargement en cas d'erreur    
        // On récupère le message du backend ou un message par défaut
        if (err.status === 401) {
        const errorMessage = err.error?.message || 'Identifiants ou mot de passe incorrects';
        this.authService.loginError.set(errorMessage);    
        } 
      }      
    });
  } 
}