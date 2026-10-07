import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { LoginDto } from '../../../models/auth.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="auth-wrapper d-flex align-items-center justify-content-center min-vh-100 p-3">
      <div class="card border-0 shadow-lg rounded-4 overflow-hidden auth-card" style="max-width: 440px; width: 100%;">
        <div class="card-header bg-dark text-white p-4 text-center border-0">
          <div class="d-inline-flex align-items-center justify-content-center bg-primary text-white rounded-circle mb-3 p-3 shadow" style="width: 54px; height: 54px;">
            <i class="bi bi-shield-lock fs-3"></i>
          </div>
          <h4 class="fw-bold mb-1">FIRMEZA</h4>
          <p class="text-muted small mb-0">Portal de Autenticación de Clientes</p>
        </div>

        <div class="card-body p-4 p-sm-5">
          <!-- Session Expired Notification -->
          @if (sessionExpired()) {
            <div class="alert alert-warning d-flex align-items-center rounded-3 p-3 mb-4" role="alert">
              <i class="bi bi-exclamation-triangle-fill flex-shrink-0 me-2 fs-5"></i>
              <div class="small">
                Tu sesión ha expirado o el token no es válido. Por favor, inicia sesión nuevamente.
              </div>
            </div>
          }

          <!-- General Error Message -->
          @if (errorMessage()) {
            <div class="alert alert-danger d-flex align-items-center rounded-3 p-3 mb-4" role="alert">
              <i class="bi bi-x-circle-fill flex-shrink-0 me-2 fs-5"></i>
              <div class="small">{{ errorMessage() }}</div>
            </div>
          }

          <form (ngSubmit)="onLogin()" #loginForm="ngForm">
            <div class="mb-3">
              <label for="email" class="form-label fw-semibold small text-secondary">Correo Electrónico</label>
              <div class="input-group">
                <span class="input-group-text bg-light border-end-0 text-muted">
                  <i class="bi bi-envelope"></i>
                </span>
                <input
                  type="email"
                  id="email"
                  name="email"
                  class="form-control bg-light border-start-0 py-2"
                  [(ngModel)]="credentials.email"
                  placeholder="ejemplo@cliente.com"
                  required
                  email
                  #emailModel="ngModel"
                />
              </div>
              @if (emailModel.invalid && emailModel.touched) {
                <div class="text-danger small mt-1">Ingrese un correo válido.</div>
              }
            </div>

            <div class="mb-3">
              <label for="password" class="form-label fw-semibold small text-secondary">Contraseña</label>
              <div class="input-group">
                <span class="input-group-text bg-light border-end-0 text-muted">
                  <i class="bi bi-lock"></i>
                </span>
                <input
                  [type]="showPassword ? 'text' : 'password'"
                  id="password"
                  name="password"
                  class="form-control bg-light border-start-0 border-end-0 py-2"
                  [(ngModel)]="credentials.password"
                  placeholder="••••••••"
                  required
                  #passwordModel="ngModel"
                />
                <button
                  type="button"
                  class="btn btn-light border border-start-0 text-muted"
                  (click)="showPassword = !showPassword"
                >
                  <i class="bi" [ngClass]="showPassword ? 'bi-eye-slash' : 'bi-eye'"></i>
                </button>
              </div>
              @if (passwordModel.invalid && passwordModel.touched) {
                <div class="text-danger small mt-1">La contraseña es obligatoria.</div>
              }
            </div>

            <div class="d-flex justify-content-between align-items-center mb-4">
              <div class="form-check">
                <input
                  class="form-check-input"
                  type="checkbox"
                  id="rememberMe"
                  name="rememberMe"
                  [(ngModel)]="credentials.rememberMe"
                />
                <label class="form-check-label small text-muted" for="rememberMe">
                  Recordarme
                </label>
              </div>
              <button
                type="button"
                class="btn btn-link p-0 text-decoration-none small fw-semibold"
                (click)="fillDemoCliente()"
              >
                Cargar Demo Cliente
              </button>
            </div>

            <button
              type="submit"
              class="btn btn-primary w-100 py-2 fw-semibold rounded-3 shadow-sm d-flex align-items-center justify-content-center gap-2"
              [disabled]="loginForm.invalid || isLoading()"
            >
              @if (isLoading()) {
                <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                <span>Iniciando sesión...</span>
              } @else {
                <i class="bi bi-box-arrow-in-right"></i>
                <span>Ingresar al Sistema</span>
              }
            </button>
          </form>

          <hr class="my-4 text-muted" />

          <div class="text-center small text-muted">
            ¿No tienes una cuenta de cliente?
            <a routerLink="/register" class="text-primary fw-semibold text-decoration-none ms-1">
              Regístrate aquí
            </a>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-wrapper {
      background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
      min-height: 100vh;
    }
    .auth-card {
      box-shadow: 0 20px 40px rgba(0, 0, 0, 0.25) !important;
      border: 1px solid rgba(255, 255, 255, 0.08);
    }
  `]
})
export class LoginComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  credentials: LoginDto = {
    email: '',
    password: '',
    rememberMe: true
  };

  showPassword = false;
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  sessionExpired = signal(false);
  returnUrl = '/productos';

  ngOnInit(): void {
    const reason = this.route.snapshot.queryParamMap.get('reason');
    if (reason === 'expired') {
      this.sessionExpired.set(true);
    }

    const ret = this.route.snapshot.queryParamMap.get('returnUrl');
    if (ret) {
      this.returnUrl = ret;
    }
  }

  fillDemoCliente(): void {
    this.credentials.email = 'cliente@firmeza.com';
    this.credentials.password = 'Cliente123*';
  }

  onLogin(): void {
    if (!this.credentials.email || !this.credentials.password) {
      this.errorMessage.set('Por favor ingrese correo y contraseña.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.sessionExpired.set(false);

    this.authService.login(this.credentials).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        this.router.navigateByUrl(this.returnUrl);
      },
      error: (err) => {
        this.isLoading.set(false);
        if (err.error?.mensaje) {
          this.errorMessage.set(err.error.mensaje);
        } else if (err.error?.errores && err.error.errores.length > 0) {
          this.errorMessage.set(err.error.errores.join(', '));
        } else if (err.status === 401) {
          this.errorMessage.set('Credenciales incorrectas. Verifique su correo o contraseña.');
        } else {
          this.errorMessage.set('No fue posible conectar con el servidor. Intente de nuevo.');
        }
      }
    });
  }
}
