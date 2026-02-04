import { Component, OnInit } from '@angular/core';
import { CustomerService } from '../../services/customer.service';
import { CustomerRead } from '../../models/customer-read.model';
import { NgIf } from "../../../../../node_modules/@angular/common/types/_common_module-chunk";
import { CommonModule } from '@angular/common';


@Component({
  selector: 'app-customer-list',
  templateUrl: './customer-list.component.html',
  imports: [NgIf, CommonModule]
})
export class CustomerListComponent implements OnInit {
  customers: CustomerRead[] = [];
  loading = true;

  constructor(private customerService: CustomerService) {}

  ngOnInit(): void {
    this.customerService.getAll().subscribe({
      next: (data) => {
        this.customers = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error fetching customers:', error);
        this.loading = false;
      }
    });
  }

}
