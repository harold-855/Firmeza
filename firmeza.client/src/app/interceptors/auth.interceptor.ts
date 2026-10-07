import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  let modifiedReq = req;

  // Adjuntar token Bearer si existe y no es una solicitud externa
  if (token) {
    modifiedReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(modifiedReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Manejar token expirado o credenciales no válidas (401 Unauthorized)
      if (error.status === 401) {
        console.warn('Sesión expirada o token no autorizado (401). Redirigiendo al login...');
        authService.logoutLocal('expired');
      }
      return throwError(() => error);
    })
  );
};
