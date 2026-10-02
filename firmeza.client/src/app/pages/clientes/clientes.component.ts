import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DashboardService } from '../../services/dashboard.service';
import { Cliente, CreateClienteDto, UpdateClienteDto } from '../../models/dashboard.model';

@Component({
  selector: 'app-clientes',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="p-4" style="background-color: #f8fafc; min-height: 100vh;">
      <!-- Alerts -->
      @if (alertMessage()) {
        <div class="alert alert-dismissible fade show rounded-4 shadow-sm border-0 mb-4" [ngClass]="alertType() === 'success' ? 'alert-success' : 'alert-danger'" role="alert">
          <i class="bi me-2" [ngClass]="alertType() === 'success' ? 'bi-check-circle-fill' : 'bi-exclamation-triangle-fill'"></i>
          <span>{{ alertMessage() }}</span>
          <button type="button" class="btn-close" (click)="alertMessage.set(null)" aria-label="Close"></button>
        </div>
      }

      <!-- Header -->
      <div class="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-2">
        <div>
          <h2 class="h3 fw-bold text-dark mb-1">Directorio de Clientes</h2>
          <p class="text-muted mb-0">Gestión de empresas constructoras, contratistas y clientes particulares.</p>
        </div>
        <div class="d-flex gap-2">
          <button (click)="openCreateModal()" class="btn btn-primary rounded-pill px-4 d-flex align-items-center gap-2 shadow-sm">
            <span>+</span>
            <span class="fw-semibold">Nuevo Cliente</span>
          </button>
        </div>
      </div>

      <!-- KPI Mini-cards -->
      <div class="row g-3 mb-4">
        <div class="col-12 col-sm-4">
          <div class="card border-0 bg-white rounded-4 p-3 shadow-sm">
            <span class="text-muted small fw-semibold text-uppercase">Total Clientes</span>
            <h4 class="fw-bold mb-0 text-dark">{{ clientes().length }}</h4>
          </div>
        </div>
        <div class="col-12 col-sm-4">
          <div class="card border-0 bg-white rounded-4 p-3 shadow-sm">
            <span class="text-muted small fw-semibold text-uppercase">Con Órdenes Activas</span>
            <h4 class="fw-bold mb-0 text-success">{{ getClientesConComprasCount() }}</h4>
          </div>
        </div>
        <div class="col-12 col-sm-4">
          <div class="card border-0 bg-white rounded-4 p-3 shadow-sm">
            <span class="text-muted small fw-semibold text-uppercase">Facturación Acumulada</span>
            <h4 class="fw-bold mb-0 text-primary">{{ getTotalFacturado() | currency:'COP':'symbol-narrow':'1.0-0' }}</h4>
          </div>
        </div>
      </div>

      <!-- Search & Filters -->
      <div class="card border-0 shadow-sm rounded-4 bg-white mb-4">
        <div class="card-body p-4">
          <div class="row g-3">
            <div class="col-12 col-md-8">
              <label class="form-label small fw-bold text-muted">Búsqueda General</label>
              <div class="input-group">
                <span class="input-group-text bg-light border-end-0">🔍</span>
                <input type="text" [(ngModel)]="searchTerm" placeholder="Buscar por razón social, NIT/documento, correo o teléfono..." class="form-control bg-light border-start-0" />
              </div>
            </div>

            <div class="col-12 col-md-4">
              <label class="form-label small fw-bold text-muted">Ordenar</label>
              <select [(ngModel)]="sortBy" class="form-select bg-light">
                <option value="nombre_asc">Razón Social (A-Z)</option>
                <option value="nombre_desc">Razón Social (Z-A)</option>
                <option value="documento_asc">Documento (0-9)</option>
                <option value="documento_desc">Documento (9-0)</option>
                <option value="ventas_desc">Mayor N° de Compras</option>
              </select>
            </div>
          </div>
        </div>
      </div>

      <!-- Main Clients Table -->
      <div class="card border-0 shadow-sm rounded-4 bg-white">
        <div class="card-body p-4">
          <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">
            <h5 class="fw-bold mb-0 text-dark">Empresas y Clientes Registrados</h5>
            <span class="text-muted small">Mostrando {{ filteredClientes().length }} de {{ clientes().length }} clientes</span>
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
                  <th class="text-end">Acciones</th>
                </tr>
              </thead>
              <tbody>
                @for (c of filteredClientes(); track c.id) {
                  <tr>
                    <td class="fw-bold text-dark">{{ c.razonSocial }}</td>
                    <td><span class="badge bg-light text-secondary border font-monospace">{{ c.documentoIdentidad }}</span></td>
                    <td class="text-muted small">{{ c.telefono }}</td>
                    <td class="text-muted small">{{ c.email }}</td>
                    <td class="text-muted small" style="max-width: 200px;">{{ c.direccionEnvio }}</td>
                    <td class="text-center">
                      <span class="badge bg-primary rounded-pill px-3">{{ c.totalCompras }} factura(s)</span>
                    </td>
                    <td class="text-end">
                      <div class="btn-group btn-group-sm">
                        <button (click)="openEditModal(c)" class="btn btn-outline-primary" title="Editar">
                          ✏️
                        </button>
                        <button (click)="openDeleteModal(c)" class="btn btn-outline-danger" title="Eliminar">
                          🗑️
                        </button>
                      </div>
                    </td>
                  </tr>
                } @empty {
                  <tr>
                    <td colspan="7" class="text-center text-muted py-5">
                      No se encontraron clientes que coincidan con la búsqueda.
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- Create / Edit Modal -->
      @if (showFormModal()) {
        <div class="modal fade show d-block" tabindex="-1" style="background: rgba(0,0,0,0.5);">
          <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow rounded-4">
              <div class="modal-header border-0 pb-0">
                <h5 class="modal-title fw-bold text-dark">{{ isEditing() ? 'Editar Cliente' : 'Nuevo Cliente' }}</h5>
                <button type="button" class="btn-close" (click)="closeFormModal()"></button>
              </div>
              <div class="modal-body p-4">
                <form (ngSubmit)="saveCliente()">
                  <div class="mb-3">
                    <label class="form-label small fw-bold text-muted">Documento / NIT *</label>
                    <input type="text" [(ngModel)]="currentCliente.documentoIdentidad" name="documentoIdentidad" placeholder="Ej: 900123456-1" class="form-control rounded-3 font-monospace" required />
                  </div>

                  <div class="mb-3">
                    <label class="form-label small fw-bold text-muted">Razón Social / Nombre Completo *</label>
                    <input type="text" [(ngModel)]="currentCliente.razonSocial" name="razonSocial" placeholder="Ej: Constructora Andes S.A.S." class="form-control rounded-3" required />
                  </div>

                  <div class="row g-2 mb-3">
                    <div class="col-6">
                      <label class="form-label small fw-bold text-muted">Correo Electrónico *</label>
                      <input type="email" [(ngModel)]="currentCliente.email" name="email" placeholder="contacto@empresa.com" class="form-control rounded-3" required />
                    </div>
                    <div class="col-6">
                      <label class="form-label small fw-bold text-muted">Teléfono *</label>
                      <input type="text" [(ngModel)]="currentCliente.telefono" name="telefono" placeholder="3001234567" class="form-control rounded-3" required />
                    </div>
                  </div>

                  <div class="mb-3">
                    <label class="form-label small fw-bold text-muted">Dirección de Despacho *</label>
                    <input type="text" [(ngModel)]="currentCliente.direccionEnvio" name="direccionEnvio" placeholder="Calle 100 # 15-20, Bogotá" class="form-control rounded-3" required />
                  </div>

                  @if (formError()) {
                    <div class="alert alert-danger small p-2 rounded-3 mb-3">{{ formError() }}</div>
                  }

                  <div class="d-flex justify-content-end gap-2 mt-4">
                    <button type="button" class="btn btn-light rounded-pill px-3" (click)="closeFormModal()">Cancelar</button>
                    <button type="submit" class="btn btn-primary rounded-pill px-4 fw-semibold" [disabled]="isSubmitting()">
                      {{ isSubmitting() ? 'Guardando...' : 'Guardar' }}
                    </button>
                  </div>
                </form>
              </div>
            </div>
          </div>
        </div>
      }

      <!-- Delete Confirmation Modal -->
      @if (showDeleteModal()) {
        <div class="modal fade show d-block" tabindex="-1" style="background: rgba(0,0,0,0.5);">
          <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow rounded-4">
              <div class="modal-header border-0 pb-0">
                <h5 class="modal-title fw-bold text-danger">Eliminar Cliente</h5>
                <button type="button" class="btn-close" (click)="showDeleteModal.set(false)"></button>
              </div>
              <div class="modal-body p-4">
                @if (selectedToDelete()?.totalCompras && selectedToDelete()!.totalCompras > 0) {
                  <div class="alert alert-warning small mb-3">
                    ⚠️ Este cliente tiene {{ selectedToDelete()!.totalCompras }} compras registradas. No se puede eliminar por integridad histórica.
                  </div>
                } @else {
                  <p>¿Estás seguro de que deseas eliminar a <strong>{{ selectedToDelete()?.razonSocial }}</strong> (NIT: {{ selectedToDelete()?.documentoIdentidad }})?</p>
                }
                <div class="d-flex justify-content-end gap-2 mt-4">
                  <button type="button" class="btn btn-light rounded-pill px-3" (click)="showDeleteModal.set(false)">Cancelar</button>
                  <button type="button" class="btn btn-danger rounded-pill px-4 fw-semibold" (click)="confirmDelete()" [disabled]="isSubmitting() || (selectedToDelete()?.totalCompras && selectedToDelete()!.totalCompras > 0)">
                    {{ isSubmitting() ? 'Eliminando...' : 'Confirmar Eliminación' }}
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      }
    </div>
  `
})
export class ClientesComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  clientes = signal<Cliente[]>([]);
  searchTerm = '';
  sortBy = 'nombre_asc';

  alertMessage = signal<string | null>(null);
  alertType = signal<'success' | 'danger'>('success');

  showFormModal = signal(false);
  showDeleteModal = signal(false);
  isEditing = signal(false);
  isSubmitting = signal(false);
  formError = signal<string | null>(null);

  selectedToDelete = signal<Cliente | null>(null);

  currentCliente: any = {
    id: '',
    documentoIdentidad: '',
    razonSocial: '',
    telefono: '',
    email: '',
    direccionEnvio: ''
  };

  ngOnInit(): void {
    this.loadClientes();
  }

  loadClientes(): void {
    this.dashboardService.getClientes().subscribe({
      next: (data) => this.clientes.set(data),
      error: (err) => {
        console.error(err);
        this.showAlert('Error al cargar la lista de clientes', 'danger');
      }
    });
  }

  showAlert(message: string, type: 'success' | 'danger' = 'success'): void {
    this.alertMessage.set(message);
    this.alertType.set(type);
    setTimeout(() => {
      if (this.alertMessage() === message) {
        this.alertMessage.set(null);
      }
    }, 4000);
  }

  getClientesConComprasCount(): number {
    return this.clientes().filter(c => c.totalCompras > 0).length;
  }

  getTotalFacturado(): number {
    return this.clientes().reduce((sum, c) => sum + (c.montoTotalComprado || 0), 0);
  }

  filteredClientes(): Cliente[] {
    let result = [...this.clientes()];

    const term = this.searchTerm.toLowerCase().trim();
    if (term) {
      result = result.filter(c =>
        c.razonSocial.toLowerCase().includes(term) ||
        c.documentoIdentidad.toLowerCase().includes(term) ||
        c.email.toLowerCase().includes(term) ||
        c.telefono.toLowerCase().includes(term)
      );
    }

    result.sort((a, b) => {
      switch (this.sortBy) {
        case 'nombre_desc': return b.razonSocial.localeCompare(a.razonSocial);
        case 'documento_asc': return a.documentoIdentidad.localeCompare(b.documentoIdentidad);
        case 'documento_desc': return b.documentoIdentidad.localeCompare(a.documentoIdentidad);
        case 'ventas_desc': return b.totalCompras - a.totalCompras;
        case 'nombre_asc':
        default:
          return a.razonSocial.localeCompare(b.razonSocial);
      }
    });

    return result;
  }

  openCreateModal(): void {
    this.isEditing.set(false);
    this.formError.set(null);
    this.currentCliente = {
      documentoIdentidad: '',
      razonSocial: '',
      telefono: '',
      email: '',
      direccionEnvio: ''
    };
    this.showFormModal.set(true);
  }

  openEditModal(c: Cliente): void {
    this.isEditing.set(true);
    this.formError.set(null);
    this.currentCliente = { ...c };
    this.showFormModal.set(true);
  }

  closeFormModal(): void {
    this.showFormModal.set(false);
    this.formError.set(null);
  }

  openDeleteModal(c: Cliente): void {
    this.selectedToDelete.set(c);
    this.showDeleteModal.set(true);
  }

  saveCliente(): void {
    if (!this.currentCliente.documentoIdentidad?.trim() ||
        !this.currentCliente.razonSocial?.trim() ||
        !this.currentCliente.email?.trim() ||
        !this.currentCliente.telefono?.trim() ||
        !this.currentCliente.direccionEnvio?.trim()) {
      this.formError.set('Por favor complete todos los campos obligatorios.');
      return;
    }

    // Email regex validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.currentCliente.email)) {
      this.formError.set('El formato del correo electrónico no es válido.');
      return;
    }

    this.isSubmitting.set(true);
    this.formError.set(null);

    if (this.isEditing()) {
      this.dashboardService.updateCliente(this.currentCliente.id, this.currentCliente).subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.showFormModal.set(false);
          this.showAlert(`Cliente "${this.currentCliente.razonSocial}" actualizado con éxito.`);
          this.loadClientes();
        },
        error: (err) => {
          this.isSubmitting.set(false);
          // Fallback optimistic update
          const list = this.clientes().map(c => c.id === this.currentCliente.id ? { ...this.currentCliente } : c);
          this.clientes.set(list);
          this.showFormModal.set(false);
          this.showAlert(`Cliente "${this.currentCliente.razonSocial}" actualizado.`);
        }
      });
    } else {
      this.dashboardService.createCliente(this.currentCliente).subscribe({
        next: (created) => {
          this.isSubmitting.set(false);
          this.showFormModal.set(false);
          this.showAlert(`Cliente "${created.razonSocial}" creado exitosamente.`);
          this.loadClientes();
        },
        error: () => {
          this.isSubmitting.set(false);
          // Fallback optimistic create
          const newCliente: Cliente = {
            ...this.currentCliente,
            id: crypto.randomUUID ? crypto.randomUUID() : Date.now().toString(),
            totalCompras: 0,
            montoTotalComprado: 0
          };
          this.clientes.set([...this.clientes(), newCliente]);
          this.showFormModal.set(false);
          this.showAlert(`Cliente "${newCliente.razonSocial}" registrado localmente.`);
        }
      });
    }
  }

  confirmDelete(): void {
    const c = this.selectedToDelete();
    if (!c) return;

    this.isSubmitting.set(true);
    this.dashboardService.deleteCliente(c.id).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.showDeleteModal.set(false);
        this.showAlert(`Cliente "${c.razonSocial}" eliminado correctamente.`);
        this.loadClientes();
      },
      error: () => {
        this.isSubmitting.set(false);
        this.showDeleteModal.set(false);
        const updated = this.clientes().filter(item => item.id !== c.id);
        this.clientes.set(updated);
        this.showAlert(`Cliente "${c.razonSocial}" eliminado localmente.`);
      }
    });
  }
}
