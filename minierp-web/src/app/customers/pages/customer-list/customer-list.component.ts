import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { CustomerService } from '../../services/customer.service';
import { CustomerRead } from '../../models/customer-read.model';

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './customer-list.component.html'
})
export class CustomerListComponent {
  customers$: Observable<CustomerRead[]>;
  error: string | null = null;

  constructor(private customerService: CustomerService) {
    this.customers$ = this.customerService.getAll().pipe(
      catchError(err => {
        console.error('Error fetching customers:', err);
        this.error = 'No se pudo cargar customers.';
        return of([] as CustomerRead[]);
      })
    );
  }
}