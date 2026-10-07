import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated() && !authService.isTokenExpired()) {
    return true;
  }

  // Si no está autenticado o el token ya expiró, redirigir al login
  authService.logoutLocal(authService.getToken() ? 'expired' : undefined);
  return router.createUrlTree(['/login'], {
    queryParams: { returnUrl: state.url }
  });
};
