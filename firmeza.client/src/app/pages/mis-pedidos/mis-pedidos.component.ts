import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ClienteApiService } from '../../services/cliente-api.service';
import { VentaDto } from '../../models/venta.model';

const ORDERS_KEY = 'firmeza_cliente_orders';

@Component({
  selector: 'app-mis-pedidos',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="p-4" style="background-color: #f8fafc; min-height: 100vh;">
      <!-- Page Header -->
      <div class="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-2">
        <div>
          <h2 class="h3 fw-bold text-dark mb-1">Mis Pedidos y Comprobantes</h2>
          <p class="text-muted mb-0">Historial de órdenes de compra realizadas y descarga de comprobantes oficiales.</p>
        </div>
        <a routerLink="/productos" class="btn btn-primary rounded-pill px-4 d-flex align-items-center gap-2 shadow-sm">
          <i class="bi bi-plus-lg"></i>
          <span>Nuevo Pedido</span>
        </a>
      </div>

      <!-- Search & Filters -->
      <div class="card border-0 shadow-sm rounded-4 bg-white mb-4">
        <div class="card-body p-4">
          <div class="row g-3 align-items-center">
            <div class="col-12 col-md-6">
              <div class="input-group">
                <span class="input-group-text bg-light border-end-0 text-muted">
                  <i class="bi bi-search"></i>
                </span>
                <input
                  type="text"
                  [(ngModel)]="searchTerm"
                  placeholder="Buscar por número de comprobante o ID..."
                  class="form-control bg-light border-start-0"
                />
              </div>
            </div>

            <div class="col-6 col-md-3">
              <select [(ngModel)]="statusFilter" class="form-select bg-light">
                <option value="todos">Todos los estados</option>
                <option value="Pendiente">Pendientes</option>
                <option value="En Ruta">En Ruta</option>
                <option value="Entregado">Entregados</option>
              </select>
            </div>

            <div class="col-6 col-md-3 text-end text-muted small">
              Total órdenes: <strong>{{ filteredOrders().length }}</strong>
            </div>
          </div>
        </div>
      </div>

      <!-- Orders List -->
      <div class="card border-0 shadow-sm rounded-4 bg-white overflow-hidden">
        <div class="card-body p-4">
          <div class="table-responsive">
            <table class="table table-hover align-middle mb-0">
              <thead class="table-light small text-muted">
                <tr>
                  <th>N° Comprobante</th>
                  <th>Fecha de Venta</th>
                  <th>Ítems</th>
                  <th>Monto Total</th>
                  <th class="text-center">Estado del Despacho</th>
                  <th class="text-end">Comprobante Oficial</th>
                </tr>
              </thead>
              <tbody>
                @for (order of filteredOrders(); track order.id) {
                  <tr>
                    <td>
                      <span class="badge bg-light text-dark border font-monospace px-2 py-1">
                        {{ order.numeroComprobante || ('REC-' + order.id.substring(0, 8).toUpperCase()) }}
                      </span>
                    </td>
                    <td class="text-muted small">
                      {{ order.fechaVenta | date:'dd/MM/yyyy HH:mm' }}
                    </td>
                    <td>
                      <span class="badge bg-secondary-subtle text-secondary rounded-pill px-3 py-1">
                        {{ order.totalItems || order.detalles?.length || 1 }} materiales
                      </span>
                    </td>
                    <td class="fw-bold text-primary">
                      {{ order.total | currency:'COP':'symbol-narrow':'1.0-0' }}
                    </td>
                    <td class="text-center">
                      <span
                        class="badge rounded-pill px-3 py-2"
                        [ngClass]="{
                          'bg-warning text-dark': order.estadoDespacho === 'Pendiente',
                          'bg-primary': order.estadoDespacho === 'En Ruta',
                          'bg-success': order.estadoDespacho === 'Entregado'
                        }"
                      >
                        {{ order.estadoDespacho }}
                      </span>
                    </td>
                    <td class="text-end">
                      <button
                        (click)="downloadPdf(order.id)"
                        class="btn btn-outline-danger btn-sm rounded-pill px-3 d-inline-flex align-items-center gap-2 shadow-sm"
                        [disabled]="downloadingId() === order.id"
                        title="Descargar PDF"
                      >
                        @if (downloadingId() === order.id) {
                          <span class="spinner-border spinner-border-sm" role="status"></span>
                          <span>Descargando...</span>
                        } @else {
                          <i class="bi bi-file-earmark-pdf-fill"></i>
                          <span>Descargar PDF</span>
                        }
                      </button>
                    </td>
                  </tr>
                } @empty {
                  <tr>
                    <td colspan="6" class="text-center py-5 text-muted">
                      <div class="fs-1 mb-2">📄</div>
                      <p class="mb-2 fw-semibold">No se encontraron pedidos registrados.</p>
                      <a routerLink="/productos" class="btn btn-sm btn-primary rounded-pill px-3">
                        Ir al Catálogo de Productos
                      </a>
                    </td>
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
export class MisPedidosComponent implements OnInit {
  private clienteApi = inject(ClienteApiService);

  orders = signal<VentaDto[]>([]);
  searchTerm = '';
  statusFilter = 'todos';
  downloadingId = signal<string | null>(null);

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    try {
      const raw = localStorage.getItem(ORDERS_KEY);
      if (raw) {
        this.orders.set(JSON.parse(raw));
      } else {
        // Seed default sample order for demo client
        const defaultOrders: VentaDto[] = [
          {
            id: 'd9b736e2-1234-4567-89ab-cdef01234567',
            numeroComprobante: 'REC-D9B736E2',
            fechaVenta: new Date().toISOString(),
            total: 1285000,
            subtotalBase: 1079832,
            iva: 205168,
            estadoDespacho: 'Pendiente',
            clienteId: '33333333-3333-3333-3333-333333333333',
            clienteRazonSocial: 'Inversiones Horizonte Ltda.',
            clienteDocumento: '901234567-8',
            clienteTelefono: '3204567890',
            clienteDireccion: 'Avenida 6N # 25-10, Cali',
            clienteEmail: 'cliente@firmeza.com',
            detalles: [],
            totalItems: 2
          }
        ];
        this.orders.set(defaultOrders);
        localStorage.setItem(ORDERS_KEY, JSON.stringify(defaultOrders));
      }
    } catch {
      this.orders.set([]);
    }
  }

  filteredOrders(): VentaDto[] {
    let list = [...this.orders()];

    if (this.searchTerm.trim()) {
      const term = this.searchTerm.toLowerCase().trim();
      list = list.filter(o =>
        o.id.toLowerCase().includes(term) ||
        o.numeroComprobante?.toLowerCase().includes(term)
      );
    }

    if (this.statusFilter !== 'todos') {
      list = list.filter(o => o.estadoDespacho === this.statusFilter);
    }

    return list;
  }

  downloadPdf(ventaId: string): void {
    this.downloadingId.set(ventaId);
    this.clienteApi.descargarReciboPdf(ventaId).subscribe({
      next: (blob) => {
        this.downloadingId.set(null);
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `comprobante_venta_${ventaId}.pdf`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.downloadingId.set(null);
        alert('No se pudo descargar el comprobante en este momento.');
      }
    });
  }
}
