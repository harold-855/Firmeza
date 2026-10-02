import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { DashboardMetrics, Producto, Cliente, Venta } from '../models/dashboard.model';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private http = inject(HttpClient);

  getMetrics(): Observable<DashboardMetrics> {
    return this.http.get<DashboardMetrics>('/api/dashboard/metrics').pipe(
      catchError(error => {
        console.warn('Usando datos de respaldo para métricas:', error);
        return of({
          totalProductos: 4,
          totalClientes: 3,
          totalVentas: 3,
          montoTotalVentas: 5135000,
          despachosPendientes: 1,
          despachosEnRuta: 1,
          despachosEntregados: 1,
          ventasRecientes: [
            { id: '1', clienteNombre: 'Inversiones Horizonte Ltda.', fecha: new Date().toISOString(), total: 1285000, estadoDespacho: 'Pendiente' },
            { id: '2', clienteNombre: 'Ferretería El Progreso', fecha: new Date(Date.now() - 86400000).toISOString(), total: 2250000, estadoDespacho: 'En Ruta' },
            { id: '3', clienteNombre: 'Constructora Los Andes S.A.S.', fecha: new Date(Date.now() - 172800000).toISOString(), total: 1600000, estadoDespacho: 'Entregado' }
          ],
          productosBajoStock: [
            { id: '1', nombre: 'Ladrillo Estructurado Arcilla 10x20x40', stockActual: 12, precioUnitario: 1200000, unidadMedida: 'Millar' },
            { id: '2', nombre: 'Arena Lavada de Río (M3)', stockActual: 35, precioUnitario: 85000, unidadMedida: 'M3' }
          ]
        });
      })
    );
  }

  getProductos(): Observable<Producto[]> {
    return this.http.get<Producto[]>('/api/productos').pipe(
      catchError(error => {
        console.warn('Usando datos de respaldo para productos:', error);
        return of([
          { id: '1', nombre: 'Cemento Gris Tipo 1 x 50kg', descripcion: 'Cemento de uso general', unidadMedida: 'Bolsa', precioUnitario: 32000, stockActual: 150, activo: true },
          { id: '2', nombre: 'Varilla Corrugada 1/2 pulgada', descripcion: 'Acero de refuerzo NTC 2289', unidadMedida: 'Unidad', precioUnitario: 45000, stockActual: 80, activo: true },
          { id: '3', nombre: 'Ladrillo Estructurado Arcilla 10x20x40', descripcion: 'Ladrillo cocido estructural', unidadMedida: 'Millar', precioUnitario: 1200000, stockActual: 12, activo: true },
          { id: '4', nombre: 'Arena Lavada de Río (M3)', descripcion: 'Agregado fino para mezcla', unidadMedida: 'M3', precioUnitario: 85000, stockActual: 35, activo: true }
        ]);
      })
    );
  }

  getClientes(): Observable<Cliente[]> {
    return this.http.get<Cliente[]>('/api/clientes').pipe(
      catchError(error => {
        console.warn('Usando datos de respaldo para clientes:', error);
        return of([
          { id: '1', razonSocial: 'Constructora Los Andes S.A.S.', documentoIdentidad: '900123456-1', telefono: '3001234567', email: 'compras@losandes.com', direccionEnvio: 'Calle 100 # 15-20, Bogotá', totalCompras: 1 },
          { id: '2', razonSocial: 'Ferretería El Progreso', documentoIdentidad: '800987654-3', telefono: '3109876543', email: 'contacto@elprogreso.com', direccionEnvio: 'Carrera 45 # 10-30, Medellín', totalCompras: 1 },
          { id: '3', razonSocial: 'Inversiones Horizonte Ltda.', documentoIdentidad: '901234567-8', telefono: '3204567890', email: 'gerencia@horizonte.com', direccionEnvio: 'Avenida 6N # 25-10, Cali', totalCompras: 1 }
        ]);
      })
    );
  }

  getVentas(): Observable<Venta[]> {
    return this.http.get<Venta[]>('/api/ventas').pipe(
      catchError(error => {
        console.warn('Usando datos de respaldo para ventas:', error);
        return of([
          { id: '1', cliente: 'Inversiones Horizonte Ltda.', documentoCliente: '901234567-8', fecha: new Date().toISOString(), total: 1285000, estadoDespacho: 'Pendiente', totalItems: 2 },
          { id: '2', cliente: 'Ferretería El Progreso', documentoCliente: '800987654-3', fecha: new Date(Date.now() - 86400000).toISOString(), total: 2250000, estadoDespacho: 'En Ruta', totalItems: 1 },
          { id: '3', cliente: 'Constructora Los Andes S.A.S.', documentoCliente: '900123456-1', fecha: new Date(Date.now() - 172800000).toISOString(), total: 1600000, estadoDespacho: 'Entregado', totalItems: 1 }
        ]);
      })
    );
  }
}
