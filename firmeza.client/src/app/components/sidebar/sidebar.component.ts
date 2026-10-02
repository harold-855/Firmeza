import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <aside class="sidebar bg-dark text-white d-flex flex-column flex-shrink-0 p-3">
      <div class="sidebar-brand d-flex align-items-center mb-3 mb-md-0 me-md-auto text-white text-decoration-none">
        <span class="fs-4 fw-bold tracking-tight text-primary-gradient">
          ⚡ FIRMEZA <span class="badge bg-primary fs-6">Admin</span>
        </span>
      </div>
      
      <hr class="border-secondary my-3" />
      
      <div class="user-pill d-flex align-items-center p-2 mb-3 rounded bg-dark-subtle text-light">
        <div class="avatar bg-primary text-white rounded-circle d-flex align-items-center justify-content-center me-2 fw-bold">
          A
        </div>
        <div class="user-info text-truncate">
          <div class="fw-semibold small">Administrador</div>
          <div class="text-muted text-truncate" style="font-size: 0.75rem;">admin&#64;firmeza.com</div>
        </div>
      </div>

      <ul class="nav nav-pills flex-column mb-auto gap-1">
        <li class="nav-item">
          <a routerLink="/dashboard" routerLinkActive="active" class="nav-link text-white d-flex align-items-center gap-2">
            <span class="nav-icon">📊</span>
            <span>Inicio / Dashboard</span>
          </a>
        </li>
        <li class="nav-item">
          <a routerLink="/productos" routerLinkActive="active" class="nav-link text-white d-flex align-items-center gap-2">
            <span class="nav-icon">📦</span>
            <span>Productos</span>
          </a>
        </li>
        <li class="nav-item">
          <a routerLink="/clientes" routerLinkActive="active" class="nav-link text-white d-flex align-items-center gap-2">
            <span class="nav-icon">👥</span>
            <span>Clientes</span>
          </a>
        </li>
        <li class="nav-item">
          <a routerLink="/ventas" routerLinkActive="active" class="nav-link text-white d-flex align-items-center gap-2">
            <span class="nav-icon">🛒</span>
            <span>Ventas</span>
          </a>
        </li>
      </ul>

      <hr class="border-secondary my-3" />

      <div class="sidebar-footer">
        <a href="/Account/Login" class="btn btn-outline-danger w-100 btn-sm d-flex align-items-center justify-content-center gap-2">
          <span>🚪</span>
          <span>Cerrar Sesión</span>
        </a>
      </div>
    </aside>
  `,
  styles: [`
    .sidebar {
      width: 260px;
      min-height: 100vh;
      background: #111827 !important;
      border-right: 1px solid rgba(255,255,255,0.08);
      box-shadow: 2px 0 10px rgba(0,0,0,0.15);
    }
    .text-primary-gradient {
      background: linear-gradient(135deg, #60a5fa 0%, #3b82f6 100%);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }
    .avatar {
      width: 36px;
      height: 36px;
      background: linear-gradient(135deg, #3b82f6, #1d4ed8) !important;
    }
    .nav-link {
      color: #9ca3af !important;
      border-radius: 8px;
      padding: 0.65rem 1rem;
      font-weight: 500;
      transition: all 0.2s ease;
    }
    .nav-link:hover {
      background: rgba(255, 255, 255, 0.06) !important;
      color: #f3f4f6 !important;
      transform: translateX(4px);
    }
    .nav-link.active {
      background: #2563eb !important;
      color: #ffffff !important;
      font-weight: 600;
      box-shadow: 0 4px 12px rgba(37, 99, 235, 0.35);
    }
    .nav-icon {
      font-size: 1.15rem;
    }
    .bg-dark-subtle {
      background: rgba(255,255,255,0.04) !important;
    }
  `]
})
export class SidebarComponent {}
