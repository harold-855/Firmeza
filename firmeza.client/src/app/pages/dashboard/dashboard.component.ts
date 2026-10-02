import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DashboardService } from '../../services/dashboard.service';
import { DashboardMetrics } from '../../models/dashboard.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="dashboard-container p-4">
      <!-- Top Bar -->
      <div class="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-2">
        <div>
          <h2 class="h3 fw-bold text-dark mb-1">Panel de Control General</h2>
          <p class="text-muted mb-0">Métricas y resumen operativo de <strong>Firmeza</strong> en tiempo real.</p>
        </div>
        <div class="d-flex gap-2">
          <button (click)="loadMetrics()" class="btn btn-outline-primary btn-sm d-flex align-items-center gap-1 shadow-sm">
            <span>🔄</span> Actualizar Datos
          </button>
        </div>
      </div>

      <!-- KPI Cards Row -->
      <div class="row g-3 mb-4">
        <!-- Card 1: Productos -->
        <div class="col-12 col-sm-6 col-xl-3">
          <div class="card card-kpi border-0 shadow-sm rounded-4 h-100 kpi-blue">
            <div class="card-body p-3 d-flex align-items-center">
              <div class="kpi-icon bg-primary bg-opacity-10 text-primary rounded-3 me-3">
                📦
              </div>
              <div class="flex-grow-1">
                <span class="text-muted small fw-semibold text-uppercase">Productos</span>
                <h3 class="fw-bold mb-0 text-dark">{{ metrics()?.totalProductos ?? 0 }}</h3>
                <span class="badge bg-primary-subtle text-primary mt-1" style="font-size: 0.7rem;">En catálogo</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Card 2: Clientes -->
        <div class="col-12 col-sm-6 col-xl-3">
          <div class="card card-kpi border-0 shadow-sm rounded-4 h-100 kpi-purple">
            <div class="card-body p-3 d-flex align-items-center">
              <div class="kpi-icon bg-info bg-opacity-10 text-info rounded-3 me-3">
                👥
              </div>
              <div class="flex-grow-1">
                <span class="text-muted small fw-semibold text-uppercase">Clientes</span>
                <h3 class="fw-bold mb-0 text-dark">{{ metrics()?.totalClientes ?? 0 }}</h3>
                <span class="badge bg-info-subtle text-info mt-1" style="font-size: 0.7rem;">Registrados</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Card 3: Ventas -->
        <div class="col-12 col-sm-6 col-xl-3">
          <div class="card card-kpi border-0 shadow-sm rounded-4 h-100 kpi-orange">
            <div class="card-body p-3 d-flex align-items-center">
              <div class="kpi-icon bg-warning bg-opacity-10 text-warning rounded-3 me-3">
                🛒
              </div>
              <div class="flex-grow-1">
                <span class="text-muted small fw-semibold text-uppercase">Total Ventas</span>
                <h3 class="fw-bold mb-0 text-dark">{{ metrics()?.totalVentas ?? 0 }}</h3>
                <span class="badge bg-warning-subtle text-warning mt-1" style="font-size: 0.7rem;">Órdenes procesadas</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Card 4: Ingresos -->
        <div class="col-12 col-sm-6 col-xl-3">
          <div class="card card-kpi border-0 shadow-sm rounded-4 h-100 kpi-green">
            <div class="card-body p-3 d-flex align-items-center">
              <div class="kpi-icon bg-success bg-opacity-10 text-success rounded-3 me-3">
                💰
              </div>
              <div class="flex-grow-1">
                <span class="text-muted small fw-semibold text-uppercase">Ingresos Totales</span>
                <h3 class="fw-bold mb-0 text-dark">{{ (metrics()?.montoTotalVentas ?? 0) | currency:'COP':'symbol-narrow':'1.0-0' }}</h3>
                <span class="badge bg-success-subtle text-success mt-1" style="font-size: 0.7rem;">Recaudado</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Logistics / Dispatch Status Bar -->
      <div class="card border-0 shadow-sm rounded-4 mb-4 p-3 bg-white">
        <div class="d-flex justify-content-between align-items-center mb-2">
          <h6 class="fw-bold mb-0 text-dark d-flex align-items-center gap-2">
            <span>🚚</span> Estado Operativo de Despachos
          </h6>
          <span class="text-muted small">Flujo de entregas</span>
        </div>
        <div class="row g-2 text-center">
          <div class="col-md-4">
            <div class="p-2 rounded-3 bg-warning-subtle border border-warning-subtle">
              <span class="small text-warning-emphasis fw-bold">Pendientes</span>
              <h5 class="fw-bold mb-0 text-warning-emphasis">{{ metrics()?.despachosPendientes ?? 0 }}</h5>
            </div>
          </div>
          <div class="col-md-4">
            <div class="p-2 rounded-3 bg-primary-subtle border border-primary-subtle">
              <span class="small text-primary-emphasis fw-bold">En Ruta</span>
              <h5 class="fw-bold mb-0 text-primary-emphasis">{{ metrics()?.despachosEnRuta ?? 0 }}</h5>
            </div>
          </div>
          <div class="col-md-4">
            <div class="p-2 rounded-3 bg-success-subtle border border-success-subtle">
              <span class="small text-success-emphasis fw-bold">Entregados</span>
              <h5 class="fw-bold mb-0 text-success-emphasis">{{ metrics()?.despachosEntregados ?? 0 }}</h5>
            </div>
          </div>
        </div>
      </div>

      <!-- Main Tables Grid -->
      <div class="row g-4">
        <!-- Recent Sales -->
        <div class="col-12 col-lg-7">
          <div class="card border-0 shadow-sm rounded-4 h-100 bg-white">
            <div class="card-header bg-white border-0 pt-3 px-4 d-flex justify-content-between align-items-center">
              <h6 class="fw-bold mb-0 text-dark">📋 Ventas Recientes</h6>
              <a routerLink="/ventas" class="btn btn-link btn-sm text-decoration-none p-0">Ver todas &rarr;</a>
            </div>
            <div class="card-body px-4 pb-4 pt-2">
              <div class="table-responsive">
                <table class="table table-hover align-middle mb-0">
                  <thead class="table-light small text-muted">
                    <tr>
                      <th>Cliente</th>
                      <th>Fecha</th>
                      <th>Total</th>
                      <th>Estado</th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (v of metrics()?.ventasRecientes; track v.id) {
                      <tr>
                        <td class="fw-semibold text-dark">{{ v.clienteNombre }}</td>
                        <td class="text-muted small">{{ v.fecha | date:'dd/MM/yyyy HH:mm' }}</td>
                        <td class="fw-bold text-dark">{{ v.total | currency:'COP':'symbol-narrow':'1.0-0' }}</td>
                        <td>
                          <span class="badge" [ngClass]="{
                            'bg-warning text-dark': v.estadoDespacho === 'Pendiente',
                            'bg-primary': v.estadoDespacho === 'En Ruta',
                            'bg-success': v.estadoDespacho === 'Entregado'
                          }">{{ v.estadoDespacho }}</span>
                        </td>
                      </tr>
                    } @empty {
                      <tr>
                        <td colspan="4" class="text-center text-muted py-3">No hay ventas registradas aún.</td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>

        <!-- Low Stock Alerts -->
        <div class="col-12 col-lg-5">
          <div class="card border-0 shadow-sm rounded-4 h-100 bg-white">
            <div class="card-header bg-white border-0 pt-3 px-4 d-flex justify-content-between align-items-center">
              <h6 class="fw-bold mb-0 text-dark">⚠️ Alertas de Inventario</h6>
              <a routerLink="/productos" class="btn btn-link btn-sm text-decoration-none p-0">Inventario &rarr;</a>
            </div>
            <div class="card-body px-4 pb-4 pt-2">
              <div class="table-responsive">
                <table class="table table-hover align-middle mb-0">
                  <thead class="table-light small text-muted">
                    <tr>
                      <th>Producto</th>
                      <th class="text-center">Stock</th>
                      <th>Precio Unit.</th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (p of metrics()?.productosBajoStock; track p.id) {
                      <tr>
                        <td class="small fw-semibold text-dark">{{ p.nombre }}</td>
                        <td class="text-center">
                          <span class="badge bg-danger rounded-pill px-2">
                            {{ p.stockActual }} {{ p.unidadMedida }}
                          </span>
                        </td>
                        <td class="small text-muted">{{ p.precioUnitario | currency:'COP':'symbol-narrow':'1.0-0' }}</td>
                      </tr>
                    } @empty {
                      <tr>
                        <td colspan="3" class="text-center text-muted py-3">Todos los productos cuentan con stock suficiente.</td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container {
      background-color: #f8fafc;
      min-height: 100vh;
    }
    .card-kpi {
      transition: transform 0.2s ease, box-shadow 0.2s ease;
    }
    .card-kpi:hover {
      transform: translateY(-4px);
      box-shadow: 0 10px 20px rgba(0,0,0,0.08) !important;
    }
    .kpi-icon {
      width: 48px;
      height: 48px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.5rem;
    }
  `]
})
export class DashboardComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  metrics = signal<DashboardMetrics | null>(null);

  ngOnInit(): void {
    this.loadMetrics();
  }

  loadMetrics(): void {
    this.dashboardService.getMetrics().subscribe(data => {
      this.metrics.set(data);
    });
  }
}
