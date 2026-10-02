import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule],
  template: `
    <footer class="app-footer bg-white border-top px-4 py-3 d-flex justify-content-between align-items-center flex-wrap gap-2 text-muted small mt-auto">
      <div>
        &copy; 2026 <strong class="text-dark">Firmeza</strong> - Gestión y Despacho de Materiales Pesados.
      </div>
      <div class="d-flex align-items-center gap-3">
        <span class="badge bg-light text-secondary border">Angular 22 SPA</span>
        <a href="/Home/Privacy" class="text-decoration-none text-muted">Privacidad</a>
      </div>
    </footer>
  `
})
export class FooterComponent {}
