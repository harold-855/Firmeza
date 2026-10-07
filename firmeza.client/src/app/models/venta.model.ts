import { Producto } from './producto.model';

export interface VentaDetalleDto {
  id: string;
  productoId: string;
  productoNombre: string;
  unidadMedida: string;
  cantidad: number;
  precioAplicado: number;
  subtotal: number;
}

export interface VentaDto {
  id: string;
  numeroComprobante: string;
  fechaVenta: string;
  total: number;
  subtotalBase: number;
  iva: number;
  estadoDespacho: string;
  clienteId: string;
  clienteRazonSocial: string;
  clienteDocumento: string;
  clienteTelefono: string;
  clienteDireccion: string;
  clienteEmail: string;
  detalles: VentaDetalleDto[];
  totalItems: number;
  rutaArchivoRecibo?: string;
}

export interface CreateVentaDetalleDto {
  productoId: string;
  cantidad: number;
  precioAplicado?: number;
}

export interface CreateVentaDto {
  clienteId: string;
  estadoDespacho?: string;
  clienteEmail?: string;
  detalles: CreateVentaDetalleDto[];
}

export interface CartItem {
  producto: Producto;
  cantidad: number;
}
