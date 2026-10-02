import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DashboardService } from '../../services/dashboard.service';
import { Venta } from '../../models/dashboard.model';

@Component({
  selector: 'app-ventas',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="p-4" style="background-color: #f8fafc; min-height: 100vh;">
      <div class="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-2">
        <div>
          <h2 class="h3 fw-bold text-dark mb-1">Registro de Ventas</h2>
          <p class="text-muted mb-0">Control de órdenes, montos y despachos de materiales.</p>
        </div>
      </div>

      <div class="card border-0 shadow-sm rounded-4 bg-white">
        <div class="card-body p-4">
          <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">
            <div class="input-group" style="max-width: 320px;">
              <span class="input-group-text bg-light border-end-0">🔍</span>
              <input type="text" [(ngModel)]="searchTerm" placeholder="Buscar por cliente o NIT..." class="form-control bg-light border-start-0" />
            </div>
            <span class="text-muted small">Mostrando {{ filteredVentas().length }} ventas</span>
          </div>

          <div class="table-responsive">
            <table class="table table-hover align-middle mb-0">
              <thead class="table-light small text-muted">
                <tr>
                  <th>Cliente</th>
                  <th>NIT / Documento</th>
                  <th>Fecha de Venta</th>
                  <th class="text-center">Items</th>
                  <th>Monto Total</th>
                  <th class="text-center">Estado de Despacho</th>
                </tr>
              </thead>
              <tbody>
                @for (v of filteredVentas(); track v.id) {
                  <tr>
                    <td class="fw-bold text-dark">{{ v.cliente }}</td>
                    <td><span class="badge bg-light text-secondary border font-monospace">{{ v.documentoCliente }}</span></td>
                    <td class="text-muted small">{{ v.fecha | date:'dd/MM/yyyy HH:mm' }}</td>
                    <td class="text-center">
                      <span class="badge bg-secondary-subtle text-secondary rounded-pill px-2">{{ v.totalItems }}</span>
                    </td>
                    <td class="fw-bold text-dark">{{ v.total | currency:'COP':'symbol-narrow':'1.0-0' }}</td>
                    <td class="text-center">
                      <span class="badge px-3 py-2 rounded-pill" [ngClass]="{
                        'bg-warning text-dark': v.estadoDespacho === 'Pendiente',
                        'bg-primary': v.estadoDespacho === 'En Ruta',
                        'bg-success': v.estadoDespacho === 'Entregado'
                      }">{{ v.estadoDespacho }}</span>
                    </td>
                  </tr>
                } @empty {
                  <tr>
                    <td colspan="6" class="text-center text-muted py-4">No se encontraron ventas.</td>
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
export class VentasComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  ventas = signal<Venta[]>([]);
  searchTerm = '';

  ngOnInit(): void {
    this.dashboardService.getVentas().subscribe(data => this.ventas.set(data));
  }

  filteredVentas(): Venta[] {
    const term = this.searchTerm.toLowerCase().trim();
    if (!term) return this.ventas();
    return this.ventas().filter(v => 
      v.cliente.toLowerCase().includes(term) || 
      v.documentoCliente.toLowerCase().includes(term)
    );
  }
}
