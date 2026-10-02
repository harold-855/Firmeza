import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DashboardService } from '../../services/dashboard.service';
import { Producto } from '../../models/dashboard.model';

@Component({
  selector: 'app-productos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="p-4" style="background-color: #f8fafc; min-height: 100vh;">
      <div class="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-2">
        <div>
          <h2 class="h3 fw-bold text-dark mb-1">Catálogo de Productos</h2>
          <p class="text-muted mb-0">Gestión de inventario de materiales de construcción.</p>
        </div>
      </div>

      <div class="card border-0 shadow-sm rounded-4 bg-white">
        <div class="card-body p-4">
          <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">
            <div class="input-group" style="max-width: 320px;">
              <span class="input-group-text bg-light border-end-0">🔍</span>
              <input type="text" [(ngModel)]="searchTerm" placeholder="Buscar por nombre o descripción..." class="form-control bg-light border-start-0" />
            </div>
            <span class="text-muted small">Mostrando {{ filteredProductos().length }} productos</span>
          </div>

          <div class="table-responsive">
            <table class="table table-hover align-middle mb-0">
              <thead class="table-light small text-muted">
                <tr>
                  <th>Nombre</th>
                  <th>Descripción</th>
                  <th>Unidad</th>
                  <th>Precio Unitario</th>
                  <th class="text-center">Stock</th>
                  <th class="text-center">Estado</th>
                </tr>
              </thead>
              <tbody>
                @for (prod of filteredProductos(); track prod.id) {
                  <tr>
                    <td class="fw-bold text-dark">{{ prod.nombre }}</td>
                    <td class="text-muted small">{{ prod.descripcion }}</td>
                    <td><span class="badge bg-light text-dark border">{{ prod.unidadMedida }}</span></td>
                    <td class="fw-semibold text-primary">{{ prod.precioUnitario | currency:'COP':'symbol-narrow':'1.0-0' }}</td>
                    <td class="text-center">
                      <span class="badge" [ngClass]="prod.stockActual > 30 ? 'bg-success-subtle text-success' : 'bg-danger-subtle text-danger'">
                        {{ prod.stockActual }}
                      </span>
                    </td>
                    <td class="text-center">
                      <span class="badge" [ngClass]="prod.activo ? 'bg-success' : 'bg-secondary'">
                        {{ prod.activo ? 'Activo' : 'Inactivo' }}
                      </span>
                    </td>
                  </tr>
                } @empty {
                  <tr>
                    <td colspan="6" class="text-center text-muted py-4">No se encontraron productos.</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  `
})
export class ProductosComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  productos = signal<Producto[]>([]);
  searchTerm = '';

  ngOnInit(): void {
    this.dashboardService.getProductos().subscribe(data => this.productos.set(data));
  }

  filteredProductos(): Producto[] {
    const term = this.searchTerm.toLowerCase().trim();
    if (!term) return this.productos();
    return this.productos().filter(p => 
      p.nombre.toLowerCase().includes(term) || 
      p.descripcion.toLowerCase().includes(term)
    );
  }
}
