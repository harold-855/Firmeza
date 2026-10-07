export interface LoginDto {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface RegisterDto {
  email: string;
  edad: string;
  password: string;
  confirmPassword: string;
  role?: string;
}

export interface AuthResponse {
  mensaje: string;
  token: string;
  tokenExpiration: string;
  userId: string;
  email: string;
  role: string;
  roles: string[];
}

export interface UserSession {
  userId: string;
  email: string;
  role: string;
  roles: string[];
  token: string;
  tokenExpiration: string;
}
