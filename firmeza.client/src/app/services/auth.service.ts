import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, of, finalize } from 'rxjs';
import { AuthResponse, LoginDto, RegisterDto, UserSession } from '../models/auth.model';

const TOKEN_KEY = 'firmeza_jwt_token';
const SESSION_KEY = 'firmeza_user_session';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  // Reactive state using Angular Signals
  readonly currentUser = signal<UserSession | null>(this.getStoredSession());
  readonly isAuthenticated = computed(() => {
    const session = this.currentUser();
    return !!session && !this.isTokenExpired();
  });
  readonly isCliente = computed(() => {
    const session = this.currentUser();
    if (!session) return false;
    return session.role === 'Cliente' || session.roles?.includes('Cliente');
  });

  constructor() {
    // Check token validity on startup
    if (this.currentUser() && this.isTokenExpired()) {
      this.clearSession();
    }
  }

  login(credentials: LoginDto): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/login', credentials).pipe(
      tap(response => {
        this.saveSession(response);
      })
    );
  }

  register(data: RegisterDto): Observable<AuthResponse> {
    const payload: RegisterDto = {
      ...data,
      edad: data.edad != null ? String(data.edad) : '',
      role: 'Cliente' // Client role enforced
    };
    return this.http.post<AuthResponse>('/api/auth/register', payload).pipe(
      tap(response => {
        this.saveSession(response);
      })
    );
  }

  getMe(): Observable<any> {
    return this.http.get('/api/auth/me');
  }

  logout(): Observable<any> {
    return this.http.post('/api/auth/logout', {}).pipe(
      catchError(() => of({ mensaje: 'Sesión local cerrada' })),
      finalize(() => {
        this.clearSession();
        this.router.navigate(['/login']);
      })
    );
  }

  logoutLocal(reason?: string): void {
    this.clearSession();
    const queryParams = reason ? { reason } : undefined;
    this.router.navigate(['/login'], { queryParams });
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY) || sessionStorage.getItem(TOKEN_KEY);
  }

  isTokenExpired(): boolean {
    const token = this.getToken();
    if (!token) return true;

    try {
      const session = this.getStoredSession();
      if (session?.tokenExpiration) {
        const expDate = new Date(session.tokenExpiration).getTime();
        if (Date.now() >= expDate) {
          return true;
        }
      }

      // Check JWT payload expiration claim 'exp'
      const payload = this.decodeJwtPayload(token);
      if (payload && payload.exp) {
        const expSeconds = payload.exp * 1000;
        return Date.now() >= expSeconds;
      }

      return false;
    } catch {
      return true;
    }
  }

  private saveSession(response: AuthResponse): void {
    const session: UserSession = {
      userId: response.userId,
      email: response.email,
      role: response.role,
      roles: response.roles || [response.role],
      token: response.token,
      tokenExpiration: response.tokenExpiration
    };

    try {
      localStorage.setItem(TOKEN_KEY, response.token);
      localStorage.setItem(SESSION_KEY, JSON.stringify(session));
    } catch (e) {
      console.warn('LocalStorage error, falling back to SessionStorage', e);
      sessionStorage.setItem(TOKEN_KEY, response.token);
      sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));
    }

    this.currentUser.set(session);
  }

  private getStoredSession(): UserSession | null {
    try {
      const raw = localStorage.getItem(SESSION_KEY) || sessionStorage.getItem(SESSION_KEY);
      if (!raw) return null;
      return JSON.parse(raw) as UserSession;
    } catch {
      return null;
    }
  }

  private clearSession(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(SESSION_KEY);
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(SESSION_KEY);
    this.currentUser.set(null);
  }

  private decodeJwtPayload(token: string): any {
    try {
      const parts = token.split('.');
      if (parts.length < 2) return null;
      const base64Url = parts[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      return JSON.parse(jsonPayload);
    } catch {
      return null;
    }
  }
}
