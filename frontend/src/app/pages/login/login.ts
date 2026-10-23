import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  email = '';
  password = '';
  errorMessage = signal('');
  isLoading = signal(false);

  onSubmit(): void {
    this.errorMessage.set('');
    this.isLoading.set(true);

    this.auth.login({email:this.email, password:this.password}).subscribe({
      next: () => {
        this.isLoading.set(false);
        console.log('Login OK, token:', this.auth.getToken());
        alert('Login uspješan! Token je u localStorage.');
      },

      error: () => {
        this.isLoading.set(false);
        this.errorMessage.set(
          'Prijava nije uspjela. Provjeri email/lozinku i da li API radi.',
        );
      },
    });
  }
}
