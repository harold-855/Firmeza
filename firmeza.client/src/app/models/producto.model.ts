export interface Producto {
  id: string;
  nombre: string;
  descripcion: string;
  unidadMedida: string;
  precioUnitario: number;
  stockActual: number;
  activo: boolean;
}

export interface ProductoFilter {
  searchTerm?: string;
  activo?: boolean;
  soloBajoStock?: boolean;
  minPrecio?: number;
  maxPrecio?: number;
}
