import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ClienteApiService } from '../../services/cliente-api.service';
import { CartService } from '../../services/cart.service';
import { Producto } from '../../models/producto.model';

@Component({
  selector: 'app-catalogo',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="p-4" style="background-color: #f8fafc; min-height: 100vh;">
      <!-- Alerts -->
      @if (alertMessage()) {
        <div class="alert alert-success alert-dismissible fade show rounded-4 shadow-sm border-0 mb-4" role="alert">
          <i class="bi bi-check-circle-fill me-2"></i>
          <span>{{ alertMessage() }}</span>
          <button type="button" class="btn-close" (click)="alertMessage.set(null)" aria-label="Close"></button>
        </div>
      }

      <!-- Page Header -->
      <div class="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-3">
        <div>
          <h2 class="h3 fw-bold text-dark mb-1">Catálogo de Materiales</h2>
          <p class="text-muted mb-0">Selecciona los productos y materiales de construcción para tu pedido.</p>
        </div>
        <div class="d-flex align-items-center gap-2">
          <a routerLink="/carrito" class="btn btn-primary rounded-pill px-4 py-2 d-flex align-items-center gap-2 shadow-sm">
            <i class="bi bi-cart3"></i>
            <span class="fw-semibold">Ver Carrito</span>
            @if (cartService.totalCount() > 0) {
              <span class="badge bg-white text-primary rounded-pill">{{ cartService.totalCount() }}</span>
            }
          </a>
        </div>
      </div>

      <!-- Search & Filters -->
      <div class="card border-0 shadow-sm rounded-4 bg-white mb-4">
        <div class="card-body p-4">
          <div class="row g-3">
            <div class="col-12 col-md-6">
              <label class="form-label small fw-bold text-muted">Buscar Material</label>
              <div class="input-group">
                <span class="input-group-text bg-light border-end-0 text-muted">
                  <i class="bi bi-search"></i>
                </span>
                <input
                  type="text"
                  [(ngModel)]="searchTerm"
                  placeholder="Buscar por nombre, tipo o unidad..."
                  class="form-control bg-light border-start-0"
                />
              </div>
            </div>

            <div class="col-6 col-md-3">
              <label class="form-label small fw-bold text-muted">Disponibilidad</label>
              <select [(ngModel)]="stockFilter" class="form-select bg-light">
                <option value="todos">Todos los productos</option>
                <option value="disponible">En Stock (&gt; 0)</option>
                <option value="agotado">Agotados</option>
              </select>
            </div>

            <div class="col-6 col-md-3">
              <label class="form-label small fw-bold text-muted">Ordenar Por</label>
              <select [(ngModel)]="sortBy" class="form-select bg-light">
                <option value="nombre_asc">Nombre (A-Z)</option>
                <option value="nombre_desc">Nombre (Z-A)</option>
                <option value="precio_asc">Menor Precio</option>
                <option value="precio_desc">Mayor Precio</option>
              </select>
            </div>
          </div>
        </div>
      </div>

      <!-- Products Grid -->
      @if (isLoading()) {
        <div class="text-center py-5">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Cargando catálogo...</span>
          </div>
          <p class="text-muted mt-2">Cargando materiales disponibles...</p>
        </div>
      } @else {
        <div class="row g-4">
          @for (prod of filteredProductos(); track prod.id) {
            <div class="col-12 col-md-6 col-lg-4 col-xl-3">
              <div class="card h-100 border-0 shadow-sm rounded-4 overflow-hidden product-card bg-white d-flex flex-column">
                <div class="card-body p-4 d-flex flex-column flex-grow-1">
                  <div class="d-flex justify-content-between align-items-start mb-2">
                    <span class="badge bg-light text-secondary border rounded-pill px-3 py-1">
                      {{ prod.unidadMedida }}
                    </span>
                    <span class="badge rounded-pill px-3 py-1" [ngClass]="prod.stockActual > 0 ? 'bg-success-subtle text-success' : 'bg-danger-subtle text-danger'">
                      {{ prod.stockActual > 0 ? 'Stock: ' + prod.stockActual : 'Agotado' }}
                    </span>
                  </div>

                  <h5 class="fw-bold text-dark mb-2 mt-1">{{ prod.nombre }}</h5>
                  <p class="text-muted small mb-3 flex-grow-1" style="min-height: 40px;">
                    {{ prod.descripcion }}
                  </p>

                  <div class="mt-auto pt-3 border-top">
                    <div class="d-flex justify-content-between align-items-baseline mb-3">
                      <span class="text-muted small">Precio Unitario:</span>
                      <span class="h5 fw-bold text-primary mb-0">
                        {{ prod.precioUnitario | currency:'COP':'symbol-narrow':'1.0-0' }}
                      </span>
                    </div>

                    @if (prod.stockActual > 0) {
                      <div class="d-flex gap-2 align-items-center">
                        <div class="input-group input-group-sm" style="max-width: 100px;">
                          <button
                            class="btn btn-outline-secondary"
                            type="button"
                            (click)="decrementQty(prod.id)"
                          >-</button>
                          <input
                            type="number"
                            class="form-control text-center p-0"
                            [value]="getQty(prod.id)"
                            (change)="setQty(prod.id, $event)"
                            min="1"
                            [max]="prod.stockActual"
                          />
                          <button
                            class="btn btn-outline-secondary"
                            type="button"
                            (click)="incrementQty(prod.id, prod.stockActual)"
                          >+</button>
                        </div>

                        <button
                          (click)="addToCart(prod)"
                          class="btn btn-primary btn-sm flex-grow-1 rounded-3 d-flex align-items-center justify-content-center gap-1 py-2"
                        >
                          <i class="bi bi-cart-plus"></i>
                          <span>Agregar</span>
                        </button>
                      </div>
                    } @else {
                      <button class="btn btn-secondary btn-sm w-100 rounded-3" disabled>
                        No disponible
                      </button>
                    }
                  </div>
                </div>
              </div>
            </div>
          } @empty {
            <div class="col-12 text-center py-5">
              <div class="text-muted fs-1 mb-2">📦</div>
              <h5 class="text-secondary">No se encontraron productos</h5>
              <p class="text-muted small">Intenta ajustar tus criterios de búsqueda o filtros.</p>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .product-card {
      transition: transform 0.2s ease, box-shadow 0.2s ease;
    }
    .product-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 12px 24px rgba(0, 0, 0, 0.08) !important;
    }
  `]
})
export class CatalogoComponent implements OnInit {
  private clienteApi = inject(ClienteApiService);
  readonly cartService = inject(CartService);

  productos = signal<Producto[]>([]);
  isLoading = signal(true);
  alertMessage = signal<string | null>(null);

  searchTerm = '';
  stockFilter = 'todos';
  sortBy = 'nombre_asc';

  quantities: { [productoId: string]: number } = {};

  ngOnInit(): void {
    this.loadProductos();
  }

  loadProductos(): void {
    this.isLoading.set(true);
    this.clienteApi.getProductos().subscribe({
      next: (data) => {
        this.productos.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error cargando catálogo:', err);
        this.isLoading.set(false);
      }
    });
  }

  getQty(prodId: string): number {
    return this.quantities[prodId] || 1;
  }

  setQty(prodId: string, event: any): void {
    const val = parseInt(event.target.value, 10);
    if (!isNaN(val) && val >= 1) {
      this.quantities[prodId] = val;
    }
  }

  incrementQty(prodId: string, maxStock: number): void {
    const current = this.getQty(prodId);
    if (current < maxStock) {
      this.quantities[prodId] = current + 1;
    }
  }

  decrementQty(prodId: string): void {
    const current = this.getQty(prodId);
    if (current > 1) {
      this.quantities[prodId] = current - 1;
    }
  }

  addToCart(prod: Producto): void {
    const qty = this.getQty(prod.id);
    this.cartService.addItem(prod, qty);
    this.alertMessage.set(`Se agregaron ${qty} unidad(es) de "${prod.nombre}" al carrito.`);
    this.quantities[prod.id] = 1;

    setTimeout(() => {
      this.alertMessage.set(null);
    }, 3500);
  }

  filteredProductos(): Producto[] {
    let list = [...this.productos()];

    if (this.searchTerm.trim()) {
      const term = this.searchTerm.toLowerCase().trim();
      list = list.filter(p =>
        p.nombre.toLowerCase().includes(term) ||
        p.descripcion?.toLowerCase().includes(term) ||
        p.unidadMedida?.toLowerCase().includes(term)
      );
    }

    if (this.stockFilter === 'disponible') {
      list = list.filter(p => p.stockActual > 0);
    } else if (this.stockFilter === 'agotado') {
      list = list.filter(p => p.stockActual <= 0);
    }

    list.sort((a, b) => {
      switch (this.sortBy) {
        case 'nombre_desc': return b.nombre.localeCompare(a.nombre);
        case 'precio_asc': return a.precioUnitario - b.precioUnitario;
        case 'precio_desc': return b.precioUnitario - a.precioUnitario;
        case 'nombre_asc':
        default:
          return a.nombre.localeCompare(b.nombre);
      }
    });

    return list;
  }
}
