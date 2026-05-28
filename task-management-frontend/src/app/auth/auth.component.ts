import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../core/services/auth.service';
import { LoginRequest, RegisterRequest } from '../core/models/auth.models';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './auth.component.html',
  styleUrl: './auth.component.css'
})
export class AuthComponent {
  isLoginMode = signal(true);
  loading = signal(false);
  error = signal<string | null>(null);

  // Form models
  loginForm: LoginRequest = { email: '', password: '' };
  registerForm: RegisterRequest = { email: '', password: '', fullName: '' };

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    // Redirect if already logged in
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/tasks']);
    }
  }

  toggleMode() {
    this.isLoginMode.update(v => !v);
    this.error.set(null);
    this.loginForm = { email: '', password: '' };
    this.registerForm = { email: '', password: '', fullName: '' };
  }

  onSubmit() {
    this.error.set(null);
    this.loading.set(true);

    const action$ = this.isLoginMode()
      ? this.authService.login(this.loginForm)
      : this.authService.register(this.registerForm);

    action$.subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/tasks']);
      },
      error: (err) => {
        this.loading.set(false);
        const message = err?.error?.title || err?.error?.error || 'Authentication failed. Please try again.';
        this.error.set(message);
      }
    });
  }
}
