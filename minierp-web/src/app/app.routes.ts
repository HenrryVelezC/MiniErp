// minierp-web/src/app/app.routes.ts
import { Routes } from '@angular/router';
import { AuthGuard } from './auth/auth.guard';

// ojo: ajusta el import del login según tu proyecto
import { LoginComponent } from './auth/login/login.component';


import { ShellComponent } from './layout/shell.component';


export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  // 1) Página pública: SOLO login
  { path: 'login', component: LoginComponent },

  // 2) Todo lo demás requiere estar logueado
  {
    path: 'app',
    component: ShellComponent,
    canActivate: [AuthGuard],
    children: [
      // por ahora: orders, luego aquí meteremos el menú / shell
      {
        path: 'orders',
        loadComponent: () =>
          import('./orders/orders-list/orders-list.component').then(m => m.OrdersListComponent)
      },
      {
        path: 'orders/new',
        loadComponent: () =>
          import('./orders/orders-form/orders-form.component').then(m => m.OrdersFormComponent)
      },
      {
        path: 'orders/:id/edit',
        loadComponent: () =>
          import('./orders/orders-form/orders-form.component').then(m => m.OrdersFormComponent)
      },
      {
        path: 'admin',
        loadComponent: () =>
          import('./admin/admin.component').then(m => m.AdminComponent)
      },

      // cuando ya esté logueado y entre a '/', lo mandamos a orders
      { path: '', redirectTo: 'orders', pathMatch: 'full' }
    ]
  },

  // 3) Cualquier otra ruta manda a login
  { path: '**', redirectTo: 'login' }
];