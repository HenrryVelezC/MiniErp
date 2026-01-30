import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { OrdersService } from '../orders.service';

// Material
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-orders-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './orders-form.component.html',
  styleUrls: ['./orders-form.component.scss']
})
export class OrdersFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private ordersService = inject(OrdersService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  loading = false;
  isEdit = false;
  orderId: string | null = null;

  form = this.fb.group({
    customerName: ['', [Validators.required, Validators.maxLength(200)]],
    items: this.fb.array([])
  });

  get items(): FormArray {
    return this.form.get('items') as FormArray;
  }

  ngOnInit(): void {
    this.orderId = this.route.snapshot.paramMap.get('id');
    this.isEdit = !!this.orderId;

    // Si es nuevo, arranca con 1 item
    if (!this.isEdit) {
      this.addItem();
      return;
    }

    // Si es editar, cargar la orden
    this.loading = true;
    
    this.ordersService.get(this.orderId!).subscribe({
      next: (order) => {
        this.form.patchValue({ customerName: order.customerName });

        // limpiar y cargar items existentes
        this.items.clear();
        for (const it of (order.items ?? [])) {
          this.items.push(this.fb.group({
            productName: [it.productName, [Validators.required, Validators.maxLength(200)]],
            quantity: [it.quantity, [Validators.required, Validators.min(1)]],
            unitPrice: [it.unitPrice, [Validators.required, Validators.min(0)]]
          }));
        }

        // si no trae items, ponemos 1
        if (this.items.length === 0) this.addItem();

        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
        alert('No se pudo cargar la orden.');
        this.router.navigate(['/app/orders']);
      }
    });
  }

  addItem(): void {
    this.items.push(this.fb.group({
      productName: ['', [Validators.required, Validators.maxLength(200)]],
      quantity: [1, [Validators.required, Validators.min(1)]],
      unitPrice: [0, [Validators.required, Validators.min(0)]]
    }));
  }

  removeItem(index: number): void {
    if (this.items.length === 1) {
      alert('Debe existir al menos un ítem.');
      return;
    }
    this.items.removeAt(index);
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const dto = this.form.value as any;

    this.loading = true;

    if (this.isEdit) {
      this.ordersService.update(this.orderId!, dto).subscribe({
        next: () => {
          this.loading = false;
          this.router.navigate(['/app/orders']);
        },
        error: (err) => {
          console.error(err);
          this.loading = false;
          alert('No se pudo actualizar la orden (¿rol Admin/Manager?).');
        }
      });
    } else {
      this.ordersService.create(dto).subscribe({
        next: () => {
          this.loading = false;
          this.router.navigate(['/app/orders']);
        },
        error: (err) => {
          console.error(err);
          this.loading = false;
          alert('No se pudo crear la orden (¿rol Admin/Manager?).');
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/app/orders']);
  }
}