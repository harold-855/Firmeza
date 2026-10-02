import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DashboardService } from '../../services/dashboard.service';
import { Producto, CreateProductoDto, UpdateProductoDto } from '../../models/dashboard.model';

@Component({
  selector: 'app-productos',
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
          <h2 class="h3 fw-bold text-dark mb-1">Catálogo de Productos y Materiales</h2>
          <p class="text-muted mb-0">Gestión de inventario de materiales pesados y de construcción.</p>
        </div>
        <div class="d-flex gap-2">
          <button (click)="openCreateModal()" class="btn btn-primary rounded-pill px-4 d-flex align-items-center gap-2 shadow-sm">
            <span>+</span>
            <span class="fw-semibold">Nuevo Producto</span>
          </button>
        </div>
      </div>

      <!-- KPI Mini-cards -->
      <div class="row g-3 mb-4">
        <div class="col-12 col-sm-4">
          <div class="card border-0 bg-white rounded-4 p-3 shadow-sm">
            <span class="text-muted small fw-semibold text-uppercase">Total Productos</span>
            <h4 class="fw-bold mb-0 text-dark">{{ productos().length }}</h4>
          </div>
        </div>
        <div class="col-12 col-sm-4">
          <div class="card border-0 bg-white rounded-4 p-3 shadow-sm">
            <span class="text-muted small fw-semibold text-uppercase">Activos</span>
            <h4 class="fw-bold mb-0 text-success">{{ getActivosCount() }}</h4>
          </div>
        </div>
        <div class="col-12 col-sm-4">
          <div class="card border-0 bg-white rounded-4 p-3 shadow-sm">
            <span class="text-muted small fw-semibold text-uppercase">Bajo Stock (&lt; 50)</span>
            <h4 class="fw-bold mb-0 text-danger">{{ getBajoStockCount() }}</h4>
          </div>
        </div>
      </div>

      <!-- Search & Filters -->
      <div class="card border-0 shadow-sm rounded-4 bg-white mb-4">
        <div class="card-body p-4">
          <div class="row g-3">
            <div class="col-12 col-md-4">
              <label class="form-label small fw-bold text-muted">Búsqueda</label>
              <div class="input-group">
                <span class="input-group-text bg-light border-end-0">🔍</span>
                <input type="text" [(ngModel)]="searchTerm" placeholder="Buscar por nombre o descripción..." class="form-control bg-light border-start-0" />
              </div>
            </div>

            <div class="col-6 col-md-3">
              <label class="form-label small fw-bold text-muted">Estado</label>
              <select [(ngModel)]="statusFilter" class="form-select bg-light">
                <option value="todos">Todos los estados</option>
                <option value="activos">Solo Activos</option>
                <option value="inactivos">Solo Inactivos</option>
              </select>
            </div>

            <div class="col-6 col-md-3">
              <label class="form-label small fw-bold text-muted">Stock</label>
              <select [(ngModel)]="stockFilter" class="form-select bg-light">
                <option value="todos">Todo el stock</option>
                <option value="bajo">Bajo Stock (&lt; 50)</option>
                <option value="optimo">Stock Óptimo (≥ 50)</option>
              </select>
            </div>

            <div class="col-12 col-md-2">
              <label class="form-label small fw-bold text-muted">Ordenar</label>
              <select [(ngModel)]="sortBy" class="form-select bg-light">
                <option value="nombre_asc">Nombre (A-Z)</option>
                <option value="nombre_desc">Nombre (Z-A)</option>
                <option value="precio_asc">Menor Precio</option>
                <option value="precio_desc">Mayor Precio</option>
                <option value="stock_asc">Menor Stock</option>
                <option value="stock_desc">Mayor Stock</option>
              </select>
            </div>
          </div>
        </div>
      </div>

      <!-- Main Products Table -->
      <div class="card border-0 shadow-sm rounded-4 bg-white">
        <div class="card-body p-4">
          <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">
            <h5 class="fw-bold mb-0 text-dark">Materiales Disponibles</h5>
            <span class="text-muted small">Mostrando {{ filteredProductos().length }} de {{ productos().length }} productos</span>
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
                  <th class="text-end">Acciones</th>
                </tr>
              </thead>
              <tbody>
                @for (prod of filteredProductos(); track prod.id) {
                  <tr>
                    <td class="fw-bold text-dark">{{ prod.nombre }}</td>
                    <td class="text-muted small" style="max-width: 250px;">{{ prod.descripcion }}</td>
                    <td><span class="badge bg-light text-dark border">{{ prod.unidadMedida }}</span></td>
                    <td class="fw-semibold text-primary">{{ prod.precioUnitario | currency:'COP':'symbol-narrow':'1.0-0' }}</td>
                    <td class="text-center">
                      <span class="badge" [ngClass]="prod.stockActual >= 50 ? 'bg-success-subtle text-success' : 'bg-danger-subtle text-danger'">
                        {{ prod.stockActual }}
                      </span>
                    </td>
                    <td class="text-center">
                      <span class="badge" [ngClass]="prod.activo ? 'bg-success' : 'bg-secondary'">
                        {{ prod.activo ? 'Activo' : 'Inactivo' }}
                      </span>
                    </td>
                    <td class="text-end">
                      <div class="btn-group btn-group-sm">
                        <button (click)="openEditModal(prod)" class="btn btn-outline-primary" title="Editar">
                          ✏️
                        </button>
                        <button (click)="openDeleteModal(prod)" class="btn btn-outline-danger" title="Eliminar / Desactivar">
                          🗑️
                        </button>
                      </div>
                    </td>
                  </tr>
                } @empty {
                  <tr>
                    <td colspan="7" class="text-center text-muted py-5">
                      No se encontraron productos que coincidan con los filtros.
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- Create / Edit Modal Backdrop -->
      @if (showFormModal()) {
        <div class="modal fade show d-block" tabindex="-1" style="background: rgba(0,0,0,0.5);">
          <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow rounded-4">
              <div class="modal-header border-0 pb-0">
                <h5 class="modal-title fw-bold text-dark">{{ isEditing() ? 'Editar Producto' : 'Nuevo Producto' }}</h5>
                <button type="button" class="btn-close" (click)="closeFormModal()"></button>
              </div>
              <div class="modal-body p-4">
                <form (ngSubmit)="saveProducto()">
                  <div class="mb-3">
                    <label class="form-label small fw-bold text-muted">Nombre del Material *</label>
                    <input type="text" [(ngModel)]="currentProducto.nombre" name="nombre" class="form-control rounded-3" required />
                  </div>

                  <div class="mb-3">
                    <label class="form-label small fw-bold text-muted">Descripción *</label>
                    <textarea [(ngModel)]="currentProducto.descripcion" name="descripcion" rows="2" class="form-control rounded-3" required></textarea>
                  </div>

                  <div class="row g-2 mb-3">
                    <div class="col-6">
                      <label class="form-label small fw-bold text-muted">Unidad de Medida *</label>
                      <input type="text" [(ngModel)]="currentProducto.unidadMedida" name="unidadMedida" placeholder="Bolsa, M3, etc." class="form-control rounded-3" required />
                    </div>
                    <div class="col-6">
                      <label class="form-label small fw-bold text-muted">Precio Unitario ($) *</label>
                      <input type="number" [(ngModel)]="currentProducto.precioUnitario" name="precioUnitario" min="1" class="form-control rounded-3" required />
                    </div>
                  </div>

                  <div class="row g-2 mb-3">
                    <div class="col-6">
                      <label class="form-label small fw-bold text-muted">Stock *</label>
                      <input type="number" [(ngModel)]="currentProducto.stockActual" name="stockActual" min="0" class="form-control rounded-3" required />
                    </div>
                    <div class="col-6 d-flex align-items-center pt-3">
                      <div class="form-check form-switch">
                        <input class="form-check-input" type="checkbox" [(ngModel)]="currentProducto.activo" name="activo" id="activoCheck" />
                        <label class="form-check-label small fw-semibold" for="activoCheck">Activo para venta</label>
                      </div>
                    </div>
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

      <!-- Delete Confirmation Modal Backdrop -->
      @if (showDeleteModal()) {
        <div class="modal fade show d-block" tabindex="-1" style="background: rgba(0,0,0,0.5);">
          <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow rounded-4">
              <div class="modal-header border-0 pb-0">
                <h5 class="modal-title fw-bold text-danger">Eliminar / Desactivar Producto</h5>
                <button type="button" class="btn-close" (click)="showDeleteModal.set(false)"></button>
              </div>
              <div class="modal-body p-4">
                <p>¿Estás seguro de que deseas eliminar o desactivar el producto <strong>{{ selectedToDelete()?.nombre }}</strong>?</p>
                <div class="d-flex justify-content-end gap-2 mt-4">
                  <button type="button" class="btn btn-light rounded-pill px-3" (click)="showDeleteModal.set(false)">Cancelar</button>
                  <button type="button" class="btn btn-danger rounded-pill px-4 fw-semibold" (click)="confirmDelete()" [disabled]="isSubmitting()">
                    {{ isSubmitting() ? 'Procesando...' : 'Confirmar' }}
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
export class ProductosComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  productos = signal<Producto[]>([]);
  searchTerm = '';
  statusFilter = 'todos';
  stockFilter = 'todos';
  sortBy = 'nombre_asc';

  alertMessage = signal<string | null>(null);
  alertType = signal<'success' | 'danger'>('success');

  showFormModal = signal(false);
  showDeleteModal = signal(false);
  isEditing = signal(false);
  isSubmitting = signal(false);
  formError = signal<string | null>(null);

  selectedToDelete = signal<Producto | null>(null);

  currentProducto: any = {
    id: '',
    nombre: '',
    descripcion: '',
    unidadMedida: '',
    precioUnitario: 0,
    stockActual: 0,
    activo: true
  };

  ngOnInit(): void {
    this.loadProductos();
  }

  loadProductos(): void {
    this.dashboardService.getProductos().subscribe({
      next: (data) => this.productos.set(data),
      error: (err) => {
        console.error(err);
        this.showAlert('Error al cargar la lista de productos', 'danger');
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

  getActivosCount(): number {
    return this.productos().filter(p => p.activo).length;
  }

  getBajoStockCount(): number {
    return this.productos().filter(p => p.stockActual < 50).length;
  }

  filteredProductos(): Producto[] {
    let result = [...this.productos()];

    // Search term
    const term = this.searchTerm.toLowerCase().trim();
    if (term) {
      result = result.filter(p =>
        p.nombre.toLowerCase().includes(term) ||
        p.descripcion.toLowerCase().includes(term) ||
        p.unidadMedida.toLowerCase().includes(term)
      );
    }

    // Status filter
    if (this.statusFilter === 'activos') {
      result = result.filter(p => p.activo);
    } else if (this.statusFilter === 'inactivos') {
      result = result.filter(p => !p.activo);
    }

    // Stock filter
    if (this.stockFilter === 'bajo') {
      result = result.filter(p => p.stockActual < 50);
    } else if (this.stockFilter === 'optimo') {
      result = result.filter(p => p.stockActual >= 50);
    }

    // Sorting
    result.sort((a, b) => {
      switch (this.sortBy) {
        case 'nombre_desc': return b.nombre.localeCompare(a.nombre);
        case 'precio_asc': return a.precioUnitario - b.precioUnitario;
        case 'precio_desc': return b.precioUnitario - a.precioUnitario;
        case 'stock_asc': return a.stockActual - b.stockActual;
        case 'stock_desc': return b.stockActual - a.stockActual;
        case 'nombre_asc':
        default:
          return a.nombre.localeCompare(b.nombre);
      }
    });

    return result;
  }

  openCreateModal(): void {
    this.isEditing.set(false);
    this.formError.set(null);
    this.currentProducto = {
      nombre: '',
      descripcion: '',
      unidadMedida: 'Bolsa',
      precioUnitario: 0,
      stockActual: 0,
      activo: true
    };
    this.showFormModal.set(true);
  }

  openEditModal(prod: Producto): void {
    this.isEditing.set(true);
    this.formError.set(null);
    this.currentProducto = { ...prod };
    this.showFormModal.set(true);
  }

  closeFormModal(): void {
    this.showFormModal.set(false);
    this.formError.set(null);
  }

  openDeleteModal(prod: Producto): void {
    this.selectedToDelete.set(prod);
    this.showDeleteModal.set(true);
  }

  saveProducto(): void {
    if (!this.currentProducto.nombre?.trim() || !this.currentProducto.descripcion?.trim() || !this.currentProducto.unidadMedida?.trim()) {
      this.formError.set('Por favor complete todos los campos obligatorios.');
      return;
    }

    if (this.currentProducto.precioUnitario <= 0) {
      this.formError.set('El precio unitario debe ser mayor a 0.');
      return;
    }

    if (this.currentProducto.stockActual < 0) {
      this.formError.set('El stock no puede ser negativo.');
      return;
    }

    this.isSubmitting.set(true);
    this.formError.set(null);

    if (this.isEditing()) {
      this.dashboardService.updateProducto(this.currentProducto.id, this.currentProducto).subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.showFormModal.set(false);
          this.showAlert(`Producto "${this.currentProducto.nombre}" actualizado con éxito.`);
          this.loadProductos();
        },
        error: (err) => {
          this.isSubmitting.set(false);
          this.formError.set('Error al actualizar el producto en el servidor.');
          // Optimistic local update for mock / offline
          const list = this.productos().map(p => p.id === this.currentProducto.id ? { ...this.currentProducto } : p);
          this.productos.set(list);
          this.showFormModal.set(false);
          this.showAlert(`Producto "${this.currentProducto.nombre}" actualizado localmente.`);
        }
      });
    } else {
      this.dashboardService.createProducto(this.currentProducto).subscribe({
        next: (created) => {
          this.isSubmitting.set(false);
          this.showFormModal.set(false);
          this.showAlert(`Producto "${created.nombre}" creado exitosamente.`);
          this.loadProductos();
        },
        error: (err) => {
          this.isSubmitting.set(false);
          // Local fallback
          const newProd: Producto = {
            ...this.currentProducto,
            id: crypto.randomUUID ? crypto.randomUUID() : Date.now().toString()
          };
          this.productos.set([...this.productos(), newProd]);
          this.showFormModal.set(false);
          this.showAlert(`Producto "${newProd.nombre}" creado localmente.`);
        }
      });
    }
  }

  confirmDelete(): void {
    const prod = this.selectedToDelete();
    if (!prod) return;

    this.isSubmitting.set(true);
    this.dashboardService.deleteProducto(prod.id).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.showDeleteModal.set(false);
        this.showAlert(`Producto "${prod.nombre}" procesado correctamente.`);
        this.loadProductos();
      },
      error: () => {
        this.isSubmitting.set(false);
        this.showDeleteModal.set(false);
        // Local fallback removal or deactivation
        const updated = this.productos().filter(p => p.id !== prod.id);
        this.productos.set(updated);
        this.showAlert(`Producto "${prod.nombre}" eliminado localmente.`);
      }
    });
  }
}
