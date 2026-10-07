import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CartService } from '../../services/cart.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <header class="navbar navbar-expand-lg navbar-dark bg-dark sticky-top shadow-sm px-3 py-2">
      <div class="container-fluid">
        <!-- Brand -->
        <a class="navbar-brand d-flex align-items-center gap-2 fw-bold" routerLink="/productos">
          <span class="fs-4 text-warning">⚡</span>
          <span class="tracking-tight text-white">FIRMEZA</span>
          <span class="badge bg-primary text-white rounded-pill px-2 py-1 small ms-1">Portal Clientes</span>
        </a>

        <!-- Mobile Toggle Button -->
        <button
          class="navbar-toggler border-0"
          type="button"
          (click)="toggleMenu()"
          [attr.aria-expanded]="isMenuOpen()"
          aria-label="Toggle navigation"
        >
          <span class="navbar-toggler-icon"></span>
        </button>

        <!-- Navbar Links & Session Area -->
        <div class="collapse navbar-collapse" [class.show]="isMenuOpen()" id="clientNavbar">
          <ul class="navbar-nav me-auto mb-2 mb-lg-0 gap-1 ms-lg-4">
            <li class="nav-item">
              <a
                routerLink="/productos"
                routerLinkActive="active"
                (click)="isMenuOpen.set(false)"
                class="nav-link px-3 rounded-pill text-light d-flex align-items-center gap-2"
              >
                <i class="bi bi-box-seam"></i>
                <span>Catálogo de Materiales</span>
              </a>
            </li>
            <li class="nav-item">
              <a
                routerLink="/carrito"
                routerLinkActive="active"
                (click)="isMenuOpen.set(false)"
                class="nav-link px-3 rounded-pill text-light d-flex align-items-center gap-2"
              >
                <i class="bi bi-cart3"></i>
                <span>Mi Carrito</span>
                @if (cartService.totalCount() > 0) {
                  <span class="badge bg-primary rounded-pill">{{ cartService.totalCount() }}</span>
                }
              </a>
            </li>
            <li class="nav-item">
              <a
                routerLink="/mis-pedidos"
                routerLinkActive="active"
                (click)="isMenuOpen.set(false)"
                class="nav-link px-3 rounded-pill text-light d-flex align-items-center gap-2"
              >
                <i class="bi bi-receipt"></i>
                <span>Mis Pedidos & Comprobantes</span>
              </a>
            </li>
          </ul>

          <!-- User Info & Session Options -->
          <div class="d-flex align-items-center gap-3 flex-wrap mt-2 mt-lg-0">
            @if (authService.currentUser(); as user) {
              <div class="user-pill d-flex align-items-center bg-secondary bg-opacity-25 rounded-pill px-3 py-1 border border-secondary border-opacity-25 gap-2 shadow-sm">
                <div class="avatar-sm rounded-circle bg-primary text-white d-flex align-items-center justify-content-center fw-bold" style="width: 32px; height: 32px; font-size: 0.85rem;">
                  {{ (user.email.charAt(0) || 'C').toUpperCase() }}
                </div>
                <div class="d-flex flex-column text-start">
                  <span class="small text-white fw-semibold lh-1">{{ user.email }}</span>
                  <span class="text-success small fw-bold lh-1 mt-1" style="font-size: 0.7rem;">● Cliente Autenticado</span>
                </div>
              </div>

              <button
                (click)="onLogout()"
                class="btn btn-outline-danger btn-sm rounded-pill px-3 d-flex align-items-center gap-2 shadow-sm"
                title="Cerrar sesión y eliminar token"
              >
                <i class="bi bi-box-arrow-right"></i>
                <span class="fw-semibold">Cerrar Sesión</span>
              </button>
            } @else {
              <a routerLink="/login" class="btn btn-outline-light btn-sm rounded-pill px-3">
                Iniciar Sesión
              </a>
              <a routerLink="/register" class="btn btn-primary btn-sm rounded-pill px-3">
                Registrarse
              </a>
            }
          </div>
        </div>
      </div>
    </header>
  `,
  styles: [`
    .navbar {
      background-color: #111827 !important;
      border-bottom: 1px solid rgba(255, 255, 255, 0.08);
    }
    .nav-link {
      color: #94a3b8 !important;
      font-weight: 500;
      transition: all 0.2s ease;
    }
    .nav-link:hover {
      color: #ffffff !important;
      background: rgba(255, 255, 255, 0.05);
    }
    .nav-link.active {
      color: #ffffff !important;
      background: #2563eb !important;
      font-weight: 600;
      box-shadow: 0 2px 8px rgba(37, 99, 235, 0.35);
    }
    .user-pill {
      background: rgba(255, 255, 255, 0.06) !important;
    }
  `]
})
export class HeaderComponent {
  readonly authService = inject(AuthService);
  readonly cartService = inject(CartService);
  readonly isMenuOpen = signal(false);

  toggleMenu(): void {
    this.isMenuOpen.update(open => !open);
  }

  onLogout(): void {
    this.isMenuOpen.set(false);
    this.authService.logout().subscribe();
  }
}
