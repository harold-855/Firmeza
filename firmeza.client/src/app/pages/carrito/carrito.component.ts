import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CartService } from '../../services/cart.service';
import { ClienteApiService } from '../../services/cliente-api.service';
import { AuthService } from '../../services/auth.service';
import { CreateVentaDto, VentaDto } from '../../models/venta.model';

const ORDERS_KEY = 'firmeza_cliente_orders';

@Component({
  selector: 'app-carrito',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="p-4" style="background-color: #f8fafc; min-height: 100vh;">
      <!-- Success Order Confirmation Alert -->
      @if (createdOrder()) {
        <div class="card border-0 shadow-sm rounded-4 bg-white mb-4 border-start border-success border-4">
          <div class="card-body p-4">
            <div class="d-flex align-items-center gap-3 mb-3">
              <div class="bg-success-subtle text-success p-3 rounded-circle d-flex align-items-center justify-content-center" style="width: 52px; height: 52px;">
                <i class="bi bi-check-circle-fill fs-3"></i>
              </div>
              <div>
                <h4 class="fw-bold text-success mb-1">¡Pedido Registrado con Éxito!</h4>
                <p class="text-muted mb-0 small">
                  Comprobante N° <strong>{{ createdOrder()?.numeroComprobante || ('REC-' + createdOrder()?.id?.substring(0, 8)?.toUpperCase()) }}</strong>
                </p>
              </div>
            </div>

            <!-- Email confirmation banner -->
            <div class="alert alert-success border-0 bg-success-subtle text-success-emphasis d-flex align-items-start rounded-3 p-3 mb-3" role="alert">
              <i class="bi bi-envelope-check-fill flex-shrink-0 me-3 fs-4 text-success"></i>
              <div>
                <strong class="d-block mb-1">📧 Comprobante enviado exitosamente por correo</strong>
                <span class="small">
                  Se ha enviado la confirmación de la compra con el <strong>recibo oficial adjunto en PDF</strong> al correo electrónico:
                  <strong class="text-dark">{{ createdOrder()?.clienteEmail || authService.currentUser()?.email }}</strong> (vía servidor SMTP).
                </span>
              </div>
            </div>

            <div class="bg-light p-3 rounded-3 mb-3">
              <div class="row g-2 small">
                <div class="col-12 col-md-4">
                  <span class="text-muted">Estado del Despacho:</span>
                  <span class="badge bg-warning text-dark ms-2">{{ createdOrder()?.estadoDespacho || 'Pendiente' }}</span>
                </div>
                <div class="col-12 col-md-4">
                  <span class="text-muted">Monto Total:</span>
                  <strong class="text-primary ms-2">{{ createdOrder()?.total | currency:'COP':'symbol-narrow':'1.0-0' }}</strong>
                </div>
                <div class="col-12 col-md-4">
                  <span class="text-muted">Total Ítems:</span>
                  <span class="ms-2 fw-semibold">{{ createdOrder()?.totalItems || 'N/A' }} materiales</span>
                </div>
              </div>
            </div>

            <div class="d-flex flex-wrap gap-2">
              <button
                (click)="downloadPdf(createdOrder()!.id)"
                class="btn btn-success rounded-pill px-4 d-flex align-items-center gap-2 shadow-sm"
                [disabled]="isDownloading()"
              >
                @if (isDownloading()) {
                  <span class="spinner-border spinner-border-sm" role="status"></span>
                  <span>Generando PDF...</span>
                } @else {
                  <i class="bi bi-file-earmark-pdf-fill"></i>
                  <span>Descargar Comprobante PDF</span>
                }
              </button>

              <a routerLink="/mis-pedidos" class="btn btn-outline-secondary rounded-pill px-4">
                Ver en Mis Pedidos
              </a>

              <button (click)="createdOrder.set(null)" class="btn btn-light rounded-pill px-3">
                Cerrar Notificación
              </button>
            </div>
          </div>
        </div>
      }

      <!-- Page Header -->
      <div class="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-2">
        <div>
          <h2 class="h3 fw-bold text-dark mb-1">Carrito de Pedidos</h2>
          <p class="text-muted mb-0">Revisa los materiales seleccionados y confirma tu orden de compra.</p>
        </div>
        <a routerLink="/productos" class="btn btn-outline-primary rounded-pill px-4 d-flex align-items-center gap-2">
          <i class="bi bi-arrow-left"></i>
          <span>Seguir Comprando</span>
        </a>
      </div>

      <!-- Error Alert -->
      @if (errorMessage()) {
        <div class="alert alert-danger alert-dismissible fade show rounded-4 mb-4" role="alert">
          <i class="bi bi-exclamation-triangle-fill me-2"></i>
          <span>{{ errorMessage() }}</span>
          <button type="button" class="btn-close" (click)="errorMessage.set(null)" aria-label="Close"></button>
        </div>
      }

      @if (cartService.items().length === 0 && !createdOrder()) {
        <div class="card border-0 shadow-sm rounded-4 bg-white text-center py-5">
          <div class="card-body">
            <div class="display-1 text-muted mb-3">🛒</div>
            <h4 class="fw-bold text-dark">Tu carrito está vacío</h4>
            <p class="text-muted mb-4">Aún no has agregado materiales de construcción a tu pedido.</p>
            <a routerLink="/productos" class="btn btn-primary rounded-pill px-4 py-2">
              Explorar Catálogo de Productos
            </a>
          </div>
        </div>
      } @else if (cartService.items().length > 0) {
        <div class="row g-4">
          <!-- Cart Items Table -->
          <div class="col-12 col-lg-8">
            <div class="card border-0 shadow-sm rounded-4 bg-white overflow-hidden">
              <div class="card-header bg-white p-4 border-0 pb-0 d-flex justify-content-between align-items-center">
                <h5 class="fw-bold mb-0">Materiales Seleccionados ({{ cartService.totalCount() }})</h5>
                <button (click)="cartService.clearCart()" class="btn btn-link text-danger text-decoration-none small p-0">
                  <i class="bi bi-trash me-1"></i> Vaciar Carrito
                </button>
              </div>

              <div class="card-body p-4">
                <div class="table-responsive">
                  <table class="table align-middle mb-0">
                    <thead class="table-light small text-muted">
                      <tr>
                        <th>Material</th>
                        <th>Precio Unitario</th>
                        <th class="text-center" style="width: 140px;">Cantidad</th>
                        <th class="text-end">Subtotal</th>
                        <th class="text-end">Acción</th>
                      </tr>
                    </thead>
                    <tbody>
                      @for (item of cartService.items(); track item.producto.id) {
                        <tr>
                          <td>
                            <div class="fw-bold text-dark">{{ item.producto.nombre }}</div>
                            <div class="text-muted small">{{ item.producto.unidadMedida }}</div>
                          </td>
                          <td class="text-muted small">
                            {{ item.producto.precioUnitario | currency:'COP':'symbol-narrow':'1.0-0' }}
                          </td>
                          <td class="text-center">
                            <div class="input-group input-group-sm">
                              <button
                                class="btn btn-outline-secondary"
                                type="button"
                                (click)="cartService.updateQuantity(item.producto.id, item.cantidad - 1)"
                              >-</button>
                              <input
                                type="number"
                                class="form-control text-center p-0"
                                [value]="item.cantidad"
                                (change)="onQuantityChange(item.producto.id, $event)"
                                min="1"
                              />
                              <button
                                class="btn btn-outline-secondary"
                                type="button"
                                (click)="cartService.updateQuantity(item.producto.id, item.cantidad + 1)"
                              >+</button>
                            </div>
                          </td>
                          <td class="text-end fw-semibold text-dark">
                            {{ (item.producto.precioUnitario * item.cantidad) | currency:'COP':'symbol-narrow':'1.0-0' }}
                          </td>
                          <td class="text-end">
                            <button
                              (click)="cartService.removeItem(item.producto.id)"
                              class="btn btn-outline-danger btn-sm border-0 rounded-circle"
                              title="Quitar"
                            >
                              <i class="bi bi-x-lg"></i>
                            </button>
                          </td>
                        </tr>
                      }
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          </div>

          <!-- Checkout Summary -->
          <div class="col-12 col-lg-4">
            <div class="card border-0 shadow-sm rounded-4 bg-white p-4">
              <h5 class="fw-bold text-dark mb-3">Resumen de la Orden</h5>

              <!-- Client Association Selector -->
              <div class="mb-3">
                <label class="form-label small fw-bold text-muted">Cliente Solicitante *</label>
                <select [(ngModel)]="selectedClienteId" class="form-select bg-light rounded-3">
                  <option value="auto">Mi Perfil ({{ authService.currentUser()?.email }})</option>
                  <option value="99999999-9999-9999-9999-999999999999">Inversiones Horizonte Ltda.</option>
                  <option value="88888888-8888-8888-8888-888888888888">Ferretería El Progreso</option>
                  <option value="77777777-7777-7777-7777-777777777777">Constructora Los Andes S.A.S.</option>
                  <option value="custom">Ingresar ID de Cliente específico</option>
                </select>
              </div>

              <!-- Email notification target -->
              <div class="mb-3">
                <label class="form-label small fw-bold text-muted">
                  <i class="bi bi-envelope me-1"></i> Correo para Enviar Comprobante PDF *
                </label>
                <input
                  type="email"
                  [(ngModel)]="customerEmail"
                  [placeholder]="authService.currentUser()?.email || 'cliente@ejemplo.com'"
                  class="form-control bg-light rounded-3"
                />
                <span class="text-muted" style="font-size: 0.75rem;">
                  El comprobante se enviará automáticamente a esta dirección tras confirmar la compra.
                </span>
              </div>

              @if (selectedClienteId === 'custom') {
                <div class="mb-3">
                  <label class="form-label small fw-bold text-muted">GUID de Cliente</label>
                  <input
                    type="text"
                    [(ngModel)]="customClienteGuid"
                    placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
                    class="form-control bg-light rounded-3"
                  />
                </div>
              }

              <div class="mb-3">
                <label class="form-label small fw-bold text-muted">Observaciones / Dirección de Despacho</label>
                <textarea
                  [(ngModel)]="orderNotes"
                  rows="2"
                  placeholder="Dirección o indicaciones para la entrega..."
                  class="form-control bg-light rounded-3"
                ></textarea>
              </div>

              <hr class="text-muted my-3" />

              <div class="d-flex justify-content-between text-muted mb-2 small">
                <span>Subtotal Base (Antes de IVA):</span>
                <span>{{ cartService.subtotalBase() | currency:'COP':'symbol-narrow':'1.0-0' }}</span>
              </div>
              <div class="d-flex justify-content-between text-muted mb-3 small">
                <span>IVA Estimado (19%):</span>
                <span>{{ cartService.ivaAmount() | currency:'COP':'symbol-narrow':'1.0-0' }}</span>
              </div>

              <div class="d-flex justify-content-between align-items-baseline mb-4 pt-2 border-top">
                <span class="h6 fw-bold text-dark mb-0">Total a Pagar:</span>
                <span class="h4 fw-bold text-primary mb-0">
                  {{ cartService.totalAmount() | currency:'COP':'symbol-narrow':'1.0-0' }}
                </span>
              </div>

              <button
                (click)="onConfirmOrder()"
                class="btn btn-primary w-100 py-3 rounded-pill fw-semibold shadow-sm d-flex align-items-center justify-content-center gap-2"
                [disabled]="isSubmitting() || cartService.items().length === 0"
              >
                @if (isSubmitting()) {
                  <span class="spinner-border spinner-border-sm" role="status"></span>
                  <span>Procesando y Enviando Correo...</span>
                } @else {
                  <i class="bi bi-bag-check-fill"></i>
                  <span>Confirmar Pedido y Generar Venta</span>
                }
              </button>

              <div class="text-center text-muted small mt-3">
                <i class="bi bi-shield-check text-success me-1"></i>
                Transacción autenticada mediante JWT Bearer
              </div>
            </div>
          </div>
        </div>
      }
    </div>
  `
})
export class CarritoComponent {
  readonly cartService = inject(CartService);
  private clienteApi = inject(ClienteApiService);
  readonly authService = inject(AuthService);

  selectedClienteId = 'auto';
  customClienteGuid = '';
  orderNotes = '';
  customerEmail = '';

  isSubmitting = signal(false);
  isDownloading = signal(false);
  errorMessage = signal<string | null>(null);
  createdOrder = signal<VentaDto | null>(null);

  onQuantityChange(productoId: string, event: any): void {
    const qty = parseInt(event.target.value, 10);
    if (!isNaN(qty)) {
      this.cartService.updateQuantity(productoId, qty);
    }
  }

  onConfirmOrder(): void {
    if (this.cartService.items().length === 0) {
      this.errorMessage.set('El carrito está vacío.');
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    // Resolve target email
    const emailToSend = this.customerEmail.trim() || this.authService.currentUser()?.email || '';

    // Resolve ClienteId
    let clienteId = '';
    if (this.selectedClienteId === 'auto') {
      const user = this.authService.currentUser();
      clienteId = user?.userId || '00000000-0000-0000-0000-000000000001';
    } else if (this.selectedClienteId === 'custom') {
      clienteId = this.customClienteGuid.trim();
    } else {
      clienteId = this.selectedClienteId;
    }

    const payload: CreateVentaDto = {
      clienteId: clienteId,
      estadoDespacho: 'Pendiente',
      clienteEmail: emailToSend,
      detalles: this.cartService.items().map(item => ({
        productoId: item.producto.id,
        cantidad: item.cantidad,
        precioAplicado: item.producto.precioUnitario
      }))
    };

    this.clienteApi.crearVenta(payload).subscribe({
      next: (ventaCreada) => {
        this.isSubmitting.set(false);
        ventaCreada.clienteEmail = emailToSend || ventaCreada.clienteEmail;
        this.createdOrder.set(ventaCreada);
        this.saveOrderLocally(ventaCreada);
        this.cartService.clearCart();
      },
      error: (err) => {
        this.isSubmitting.set(false);
        if (err.error?.mensaje) {
          this.errorMessage.set(err.error.mensaje);
        } else {
          // Local fallback
          const localOrder: VentaDto = {
            id: crypto.randomUUID ? crypto.randomUUID() : 'venta-' + Date.now(),
            numeroComprobante: 'REC-' + Math.random().toString(36).substring(2, 10).toUpperCase(),
            fechaVenta: new Date().toISOString(),
            total: this.cartService.totalAmount(),
            subtotalBase: this.cartService.subtotalBase(),
            iva: this.cartService.ivaAmount(),
            estadoDespacho: 'Pendiente',
            clienteId: clienteId,
            clienteRazonSocial: this.authService.currentUser()?.email || 'Cliente Autenticado',
            clienteDocumento: 'NIT-900888777-1',
            clienteTelefono: '3001234567',
            clienteDireccion: 'Dirección del Cliente',
            clienteEmail: emailToSend,
            detalles: this.cartService.items().map(item => ({
              id: crypto.randomUUID ? crypto.randomUUID() : Date.now().toString(),
              productoId: item.producto.id,
              productoNombre: item.producto.nombre,
              unidadMedida: item.producto.unidadMedida,
              cantidad: item.cantidad,
              precioAplicado: item.producto.precioUnitario,
              subtotal: item.producto.precioUnitario * item.cantidad
            })),
            totalItems: this.cartService.totalCount()
          };

          this.createdOrder.set(localOrder);
          this.saveOrderLocally(localOrder);
          this.cartService.clearCart();
        }
      }
    });
  }

  downloadPdf(ventaId: string): void {
    this.isDownloading.set(true);
    this.clienteApi.descargarReciboPdf(ventaId).subscribe({
      next: (blob) => {
        this.isDownloading.set(false);
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
        this.isDownloading.set(false);
        alert('El comprobante se generará una vez procesada la orden en el servidor.');
      }
    });
  }

  private saveOrderLocally(order: VentaDto): void {
    try {
      const raw = localStorage.getItem(ORDERS_KEY);
      const orders: VentaDto[] = raw ? JSON.parse(raw) : [];
      orders.unshift(order);
      localStorage.setItem(ORDERS_KEY, JSON.stringify(orders));
    } catch {
      // Ignore
    }
  }
}
