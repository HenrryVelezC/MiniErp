// src/app/auth/login/login.component.ts
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';

// Angular Material
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {

  // Inyección funcional con 'inject' (Angular 14+)
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  loading = false;

  // Formulario reactivo con validaciones básicas
  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  /** Envía credenciales al backend, guarda token y navega */
  submit() {
    if (this.form.invalid || this.loading) return;

    this.loading = true;

    this.auth.login(this.form.value as any).subscribe({
      next: res => {
        this.auth.saveToken(res.token);
        this.router.navigate(['/app']);    // Ruta protegida por AuthGuard
      },
      error: _ => {
        this.loading = false;
        alert('Credenciales inválidas');
      }
    });
  }
}
