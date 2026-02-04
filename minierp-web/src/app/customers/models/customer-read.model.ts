export interface CustomerRead {
  id: string;               // Guid del cliente
  name: string;             // Nombre del cliente
  email: string;            // Correo electrónico del cliente
  phone: string;      // Número de teléfono del cliente
  address: string;          // Dirección del cliente
  createdAt: Date;          // Fecha de creación del registro
}