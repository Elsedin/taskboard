import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  email = '';
  password = '';
  errorMessage = signal('');
  successMessage = signal('');
  isLoading = signal(false);

  constructor() {
    const state = this.router.getCurrentNavigation()?.extras.state as
      | { registered?: boolean }
      | undefined;

    if (state?.registered) {
      this.successMessage.set('Account created. Please sign in.');
    }
  }

  onSubmit(): void {
    this.errorMessage.set('');
    this.successMessage.set('');
    this.isLoading.set(true);

    this.auth.login({ email: this.email, password: this.password }).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/tasks']);
      },

      error: () => {
        this.isLoading.set(false);
        this.errorMessage.set(
          'Login failed. Check your email/password and that the API is running.',
        );
      },
    });
  }
}
