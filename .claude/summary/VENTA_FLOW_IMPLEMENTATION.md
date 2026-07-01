# Venta Flow Implementation Summary

## Overview
This implementation follows the same architectural pattern as the Producto entity, creating a complete flow for the Venta entity across the Domain, Data, and Business layers.

## Files Created

### 1. DTOs (SANTA.PoS.Business\DTOs\VentaDto.cs)
- **CreateVentaDto**: DTO for creating new sales with associated items
  - `ImporteVenta`: Total sale amount
  - `Fecha`: Sale date
  - `RegistroVentas`: List of sale detail records

- **UpdateVentaDto**: DTO for updating sale information (optional fields)
  - `ImporteVenta`: Updated total amount
  - `Fecha`: Updated date

- **VentaDto**: DTO for reading sale data with full details

- **CreateRegistroVentaDto**: DTO for creating sale detail records
  - `ProductoId`: Product ID
  - `Precio`: Unit price
  - `Cantidad`: Quantity sold
  - `Descuento`: Discount flag
  - `Numcliente`: Customer number (optional)
  - `Ncfolio`: Note folio (optional)

- **UpdateRegistroVentaDto**: DTO for updating sale details

- **RegistroVentaDto**: DTO for reading sale detail data

### 2. Repository Interface (SANTA.PoS.Business\Interfaces\IVentaRepository.cs)
- Extends `IBaseRepository<Venta, int>`
- `GetFilteredVentasAsync()`: Retrieve filtered sales with details
- `GetVentaWithDetallesAsync()`: Get a single sale with all details (eager loading)
- **`CreateVentaWithDetallesAsync()`**: Special transactional method that:
  - Creates the Venta record
  - Creates all associated RegistroVenta records
  - Updates product quantities (decrements by sale quantity, minimum 0)
  - Rolls back entire transaction if any step fails

### 3. Repository Implementation (SANTA.PoS.Data\Repositories\VentaRepository.cs)
Implements `IVentaRepository` with:
- Proper DbSet mapping to `_context.Venta`
- Eager loading of related RegistroVenta and Producto data
- Transactional handling for the complete sale process
- Exception handling with proper rollback mechanism
- Direct product quantity updates following the NET Framework logic

### 4. Service Layer (SANTA.PoS.Business\Services\VentaService.cs)
Implements business logic with:
- **CreateVentaAsync()**: Creates a complete sale
  - Validates RegistroVentas is not empty
  - Maps DTOs to entities
  - Calls repository's transactional method

- **GetVentaByIdAsync()**: Retrieve a sale with full details
- **GetAllVentasAsync()**: Retrieve all sales
- **GetVentasByFechaAsync()**: Filter sales by specific date
- **GetVentasByFechaRangeAsync()**: Filter sales by date range
- **UpdateVentaAsync()**: Update sale information
- **DeleteVentaAsync()**: Delete a sale record
- Full error handling with DomainException

### 5. AutoMapper Profile Updates (SANTA.PoS.Business\Mappings\MappingProfile.cs)
Added mappings for:
- `Venta <-> VentaDto`
- `CreateVentaDto -> Venta`
- `UpdateVentaDto -> Venta`
- `RegistroVenta <-> RegistroVentaDto`
- `CreateRegistroVentaDto -> RegistroVenta`
- `UpdateRegistroVentaDto -> RegistroVenta`

## Key Features

### Transaction Safety
The `CreateVentaWithDetallesAsync()` method implements the same logic as the NET Framework version:
1. Begins a database transaction
2. Creates the Venta record
3. Creates RegistroVenta records with the new Venta ID
4. Updates product quantities (preventing negative stock)
5. Commits on success, rolls back on any failure

### Product Quantity Management
- When a sale is created, product quantities are automatically decremented
- If the quantity would go below 0, it's set to 0 (using `Math.Max()`)
- This matches the NET Framework behavior: `IIf(cantidad - @cantidad <= 0, 0, cantidad - @cantidad)`

### Consistent Architecture
- Follows the exact same pattern as ProductService
- Uses records for DTOs (modern C# approach)
- Implements repository pattern with dependency injection
- Proper async/await throughout
- Full validation and error handling

## Integration Notes

To integrate this into your application:

1. **Register in Dependency Injection Container**:
   ```csharp
   services.AddScoped<IVentaRepository, VentaRepository>();
   services.AddScoped<VentaService>();
   ```

2. **Use in Controllers/API Endpoints**:
   ```csharp
   var ventaDto = new CreateVentaDto(
       ImporteVenta: 1500.50m,
       Fecha: DateOnly.FromDateTime(DateTime.Now),
       RegistroVentas: new List<CreateRegistroVentaDto>
       {
           new(ProductoId: 1, Precio: 100.00m, Cantidad: 5, Descuento: 0, Numcliente: null, Ncfolio: null)
       }
   );

   var result = await ventaService.CreateVentaAsync(ventaDto);
   ```

## Database Considerations

Ensure your database schema matches:
- `Venta` table with columns: `IdVenta`, `ImporteVenta`, `Fecha`
- `RegistroVenta` table with columns: `IdRegistro`, `IdVenta`, `ProductoId`, `Precio`, `Cantidad`, `Descuento`, `Numcliente`, `Ncfolio`
- `Productos` table with `Id` and `Cantidad` columns for updates

The implementation handles cascading operations within transactions to maintain data integrity.
