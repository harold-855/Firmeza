import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DashboardService } from '../../services/dashboard.service';
import { Cliente } from '../../models/dashboard.model';

@Component({
  selector: 'app-clientes',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="p-4" style="background-color: #f8fafc; min-height: 100vh;">
      <div class="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-2">
        <div>
          <h2 class="h3 fw-bold text-dark mb-1">Directorio de Clientes</h2>
          <p class="text-muted mb-0">Listado de empresas y personas registradas en el sistema.</p>
        </div>
      </div>

      <div class="card border-0 shadow-sm rounded-4 bg-white">
        <div class="card-body p-4">
          <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">
            <div class="input-group" style="max-width: 320px;">
              <span class="input-group-text bg-light border-end-0">🔍</span>
              <input type="text" [(ngModel)]="searchTerm" placeholder="Buscar por razón social o NIT..." class="form-control bg-light border-start-0" />
            </div>
            <span class="text-muted small">Mostrando {{ filteredClientes().length }} clientes</span>
          </div>

          <div class="table-responsive">
            <table class="table table-hover align-middle mb-0">
              <thead class="table-light small text-muted">
                <tr>
                  <th>Razón Social</th>
                  <th>NIT / Documento</th>
                  <th>Teléfono</th>
                  <th>Correo Electrónico</th>
                  <th>Dirección de Entrega</th>
                  <th class="text-center">Compras</th>
                </tr>
              </thead>
              <tbody>
                @for (c of filteredClientes(); track c.id) {
                  <tr>
                    <td class="fw-bold text-dark">{{ c.razonSocial }}</td>
                    <td><span class="badge bg-light text-secondary border font-monospace">{{ c.documentoIdentidad }}</span></td>
                    <td class="text-muted small">{{ c.telefono }}</td>
                    <td class="text-muted small">{{ c.email }}</td>
                    <td class="text-muted small">{{ c.direccionEnvio }}</td>
                    <td class="text-center">
                      <span class="badge bg-primary rounded-pill px-3">{{ c.totalCompras }}</span>
                    </td>
                  </tr>
                } @empty {
                  <tr>
                    <td colspan="6" class="text-center text-muted py-4">No se encontraron clientes.</td>
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
export class ClientesComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  clientes = signal<Cliente[]>([]);
  searchTerm = '';

  ngOnInit(): void {
    this.dashboardService.getClientes().subscribe(data => this.clientes.set(data));
  }

  filteredClientes(): Cliente[] {
    const term = this.searchTerm.toLowerCase().trim();
    if (!term) return this.clientes();
    return this.clientes().filter(c => 
      c.razonSocial.toLowerCase().includes(term) || 
      c.documentoIdentidad.toLowerCase().includes(term) ||
      c.email.toLowerCase().includes(term)
    );
  }
}
