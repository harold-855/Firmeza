export interface VentaResumen {
  id: string;
  clienteNombre: string;
  fecha: string;
  total: number;
  estadoDespacho: string;
}

export interface ProductoResumen {
  id: string;
  nombre: string;
  stockActual: number;
  precioUnitario: number;
  unidadMedida: string;
}

export interface DashboardMetrics {
  totalProductos: number;
  totalClientes: number;
  totalVentas: number;
  montoTotalVentas: number;
  despachosPendientes: number;
  despachosEnRuta: number;
  despachosEntregados: number;
  ventasRecientes: VentaResumen[];
  productosBajoStock: ProductoResumen[];
}

export interface Producto {
  id: string;
  nombre: string;
  descripcion: string;
  unidadMedida: string;
  precioUnitario: number;
  stockActual: number;
  activo: boolean;
  totalVentasAsociadas?: number;
}

export interface CreateProductoDto {
  nombre: string;
  descripcion: string;
  unidadMedida: string;
  precioUnitario: number;
  stockActual: number;
  activo: boolean;
}

export interface UpdateProductoDto {
  id: string;
  nombre: string;
  descripcion: string;
  unidadMedida: string;
  precioUnitario: number;
  stockActual: number;
  activo: boolean;
}


export interface Cliente {
  id: string;
  razonSocial: string;
  documentoIdentidad: string;
  telefono: string;
  email: string;
  direccionEnvio: string;
  totalCompras: number;
  montoTotalComprado?: number;
}

export interface CreateClienteDto {
  documentoIdentidad: string;
  razonSocial: string;
  telefono: string;
  direccionEnvio: string;
  email: string;
}

export interface UpdateClienteDto {
  id: string;
  documentoIdentidad: string;
  razonSocial: string;
  telefono: string;
  direccionEnvio: string;
  email: string;
}

export interface Venta {
  id: string;
  cliente: string;
  documentoCliente: string;
  fecha: string;
  total: number;
  estadoDespacho: string;
  totalItems: number;
}
