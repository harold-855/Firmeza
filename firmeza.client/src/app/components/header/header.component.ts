import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule],
  template: `
    <header class="app-header bg-white border-bottom px-4 py-2 d-flex justify-content-between align-items-center shadow-sm sticky-top">
      <div class="d-flex align-items-center gap-3">
        <span class="badge bg-primary-subtle text-primary px-3 py-2 rounded-pill fw-semibold d-none d-md-inline-block">
          ⚡ Panel de Control SPA
        </span>
        <span class="text-muted small d-none d-lg-inline-block">Sistema Comercial & Despachos</span>
      </div>

      <div class="d-flex align-items-center gap-3">
        <span class="badge bg-success-subtle text-success border border-success-subtle rounded-pill px-3 py-2 d-flex align-items-center gap-1">
          <span class="status-dot"></span>
          <span>API Conectada</span>
        </span>

        <div class="user-pill d-flex align-items-center bg-light rounded-pill px-3 py-1 border gap-2">
          <div class="avatar-sm rounded-circle bg-primary text-white d-flex align-items-center justify-content-center fw-bold" style="width: 28px; height: 28px; font-size: 0.8rem;">
            A
          </div>
          <span class="small fw-semibold text-dark">admin&#64;firmeza.com</span>
        </div>
      </div>
    </header>
  `,
  styles: [`
    .app-header {
      z-index: 1020;
    }
    .status-dot {
      width: 7px;
      height: 7px;
      background-color: #10b981;
      border-radius: 50%;
      display: inline-block;
    }
  `]
})
export class HeaderComponent {}
