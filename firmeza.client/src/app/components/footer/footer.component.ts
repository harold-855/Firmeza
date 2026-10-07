import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule],
  template: `
    <footer class="bg-white border-top py-3 px-4 mt-auto">
      <div class="container-fluid d-flex flex-column flex-sm-row justify-content-between align-items-center gap-2 small text-muted">
        <div>
          <strong>Firmeza S.A.S.</strong> &copy; 2026. Portal de Clientes y Gestión de Despachos.
        </div>
        <div class="d-flex align-items-center gap-3">
          <span class="badge bg-success-subtle text-success border border-success-subtle rounded-pill px-3 py-1">
            🔒 Conexión Segura JWT
          </span>
          <span>v1.0.0</span>
        </div>
      </div>
    </footer>
  `
})
export class FooterComponent {}
