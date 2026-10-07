// @vitest-environment jsdom
import '@angular/compiler';
import { describe, it, expect, beforeEach } from 'vitest';
import { Injector, runInInjectionContext } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from './services/auth.service';
import { CartService } from './services/cart.service';

describe('AuthService & CartService Unit Tests', () => {
  let authService: AuthService;
  let cartService: CartService;
  let injector: Injector;

  beforeEach(() => {
    localStorage.clear();
    sessionStorage.clear();

    const mockHttp = {
      post: () => {},
      get: () => {}
    };
    const mockRouter = {
      navigate: () => {},
      navigateByUrl: () => {}
    };

    injector = Injector.create({
      providers: [
        { provide: HttpClient, useValue: mockHttp },
        { provide: Router, useValue: mockRouter }
      ]
    });

    runInInjectionContext(injector, () => {
      authService = new AuthService();
    });

    cartService = new CartService();
  });

  it('should identify unauthenticated / expired state by default', () => {
    expect(authService.getToken()).toBeNull();
    expect(authService.isTokenExpired()).toBe(true);
    expect(authService.isAuthenticated()).toBe(false);
  });

  it('should manage cart items and calculate totals correctly', () => {
    const mockProduct = {
      id: 'prod-1',
      nombre: 'Cemento Gris Tipo 1 x 50kg',
      descripcion: 'Cemento de prueba',
      unidadMedida: 'Bolsa',
      precioUnitario: 32000,
      stockActual: 100,
      activo: true
    };

    cartService.addItem(mockProduct, 2);
    expect(cartService.totalCount()).toBe(2);
    expect(cartService.totalAmount()).toBe(64000);
    expect(cartService.subtotalBase()).toBe(Math.round(64000 / 1.19));

    cartService.updateQuantity('prod-1', 5);
    expect(cartService.totalCount()).toBe(5);
    expect(cartService.totalAmount()).toBe(160000);

    cartService.removeItem('prod-1');
    expect(cartService.totalCount()).toBe(0);
    expect(cartService.totalAmount()).toBe(0);
  });

  it('should detect token validity from simulated JWT payload', () => {
    // Simulated valid JWT token with exp in future (1 day ahead)
    const futureExp = Math.floor(Date.now() / 1000) + 86400;
    const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
    const payload = btoa(JSON.stringify({ exp: futureExp, sub: 'user-123', email: 'cliente@firmeza.com' }));
    const dummyToken = `${header}.${payload}.signature`;

    localStorage.setItem('firmeza_jwt_token', dummyToken);
    expect(authService.getToken()).toBe(dummyToken);
    expect(authService.isTokenExpired()).toBe(false);

    // Simulated expired JWT token
    const pastExp = Math.floor(Date.now() / 1000) - 3600;
    const expiredPayload = btoa(JSON.stringify({ exp: pastExp, sub: 'user-123' }));
    const expiredToken = `${header}.${expiredPayload}.signature`;

    localStorage.setItem('firmeza_jwt_token', expiredToken);
    expect(authService.isTokenExpired()).toBe(true);
  });
});
