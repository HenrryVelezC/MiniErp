// minierp-web/src/app/auth/auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

type LoginDto = { email: string; password: string };
type LoginResponse = { token: string };

@Injectable({ providedIn: 'root' })
export class AuthService {
  private tokenKey = 'minierp_token';

  constructor(private http: HttpClient) {}

  // ---------- API ----------
  login(dto: LoginDto) {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, dto);
  }

  // ---------- Token storage ----------
  saveToken(token: string) {
    localStorage.setItem(this.tokenKey, token);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  // ✅ Esto es lo que tu interceptor está buscando: auth.token
  get token(): string | null {
    return this.getToken();
  }

  // ✅ Para el guard
  isAuthenticated(): boolean {
    return !!this.token;
  }

  // ---------- Roles from JWT ----------
  get roles(): string[] {
    const t = this.token;
    if (!t) return [];

    try {
      // payload JWT = parte del medio: header.payload.signature
      const payload = JSON.parse(atob(t.split('.')[1]));
      const role = payload['role'] ?? payload['roles'];

      // puede venir como string o como array
      if (Array.isArray(role)) return role;
      if (typeof role === 'string') return [role];

      return [];
    } catch {
      return [];
    }
  }

  // ✅ Esto es lo que tu RoleGuard está buscando: auth.hasRole(...)
  hasRole(role: string): boolean {
    return this.roles.includes(role);
  }

  // ---------- Logout ----------
  logout() {
    localStorage.clear();
    sessionStorage.clear();
  }
}