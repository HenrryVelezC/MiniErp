import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { CustomerService } from '../../services/customer.service';
import { CustomerRead } from '../../models/customer-read.model';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, RouterModule],
  templateUrl: './customer-list.component.html',
  styleUrls: ['./customer-list.component.scss']
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