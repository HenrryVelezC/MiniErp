// src/app/orders/orders.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

// Tipos para crear/editar (alineados con tu API)
export interface OrderItemUpsert { productName: string; quantity: number; unitPrice: number; }
export interface OrderUpsert { customerName: string; items: OrderItemUpsert[]; }

@Injectable({ providedIn: 'root' })
export class OrdersService {

  constructor(private http: HttpClient) {}

  list() {
    return this.http.get<any[]>(`${environment.apiUrl}/orders`);
  }

  get(id: string) {
    return this.http.get<any>(`${environment.apiUrl}/orders/${id}`);
  }

  create(dto: OrderUpsert) {
    return this.http.post<any>(`${environment.apiUrl}/orders`, dto);
  }

  update(id: string, dto: OrderUpsert) {
    return this.http.put<void>(`${environment.apiUrl}/orders/${id}`, dto);
  }

  delete(id: string) {
    return this.http.delete<void>(`${environment.apiUrl}/orders/${id}`);
  }
}