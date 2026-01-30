// minierp-web/src/app/orders/orders-list/orders-list.component.ts

// Importa Component/OnInit/inject desde Angular core
// - Component: permite definir un componente Angular
// - OnInit: interfaz para ejecutar código al iniciar el componente
// - inject: inyección de dependencias moderna (sin constructor)
import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';

// CommonModule: necesario para *ngIf, *ngFor, pipes, etc.
import { CommonModule } from '@angular/common';

// Angular Material (módulos visuales que usas en el HTML)
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';

// Servicio de pedidos: aquí están las llamadas HTTP (list/get/create/update/delete)
import { OrdersService } from '../orders.service';

// Router: sirve para navegar a otras rutas (ej: editar)
// AuthService: para saber roles (Admin/Manager/User) desde el JWT
import { Router, RouterLink  } from '@angular/router';
import { AuthService } from '../../auth/auth.service';

@Component({
  // Nombre del componente (cómo se llamaría en HTML si lo usaras como tag)
  selector: 'app-orders-list',

  // Standalone: no necesita AppModule
  standalone: true,

  // Imports que este componente necesita para que el HTML funcione
  imports: [CommonModule, RouterLink, MatTableModule, MatCardModule, MatButtonModule],

  // Archivo HTML asociado
  templateUrl: './orders-list.component.html',

  // Archivo SCSS asociado
  styleUrls: ['./orders-list.component.scss']
})
export class OrdersListComponent implements OnInit {

  private cdr = inject(ChangeDetectorRef);

  // Inyectamos el servicio de Orders (para llamar al backend)
  private ordersService = inject(OrdersService);

  // Inyectamos el Router (para navegar a editar/nuevo)
  private router = inject(Router);

  // Inyectamos AuthService (para revisar roles y permisos)
  private auth = inject(AuthService);

  // Columnas que se muestran en la tabla (deben coincidir con tu HTML)
  displayedColumns: string[] = ['customer', 'createdAt', 'items', 'actions'];

  // Arreglo donde se guardan las órdenes recibidas desde la API
  orders: any[] = [];

  // Bandera para mostrar "cargando..."
  loading = false;

  // Mensaje de error para mostrar cuando el API falla
  errorMessage = '';

  // ngOnInit se ejecuta una vez cuando el componente inicia
  ngOnInit(): void {
    // Cargamos la lista al iniciar
    this.loadOrders();
  }

  /**
   * Carga todas las órdenes desde el backend
   * - Se usa al iniciar y después de eliminar para refrescar la tabla
   */
  loadOrders(): void {
    // Prendemos el indicador de carga
    this.loading = true;

    // Limpiamos error anterior (si lo había)
    this.errorMessage = '';

    // Llamamos al API GET /orders
    this.ordersService.list().subscribe({
      // next: se ejecuta cuando el backend responde bien (HTTP 200)
      next: (res: any[]) => {
        // Guardamos en memoria el listado (para que la tabla lo muestre)
        this.orders = res;

        // Apagamos indicador de carga
        this.loading = false;

        // fuerza a Angular a refrescar la vista
        this.cdr.detectChanges();

      },

      // error: se ejecuta cuando el backend falla (401/403/500/CORS/etc.)
      error: (err: any) => {
        // Mostramos error en consola para depurar
        console.error(err);

        // Apagamos indicador de carga para que no quede pegado
        this.loading = false;

        // Mensaje amigable para el usuario
        this.errorMessage = 'No se pudo cargar el listado.';
      }
    });
  }

  /**
   * Permiso para eliminar:
   * - En tu backend: DELETE es solo Admin
   * - Entonces aquí lo reflejamos para ocultar el botón si no es Admin
   */
  canDelete(): boolean {
    return this.auth.hasRole('Admin');
  }

  /**
   * Navega a la pantalla de edición
   * (cuando tengas el formulario ya creado)
   */
  edit(id: string): void {
    // Navegamos a /app/orders/:id/edit
    this.router.navigate(['/app/orders', id, 'edit']);
  }

  /**
   * Elimina una orden por id
   * - Pide confirmación
   * - Llama al backend DELETE /orders/{id}
   * - Luego recarga la lista
   */
  remove(id: string): void {
    // Si no es Admin, no dejamos borrar (seguridad UI)
    if (!this.canDelete()) {
      alert('Solo un Admin puede eliminar órdenes.');
      return;
    }

    // Confirmación simple
    const ok = confirm('¿Seguro que deseas eliminar esta orden?');
    if (!ok) return;

    // Prendemos indicador de carga
    this.loading = true;

    // Llamamos al backend DELETE /orders/{id}
    this.ordersService.delete(id).subscribe({
      // Si borra bien
      next: () => {
        // Recargamos la lista para reflejar el cambio
        this.loadOrders();
      },

      // Si falla el borrado
      error: (err: any) => {
        console.error(err);
        this.loading = false;
        alert('No se pudo eliminar la orden (¿token o rol Admin?).');
      }
    });
  }

  /**
   * Placeholder de "ver" (si quieres mantenerlo)
   * Puedes convertirlo en editar o en un detalle más adelante.
   */
  view(id: string): void {
    alert(`Ver orden: ${id} (pendiente)`);
  }
}