// src/app/auth/role.guard.ts
import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, Router } from '@angular/router';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class RoleGuard implements CanActivate {

  constructor(private auth: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    const required = (route.data['roles'] as string[]) || [];
    const ok = required.every(r => this.auth.hasRole(r));

    if (!ok) this.router.navigate(['/orders']);             // Redirige si no cumple roles
    return ok;
  }
}
