import { Injectable, computed, signal } from '@angular/core';
import { Producto } from '../models/producto.model';
import { CartItem } from '../models/venta.model';

const CART_KEY = 'firmeza_cart_items';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  readonly items = signal<CartItem[]>(this.getStoredCart());

  readonly totalCount = computed(() => {
    return this.items().reduce((sum, item) => sum + item.cantidad, 0);
  });

  readonly totalAmount = computed(() => {
    return this.items().reduce((sum, item) => sum + (item.producto.precioUnitario * item.cantidad), 0);
  });

  readonly subtotalBase = computed(() => {
    return Math.round(this.totalAmount() / 1.19);
  });

  readonly ivaAmount = computed(() => {
    return this.totalAmount() - this.subtotalBase();
  });

  addItem(producto: Producto, cantidad: number = 1): void {
    if (cantidad <= 0) return;
    const current = [...this.items()];
    const index = current.findIndex(i => i.producto.id === producto.id);

    if (index >= 0) {
      current[index] = {
        ...current[index],
        cantidad: current[index].cantidad + cantidad
      };
    } else {
      current.push({ producto, cantidad });
    }

    this.saveCart(current);
  }

  updateQuantity(productoId: string, cantidad: number): void {
    if (cantidad <= 0) {
      this.removeItem(productoId);
      return;
    }

    const current = this.items().map(i => {
      if (i.producto.id === productoId) {
        return { ...i, cantidad };
      }
      return i;
    });

    this.saveCart(current);
  }

  removeItem(productoId: string): void {
    const current = this.items().filter(i => i.producto.id !== productoId);
    this.saveCart(current);
  }

  clearCart(): void {
    this.saveCart([]);
  }

  private saveCart(items: CartItem[]): void {
    this.items.set(items);
    try {
      localStorage.setItem(CART_KEY, JSON.stringify(items));
    } catch {
      // Ignore storage errors
    }
  }

  private getStoredCart(): CartItem[] {
    try {
      const raw = localStorage.getItem(CART_KEY);
      return raw ? JSON.parse(raw) : [];
    } catch {
      return [];
    }
  }
}
