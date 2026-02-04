import { Injectable } from "@angular/core";                         // Angular core module for dependency injection
import { HttpClient } from "@angular/common/http";                  // Angular module for making HTTP requests
import { Observable } from "rxjs";                                  // RxJS module for handling asynchronous data streams
import { CustomerRead } from "../models/customer-read.model";    //  Importing the Customer model
import { environment } from "../../../environments/environment";         // Importing environment configuration

@Injectable({
  providedIn: "root", // This service is provided at the root level
})
export class CustomerService {
  private readonly apiUrl = `${environment.apiUrl}/customers`;      // Base URL for the customer API
    constructor(private http: HttpClient) {}                        // Injecting HttpClient for making HTTP requests

    // Method to get all customers
    getAll(): Observable<CustomerRead[]> {
        return this.http.get<CustomerRead[]>(this.apiUrl);      // Making a GET request to fetch all customers
    }

    // Method to get a customer by ID
    get(id: string): Observable<CustomerRead> {
        return this.http.get<CustomerRead>(`${this.apiUrl}/${id}`);
    }

    // Crear cliente
    create(customer: Partial<CustomerRead>): Observable<CustomerRead> {
        return this.http.post<CustomerRead>(this.apiUrl, customer);
    }

    // Actualizar cliente
    update(id: string, customer: Partial<CustomerRead>): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${id}`, customer);
    }

    // Eliminar cliente
    delete(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }
}   