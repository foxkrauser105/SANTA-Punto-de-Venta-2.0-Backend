# Entity Framework Setup Summary

## Overview
Successfully scaffolded and configured Entity Framework Core entities from the SANTA SQL Server database.

## Scaffolded Entities

The following entities have been created in `SANTA.PoS.Data\Entities\`:

### Core Entities
- **Cliente** - Customer/Client information
  - Primary Key: Numcliente
  - Relationships: One-to-many with Usuario (via UsuclaveUltAct)

- **Usuario** - User/Employee information
  - Primary Key: Usuclave
  - Relationships: One-to-many with Clientes

- **Producto** - Product/Inventory management
  - Primary Key: IdProducto
  - Relationships: One-to-one with Descuento, One-to-many with RegistroVenta and RegistroNotasCredito

- **Descuento** - Product discounts
  - Primary Key: IdProducto (Foreign Key to Producto)
  - Relationships: One-to-one with Producto

### Sales-Related Entities
- **Venta** (Ventum) - Sales header
  - Primary Key: IdVenta
  - Relationships: One-to-many with RegistroVenta

- **RegistroVenta** - Sales detail/line items
  - Primary Key: IdRegistro
  - Relationships: Many-to-one with Venta and Producto

- **VentaDium** (venta_dia) - Daily sales summary
  - Primary Key: IdVentadia
  - **Note**: Contains calculated fields (Total, Final) marked as computed

### Credit Notes Management
- **NotasCredito** - Credit note header
  - Primary Key: (Numcliente, Ncfolio)

- **RegistroNotasCredito** - Credit note details
  - Primary Key: (Numcliente, Ncfolio, Detalle)
  - Relationships: Many-to-one with Producto

- **PagosNotasCredito** - Credit note payments
  - Primary Key: (Numcliente, Ncfolio, Pago)

### Notifications
- **Notificacione** - System notifications
  - Primary Key: IdMensaje

## Computed Columns Configuration

### VentaDium (venta_dia) Table
The following columns are marked as **computed** in EF Core configuration:
- `Total` - Marked with `PropertySaveBehavior.Ignore` (computed value)
- `Final` - Marked with `PropertySaveBehavior.Ignore` (computed value)

**Important**: These fields are read-only after save operations. EF Core will not attempt to save values to these columns, and will instead read them from the database after insert/update operations.

## AppDbContext Configuration

The main `AppDbContext` has been updated to:
- Include all 11 DbSet properties for entity access
- Configure all entity-to-table mappings
- Set up foreign key relationships
- Map all column names to match the database schema (snake_case to camelCase)
- Configure data types and constraints (varchar lengths, decimal precision)
- Mark computed columns appropriately

### Connection Configuration
- Uses the connection string from `appsettings.json` via configuration binding
- Connection string name: `ConnectionStrings:DefaultConnection`
- Database collation: `Modern_Spanish_CI_AS` (Spanish case-insensitive)

## Usage Example

```csharp
// Inject AppDbContext in your services
public class YourService
{
    private readonly AppDbContext _context;

    public YourService(AppDbContext context)
    {
        _context = context;
    }

    // Query example
    public async Task<List<Producto>> GetProductsAsync()
    {
        return await _context.Productos.ToListAsync();
    }

    // Note: VentaDium.Total and Final are read-only after save
    public async Task<VentaDium> GetDailySalesAsync(int ventadiaId)
    {
        var ventadia = await _context.VentaDia.FindAsync(ventadiaId);
        // Total and Final will be populated from database computed values
        return ventadia;
    }
}
```

## Database Relationship Diagram

```
Usuario (1) ──── (N) Cliente
Producto (1) ──── (1) Descuento
Producto (1) ──── (N) RegistroVenta
Producto (1) ──── (N) RegistroNotasCredito
Venta (1) ──── (N) RegistroVenta
NotasCredito (1) ──── (N) RegistroNotasCredito
```

## Notes

- All entities are located in `SANTA.PoS.Data\Entities\`
- The scaffolded `SantaContext.cs` can be kept for reference or removed (using `AppDbContext` is recommended)
- Connection string is safely configured via appsettings.json
- All entity relationships and constraints are properly configured
- Computed columns are properly handled to prevent EF Core write attempts
