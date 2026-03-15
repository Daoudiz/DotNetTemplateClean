import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProfileService } from '../../services/user/profile.service';
import { UserProfile } from '../../models/user/auth.model';
import { ChangePasswordDto } from '../../models/user/auth.model';
import { CardModule, GridModule, SpinnerModule, AlertModule } from '@coreui/angular';
import { FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, CardModule, GridModule, SpinnerModule, AlertModule, ReactiveFormsModule],
  templateUrl: './profile.component.html'
})
export class ProfileComponent implements OnInit {
  private profileService = inject(ProfileService);
  private fb = inject(FormBuilder);
  private notification = inject(NotificationService);

  public profile = signal<UserProfile | null>(null);
  public isLoading = signal(true);
  showModal = false; // Pour afficher/masquer la modale
 
  ngOnInit(): void {
    this.profileService.getProfile().subscribe({
      next: (data) => {
     
        this.profile.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);        
      }
    });
  }

  passwordForm = this.fb.group({
    currentPassword: ['', [Validators.required]],
    newPassword: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', [Validators.required]]
  }, { validators: this.passwordMatchValidator });

  // Custom validator pour comparer les mots de passe
  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const newPw = control.get('newPassword')?.value;
    const confirmPw = control.get('confirmPassword')?.value;
    return newPw === confirmPw ? null : { passwordMismatch: true };
  }

  // Dans ton composant.ts
  onSubmit() {
    if (this.passwordForm.valid) {
      const val = this.passwordForm.value;

      // On prépare l'objet exactement comme le Back l'attend
      const payload: ChangePasswordDto = {
        OldPassword: val.currentPassword!,
        NewPassword: val.newPassword!,
        ConfirmPassword: val.confirmPassword!
      };

      this.profileService.changePassword(payload).subscribe({
        next: () => {
          this.notification.success(
            "Mot de passe modifié avec succès.",
            'Modification réussie'
          );
          this.closeModal();
        },
        error: (err) => {
         
        }
      });
    }
  }

  closeModal() {
    this.showModal = false;
    this.passwordForm.reset();
  }
}