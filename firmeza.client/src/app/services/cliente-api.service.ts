import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Producto, ProductoFilter } from '../models/producto.model';
import { CreateVentaDto, VentaDto } from '../models/venta.model';

@Injectable({
  providedIn: 'root'
})
export class ClienteApiService {
  private http = inject(HttpClient);

  /**
   * Consulta el catálogo de productos disponibles para el cliente.
   * Endpoint: GET /api/productos
   */
  getProductos(filter?: ProductoFilter): Observable<Producto[]> {
    let params = new HttpParams();
    if (filter?.searchTerm) {
      params = params.set('searchTerm', filter.searchTerm);
    }
    if (filter?.activo !== undefined) {
      params = params.set('activo', filter.activo.toString());
    }
    if (filter?.soloBajoStock !== undefined) {
      params = params.set('soloBajoStock', filter.soloBajoStock.toString());
    }
    return this.http.get<Producto[]>('/api/productos', { params });
  }

  /**
   * Consulta el detalle de un producto específico.
   * Endpoint: GET /api/productos/{id}
   */
  getProductoById(id: string): Observable<Producto> {
    return this.http.get<Producto>(`/api/productos/${id}`);
  }

  /**
   * Registra una nueva orden de venta o pedido para el cliente autenticado.
   * Endpoint: POST /api/ventas
   */
  crearVenta(dto: CreateVentaDto): Observable<VentaDto> {
    return this.http.post<VentaDto>('/api/ventas', dto);
  }

  /**
   * Consulta la orden de venta por ID para ver su estado y detalles.
   * Endpoint: GET /api/ventas/{id}
   */
  getVentaById(id: string): Observable<VentaDto> {
    return this.http.get<VentaDto>(`/api/ventas/${id}`);
  }

  /**
   * Consulta el listado de pedidos realizados por el cliente autenticado.
   * Endpoint: GET /api/ventas/mis-pedidos
   */
  getMisPedidos(): Observable<VentaDto[]> {
    return this.http.get<VentaDto[]>('/api/ventas/mis-pedidos');
  }

  /**
   * Descarga el comprobante / recibo en formato PDF de una venta efectuada.
   * Endpoint: GET /api/ventas/{id}/recibo
   */
  descargarReciboPdf(id: string): Observable<Blob> {
    return this.http.get(`/api/ventas/${id}/recibo`, {
      responseType: 'blob'
    });
  }
}
