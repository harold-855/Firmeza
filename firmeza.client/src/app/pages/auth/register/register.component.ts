import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { RegisterDto } from '../../../models/auth.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="auth-wrapper d-flex align-items-center justify-content-center min-vh-100 p-3">
      <div class="card border-0 shadow-lg rounded-4 overflow-hidden auth-card" style="max-width: 480px; width: 100%;">
        <div class="card-header bg-dark text-white p-4 text-center border-0">
          <div class="d-inline-flex align-items-center justify-content-center bg-success text-white rounded-circle mb-3 p-3 shadow" style="width: 54px; height: 54px;">
            <i class="bi bi-person-plus-fill fs-3"></i>
          </div>
          <h4 class="fw-bold mb-1">Registro de Cliente</h4>
          <p class="text-muted small mb-0">Crea tu cuenta para comprar y gestionar tus pedidos</p>
        </div>

        <div class="card-body p-4 p-sm-5">
          <!-- General Error Message -->
          @if (errorMessage()) {
            <div class="alert alert-danger d-flex align-items-center rounded-3 p-3 mb-4" role="alert">
              <i class="bi bi-x-circle-fill flex-shrink-0 me-2 fs-5"></i>
              <div class="small">{{ errorMessage() }}</div>
            </div>
          }

          <form (ngSubmit)="onRegister()" #registerForm="ngForm">
            <div class="mb-3">
              <label for="email" class="form-label fw-semibold small text-secondary">Correo Electrónico *</label>
              <div class="input-group">
                <span class="input-group-text bg-light border-end-0 text-muted">
                  <i class="bi bi-envelope"></i>
                </span>
                <input
                  type="email"
                  id="email"
                  name="email"
                  class="form-control bg-light border-start-0 py-2"
                  [(ngModel)]="form.email"
                  placeholder="cliente@ejemplo.com"
                  required
                  email
                  #emailModel="ngModel"
                />
              </div>
              @if (emailModel.invalid && emailModel.touched) {
                <div class="text-danger small mt-1">Ingrese un correo electrónico válido.</div>
              }
            </div>

            <div class="mb-3">
              <label for="edad" class="form-label fw-semibold small text-secondary">Edad (Mayor de 18) *</label>
              <div class="input-group">
                <span class="input-group-text bg-light border-end-0 text-muted">
                  <i class="bi bi-calendar3"></i>
                </span>
                <input
                  type="number"
                  id="edad"
                  name="edad"
                  min="18"
                  max="120"
                  class="form-control bg-light border-start-0 py-2"
                  [(ngModel)]="form.edad"
                  placeholder="Ej: 25"
                  required
                  #edadModel="ngModel"
                />
              </div>
              @if (edadModel.invalid && edadModel.touched) {
                <div class="text-danger small mt-1">Debe ingresar una edad válida (mínimo 18 años).</div>
              }
            </div>

            <div class="mb-3">
              <label for="password" class="form-label fw-semibold small text-secondary">Contraseña *</label>
              <div class="input-group">
                <span class="input-group-text bg-light border-end-0 text-muted">
                  <i class="bi bi-lock"></i>
                </span>
                <input
                  [type]="showPassword ? 'text' : 'password'"
                  id="password"
                  name="password"
                  class="form-control bg-light border-start-0 border-end-0 py-2"
                  [(ngModel)]="form.password"
                  placeholder="Mínimo 6 caracteres"
                  required
                  minlength="6"
                  #passModel="ngModel"
                />
                <button
                  type="button"
                  class="btn btn-light border border-start-0 text-muted"
                  (click)="showPassword = !showPassword"
                >
                  <i class="bi" [ngClass]="showPassword ? 'bi-eye-slash' : 'bi-eye'"></i>
                </button>
              </div>
              @if (passModel.invalid && passModel.touched) {
                <div class="text-danger small mt-1">La contraseña debe tener al menos 6 caracteres.</div>
              }
            </div>

            <div class="mb-4">
              <label for="confirmPassword" class="form-label fw-semibold small text-secondary">Confirmar Contraseña *</label>
              <div class="input-group">
                <span class="input-group-text bg-light border-end-0 text-muted">
                  <i class="bi bi-check2-circle"></i>
                </span>
                <input
                  [type]="showPassword ? 'text' : 'password'"
                  id="confirmPassword"
                  name="confirmPassword"
                  class="form-control bg-light border-start-0 py-2"
                  [(ngModel)]="form.confirmPassword"
                  placeholder="Repita su contraseña"
                  required
                />
              </div>
              @if (form.password && form.confirmPassword && form.password !== form.confirmPassword) {
                <div class="text-danger small mt-1">Las contraseñas no coinciden.</div>
              }
            </div>

            <div class="alert alert-info py-2 px-3 rounded-3 small mb-4">
              <i class="bi bi-info-circle me-1"></i>
              Su cuenta se registrará automáticamente con el rol <strong>Cliente</strong> para compras y despachos.
            </div>

            <button
              type="submit"
              class="btn btn-success w-100 py-2 fw-semibold rounded-3 shadow-sm d-flex align-items-center justify-content-center gap-2"
              [disabled]="registerForm.invalid || form.password !== form.confirmPassword || isLoading()"
            >
              @if (isLoading()) {
                <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                <span>Registrando...</span>
              } @else {
                <i class="bi bi-check-lg"></i>
                <span>Completar Registro</span>
              }
            </button>
          </form>

          <hr class="my-4 text-muted" />

          <div class="text-center small text-muted">
            ¿Ya tienes una cuenta registrada?
            <a routerLink="/login" class="text-primary fw-semibold text-decoration-none ms-1">
              Iniciar sesión
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
export class RegisterComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  form: RegisterDto = {
    email: '',
    edad: '',
    password: '',
    confirmPassword: '',
    role: 'Cliente'
  };

  showPassword = false;
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  onRegister(): void {
    if (this.form.password !== this.form.confirmPassword) {
      this.errorMessage.set('Las contraseñas no coinciden.');
      return;
    }

    const ageNum = parseInt(this.form.edad, 10);
    if (isNaN(ageNum) || ageNum < 18) {
      this.errorMessage.set('Debe ser mayor de 18 años para registrarse.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.authService.register(this.form).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/productos']);
      },
      error: (err) => {
        this.isLoading.set(false);
        if (err.error?.mensaje) {
          this.errorMessage.set(err.error.mensaje);
        } else if (err.error?.errores && err.error.errores.length > 0) {
          this.errorMessage.set(err.error.errores.join(', '));
        } else {
          this.errorMessage.set('Ocurrió un error al procesar el registro. Intente nuevamente.');
        }
      }
    });
  }
}
