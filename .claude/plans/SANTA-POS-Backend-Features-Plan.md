# SANTA POS 2.0 — Backend Feature Plan (.NET 10 / Clean Architecture)

> **Context:** Productos and Ventas (with RegistroVenta) are already implemented.  
> This document covers every remaining domain area derived from the original WinForms app.

---

## Database Schema Reference

Derived directly from the SQL queries in the legacy codebase.

```sql
-- Already exists (context from CLAUDE.md)
productos      (id_producto PK VARCHAR, nombre, marca, categoria, cantidad FLOAT,
                precio DECIMAL, status INT, fechaultact DATETIME)
venta          (id_venta PK INT IDENTITY, venta DECIMAL, fecha DATE)
registro_ventas(id_registro PK INT IDENTITY, id_venta FK, id_producto FK,
                precio DECIMAL, cantidad FLOAT, descuento BIT,
                numcliente INT NULL FK, ncfolio INT NULL FK)

-- Needs to be confirmed / added
descuentos          (id_producto FK PK, cantidadMinima FLOAT, precioDescuento FLOAT,
                     status BIT)

clientes            (numCliente INT PK, nombre VARCHAR, aPaterno VARCHAR, aMaterno VARCHAR,
                     calle VARCHAR, numeroExt INT, numeroInt INT, colonia VARCHAR,
                     telefono VARCHAR, usuclaveUltAct VARCHAR FK usuarios)

notas_credito       (numcliente INT FK PK, ncfolio INT PK, status VARCHAR(2),
                     fechaAlta DATETIME, fechaCompromiso DATE,
                     monto DECIMAL, montoPagado DECIMAL)
                     -- status codes: AU=Autorizado, PC=Pagado Parcial, CO=Cobrada, CA=Cancelada

registro_notas_credito (numcliente FK, ncfolio FK, detalle INT PK,
                        id_producto FK, cantidad FLOAT, precio DECIMAL,
                        importe DECIMAL, fechaSurtido DATETIME, descuento BIT)

pagos_notas_credito    (numcliente FK, ncfolio FK, pago INT PK, importe DECIMAL)

usuarios            (usuclave VARCHAR PK, nombre, aPaterno, aMaterno, telefono,
                     pass VARBINARY,   -- legacy: ENCRYPTBYPASSPHRASE; new: bcrypt hash
                     fechaAlta DATETIME DEFAULT GETDATE(),
                     fechaUltAct DATETIME, status BIT DEFAULT 1)

notificaciones      (id_mensaje INT PK IDENTITY, tipo VARCHAR, prioridad VARCHAR,
                     status TINYINT,  -- 0=Inactivo, 1=Activo, 2=Terminado
                     mensaje VARCHAR(MAX), fecha_alta DATETIME, fecha_termino DATETIME NULL,
                     id_producto VARCHAR NULL FK, usuclave VARCHAR NULL FK,
                     usuclave_recibe VARCHAR NULL FK)

venta_dia           (fecha DATE PK,
                     inicio FLOAT, monedas FLOAT, uso_monedas FLOAT,
                     proveedores FLOAT, gasto FLOAT, quedo FLOAT,
                     saldo_inicial FLOAT, saldo_final FLOAT, caja FLOAT,
                     venta_saldo FLOAT, venta_abarrote FLOAT, total FLOAT, final FLOAT)

categorias          -- NEW: replaces the legacy categoria.txt file
                    (nombre VARCHAR PK)
```

---

## Feature Areas

---

### 1. Descuentos (Product Discounts)

**Business rules:**
- Each active product can have at most one discount rule.
- A discount rule defines a `cantidadMinima` (minimum cumulative quantity in a single sale) and `precioDescuento` (alternative unit price).
- At sale time the query `IIF(@cantidadMinima >= d.cantidadMinima AND d.status = 1, d.precioDescuento, p.precio)` determines price dynamically.
- `status = 0` means the discount rule exists but is suspended; the standard price is used.

**Endpoints to implement:**

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/descuentos` | List all discount rules (with product info joined) |
| GET | `/api/descuentos/{idProducto}` | Get the discount rule for a specific product |
| POST | `/api/descuentos` | Create a new discount rule |
| PUT | `/api/descuentos/{idProducto}` | Update cantidadMinima and/or precioDescuento |
| PATCH | `/api/descuentos/{idProducto}/status` | Toggle status (activate / deactivate) |
| DELETE | `/api/descuentos/{idProducto}` | Remove the discount rule entirely |

**Domain entities / DTOs:**

```csharp
// Domain
public class Descuento
{
    public string IdProducto { get; set; }
    public float CantidadMinima { get; set; }
    public decimal PrecioDescuento { get; set; }
    public bool Status { get; set; }
}

// Request DTOs
public record CreateDescuentoRequest(string IdProducto, float CantidadMinima, decimal PrecioDescuento);
public record UpdateDescuentoRequest(float CantidadMinima, decimal PrecioDescuento);
```

**Integration with Ventas (already implemented):**  
The existing `GetProductoParaVenta` use-case / repository query needs to LEFT JOIN `descuentos` and accept `cantidadActual` as a parameter, returning either the discount price or the regular price plus a `tieneDescuento` boolean.

---

### 2. Clientes (Customers)

**Business rules:**
- `numCliente` is a user-assigned sequential folio, not an auto-identity column. The app always shows `MAX(numCliente) + 1` as the next suggested number.
- At minimum the following fields are required: nombre, aPaterno, calle, numeroExt, colonia, telefono (≥ 10 digits).
- aMaterno and numeroInt are optional.
- `usuclaveUltAct` records who last modified the record.

**Endpoints:**

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/clientes` | List all clients (id, full name, calle, colonia, telefono) |
| GET | `/api/clientes/{numCliente}` | Get one client |
| GET | `/api/clientes/next-folio` | Returns MAX(numCliente)+1 |
| POST | `/api/clientes` | Create client |
| PUT | `/api/clientes/{numCliente}` | Update client |

**Validation:**
- telefono length ≥ 10.
- Duplicate numCliente → 409 Conflict.

**Domain entity:**

```csharp
public class Cliente
{
    public int NumCliente { get; set; }
    public string Nombre { get; set; }
    public string APaterno { get; set; }
    public string AMaterno { get; set; }
    public string Calle { get; set; }
    public int NumeroExt { get; set; }
    public int NumeroInt { get; set; }
    public string Colonia { get; set; }
    public string Telefono { get; set; }
    public string UsuclaveUltAct { get; set; }
}
```

---

### 3. Notas de Crédito (Credit Notes)

This is the most complex domain. A credit note records products given to a customer on credit and tracks payments against it.

**Business rules:**
- A client can have at most ONE open (non-CO/CA) note at a time.
- If the open note's `fechaCompromiso` is in the past → it is expired; the client cannot get a new note until it is fully paid.
- `fechaCompromiso` must be strictly after `fechaAlta`.
- When creating a note: items come from the current active sale cart (products + quantities + prices, including discount flag). Inventory is decremented at creation time.
- When updating a note (adding more items): new items are appended to `registro_notas_credito`; `notas_credito.monto` is incremented by the new batch total.
- When paying (full or partial):
  - A `venta` record and a `registro_ventas` record are created with pseudo-products:
    - `NC01` = full/final payment of a credit note.
    - `NC02` = partial payment (abono).
  - `notas_credito.montoPagado` is incremented; status changes to `PC` if remaining > 0, `CO` if fully paid.
  - A record is added to `pagos_notas_credito`.
- Payment can exceed the remaining balance — the overpayment is treated as "cambio" (change), never stored.
- `CA` (cancelled) status exists but the legacy app never actually implemented the cancel action. Implement it as a proper endpoint.

**Endpoints:**

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/notas-credito/{numCliente}` | List all notes for a client |
| GET | `/api/notas-credito/{numCliente}/{ncfolio}` | Get one note with detail |
| GET | `/api/notas-credito/{numCliente}/{ncfolio}/pagos` | List payments for a note |
| GET | `/api/notas-credito/{numCliente}/open` | Get the current open note (if any) |
| GET | `/api/notas-credito/{numCliente}/next-folio` | MAX(ncfolio)+1 for client |
| POST | `/api/notas-credito` | Create a new note (with items) |
| PUT | `/api/notas-credito/{numCliente}/{ncfolio}/items` | Add items to existing open note |
| POST | `/api/notas-credito/{numCliente}/{ncfolio}/pagos` | Record a payment |
| PATCH | `/api/notas-credito/{numCliente}/{ncfolio}/cancelar` | Cancel a note (only if status=AU) |

**Key DTOs:**

```csharp
public record CreateNotaCreditoRequest(
    int NumCliente,
    int NcFolio,
    DateTime FechaCompromiso,
    decimal Monto,
    List<NotaCreditoItemRequest> Items);

public record NotaCreditoItemRequest(
    string IdProducto,
    float Cantidad,
    decimal Precio,
    decimal Importe,
    bool Descuento);

public record PagoNotaCreditoRequest(int NumCliente, int NcFolio, decimal MontoPagado);
```

**Transaction logic for CreateNotaCredito:**
1. INSERT into `notas_credito` (status = 'AU').
2. For each item: INSERT into `registro_notas_credito`, UPDATE `productos` decrementing quantity.
3. All in a single DB transaction.

**Transaction logic for PagoNotaCredito:**
1. Calculate `remaining = monto - montoPagado`.
2. `efectivoPagado = MIN(montoPagado, remaining)` (overpayment = change, not stored).
3. INSERT into `venta` and `registro_ventas` (NC01 or NC02 depending on full/partial).
4. UPDATE `notas_credito`: increment `montoPagado`, set `status = CO or PC`.
5. INSERT into `pagos_notas_credito`.

---

### 4. Ventas Hechas — Devoluciones (Past Sales & Returns)

**Business rules:**
- Browse historical `venta` records by date.
- Can return an entire order (returns all product quantities to inventory, deletes the `venta` record).
- Can return a single product line from an order (returns that product's quantity, removes only that `registro_ventas` row, decrements the `venta.venta` amount).
- Both operations require password / admin authorization.
- NC01/NC02 pseudo-products should NOT be individually returnable (only full-order return is allowed when NC lines are present).

**Endpoints:**

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/ventas?fecha=YYYY-MM-DD` | List sale IDs for a given date |
| GET | `/api/ventas/{idVenta}/detalle` | Detail lines for a sale (already partially done) |
| DELETE | `/api/ventas/{idVenta}` | Full-order return (requires admin claim) |
| DELETE | `/api/ventas/{idVenta}/detalle/{idRegistro}` | Single-line return (requires admin claim) |

**Transaction logic for full-order return:**
1. For each row in `registro_ventas`: UPDATE productos quantity += cantidad.
2. DELETE FROM venta WHERE id_venta = @id (cascade deletes registro_ventas via FK).

**Transaction logic for single-line return:**
1. UPDATE productos SET cantidad = cantidad + @cantidad WHERE id_producto = @id.
2. UPDATE venta SET venta = venta - @importe WHERE id_venta = @id.
3. DELETE FROM registro_ventas WHERE id_registro = @id_registro.

---

### 5. Venta del Día (Daily Cash Reconciliation)

**Business rules:**
- One record per calendar date.
- Fields: `inicio` (opening cash), `monedas` (coins counted), `uso_monedas`, `proveedores` (amount paid to suppliers), `gasto`, `saldo_inicial` (credit balance at start of day), `saldo_final`, `caja` (cash in drawer), `venta_saldo` (credit sales), `venta_abarrote` (POS sales from `venta` table), `total`, `final`.
- Derived fields (validated server-side):
  - `quedo = proveedores - gasto`
  - `venta_saldo = saldo_inicial - saldo_final`
  - `venta_abarrote = SUM(venta.venta WHERE fecha = today) - venta_saldo` *(the POS system provides this automatically)*
  - `total = monedas + venta_saldo + venta_abarrote + caja`
  - `final = monedas + saldo_final + venta_saldo + venta_abarrote + caja`
- The API should also return the auto-computed `ventaAbarroteCalculada` from the `venta` table for a given date, so the frontend can pre-fill and validate.

**Endpoints:**

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/venta-dia/{fecha}` | Get the reconciliation record for a date (or 404 if not saved yet) |
| GET | `/api/venta-dia/{fecha}/venta-pos` | Returns `SUM(venta.venta)` for that date from the POS table |
| POST | `/api/venta-dia` | Save reconciliation for a date |
| PUT | `/api/venta-dia/{fecha}` | Update an existing reconciliation |

---

### 6. Usuarios (Users)

**Business rules:**
- `usuclave` is a user-defined short key (username), not auto-generated.
- `pass` in the legacy DB is encrypted with SQL Server `ENCRYPTBYPASSPHRASE`. In the new system, store a bcrypt hash (or use ASP.NET Core Identity).
- Admin users can: add users, update personal data, enable/disable users, reset any user's password.
- Non-admin users can only change their own password (requires current password verification).
- A disabled user (status = 0) cannot log in.
- `fechaUltAct` is set to `GETDATE()` on any UPDATE.

**Endpoints:**

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/usuarios` | List all users (clave, nombre, fechaAlta, status) |
| GET | `/api/usuarios/{usuclave}` | Get one user |
| POST | `/api/usuarios` | Create user (admin only) |
| PUT | `/api/usuarios/{usuclave}` | Update personal data (admin only) |
| PATCH | `/api/usuarios/{usuclave}/status` | Enable / disable (admin only) |
| PUT | `/api/usuarios/{usuclave}/password` | Change password (admin: no old password required; self: old password required) |
| POST | `/api/auth/login` | Authenticate → return JWT |

**Password change logic:**

```csharp
// Admin path
if (isAdmin) { hashAndSave(newPassword); }

// Self-service path
else {
    if (!verify(oldPassword, storedHash)) return 400;
    if (oldPassword == newPassword) return 400;
    hashAndSave(newPassword);
}
```

**Migration note:** Existing passwords in the legacy DB are encrypted with `ENCRYPTBYPASSPHRASE('Itendstonightkrystal05', pass)`. On first login after migration, decrypt the legacy value using a one-time SQL script, verify the user-supplied password against it, then re-hash with bcrypt and store the new hash. Remove the legacy VARBINARY column.

---

### 7. Notificaciones (Internal Notifications)

**Business rules:**
- `tipo` options: `"Cambio en producto"` (requires `id_producto`), plus any other custom types.
- `prioridad` options: at minimum High/Medium/Low (map from legacy combobox items).
- `status`: 0 = Inactivo, 1 = Activo, 2 = Terminado. Active notifications are shown in the main dashboard. Transitions: Activo → Inactivo or Activo → Terminado. Both are irreversible (no revert to Activo).
- `usuclave_recibe = NULL` means the notification is addressed to "General" (everyone).
- `fecha_termino` is set by the server when marking as Inactivo or Terminado.

**Endpoints:**

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/notificaciones` | List with filters: status, tipo, prioridad, fechaAlta, fechaTermino, idMensaje |
| GET | `/api/notificaciones/{id}` | Get one notification |
| POST | `/api/notificaciones` | Create or upsert a notification |
| PATCH | `/api/notificaciones/{id}/inactivar` | Set status = 0 |
| PATCH | `/api/notificaciones/{id}/terminar` | Set status = 2 |

**Filter query parameters:**
```
GET /api/notificaciones?incluirInactivosTerminados=true&tipo=...&prioridad=...&status=...&fechaAlta=...&fechaTermino=...&idMensaje=...
```

The legacy SQL upsert pattern (`UPDATE ... IF @@ROWCOUNT = 0 INSERT ...`) maps cleanly to an `UPSERT` use-case or separate Create/Update endpoints.

---

### 8. Categorías (Product Categories)

**Business rules:**
- In the legacy app, categories are stored in a plaintext file (`categoria.txt`). This is a problem — move to a DB table.
- A category is just a `nombre` string (unique, case-insensitive).
- Used in the Producto create/edit forms as a dropdown.

**Endpoints:**

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/categorias` | List all categories |
| POST | `/api/categorias` | Add a new category |
| DELETE | `/api/categorias/{nombre}` | Remove a category (only if no product references it, or use soft-delete) |

---

### 9. Requisición (Product Requisition)

**Business rules:**
- In the legacy app, the requisition list is persisted to `requisicion.txt`. There is no DB involvement.
- This is user-session-scoped (one list per workstation session).
- The list is a set of product codes + requested quantities.
- When submitted, an email is sent to the store owner (HTML table format).
- In the new architecture, persist the draft requisition server-side per user session or via a simple API resource (not audited, can be overwritten freely).

**Endpoints:**

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/requisicion` | Get current requisition for the authenticated user |
| PUT | `/api/requisicion` | Save/overwrite the requisition list |
| DELETE | `/api/requisicion` | Clear the requisition |
| POST | `/api/requisicion/enviar` | Send the requisition by email and clear |

**Email payload** (same as legacy): HTML table with columns Nombre | Cantidad.

---

### 10. Email Notifications

The legacy `Correos` class sends email via Gmail SMTP with hardcoded credentials. In the new system:

- Use a configurable SMTP abstraction (`IEmailService`).
- Credentials stored in `appsettings.json` / environment variables / secrets vault.
- Triggered by:
  1. Sale where a product's `cantidad` drops to ≤ 0 → notify owner of zero-stock products.
  2. Venta del Día → user manually sends daily summary email.
  3. Requisición → user manually sends the order request.

---

## Existing Feature Amendments (Ventas / Productos)

These are corrections or clarifications for the already-implemented features.

### Ventas — Missing behaviours

| Behaviour | Legacy source | What to add |
|-----------|--------------|-------------|
| Discount price applied on quantity change | `dataGridViewVenta_CellEndEdit` | The `GET /api/productos/{id}/precio?cantidad=N` endpoint (or equivalent) must accept a cumulative quantity, re-run the discount join, and return the updated unit price + `tieneDescuento`. The frontend re-calculates the row total. |
| Mexican peso rounding | `calculaPrecio()` | Implement as a pure domain function: `[0, 0.25) → floor`, `[0.25, 0.75) → + 0.50`, `[0.75, 1) → ceil`. Return the rounded total along with an `aplicaRedondeo` boolean in the sale confirmation response. |
| Multi-payment flow | `buttonCompra_Click` | The frontend accumulates partial payments. The backend only sees the final `POST /api/ventas` with the total amount; partial payment tracking is purely a UI concern. |

### Productos — Barcode (code) rename

`Accion_Productos` with mode `"Editar"` allows changing only the `id_producto` (barcode) on an existing product — everything else is read-only in that screen. This is a specific use case:

```
PATCH /api/productos/{idProducto}/codigo
Body: { "nuevoCodigo": "NEWCODE" }
```

This must cascade to all FK references (`descuentos`, `registro_ventas`, `registro_notas_credito`, `notificaciones`). Ensure ON UPDATE CASCADE is set on all FK constraints, or handle it in the use-case.

---

## Notes on `Utilerias.cs` — What to keep vs. discard

| Method | Verdict | Reason |
|--------|---------|--------|
| `VerifyQuotes` | **Discard** | Was a SQL injection workaround. Parameterized queries render it unnecessary. |
| `EjecutaComando` / `EjecutaComandoAsync` | **Discard** | Replaced by EF Core / Dapper repository pattern. |
| `ExecuteQuery` / `GetResultsFromQuery` (and async variants) | **Discard** | Same as above. |
| `ValidarDatos` / `MostrarListaValores` | **Discard** | These are UI concerns (open a list-of-values picker dialog). Replaced by React combobox/search components (see Frontend plan). |
| `CaracterValido` / `CaracterEsNumero` | **Discard** | These are input-event handlers for WinForms KeyPress events. In the new stack, input validation is handled by React controlled inputs + Zod schema validation. |

The **concept** behind `ValidarDatos` is important: several fields (product code, user, recipient in notifications) need a "search and select" UX. The backend must expose search endpoints for each (`/api/productos?q=`, `/api/usuarios?q=`) and the frontend implements the picker. See Frontend plan.

---

## Architecture Notes

Follows the Clean Architecture already established in the project:

```
Domain/
  Entities/       -- Cliente, NotaCredito, Descuento, Usuario, ...
  Interfaces/     -- IClienteRepository, INotaCreditoRepository, ...

Application/
  UseCases/
    Clientes/     -- GetClientes, CreateCliente, UpdateCliente
    NotasCredito/ -- CreateNotaCredito, AddItems, RegistrarPago, Cancelar
    Ventas/       -- GetVentasByFecha, DevolverOrden, DevolverProducto (new)
    ...

Infrastructure/
  Persistence/
    Repositories/ -- SqlServer/EFCore implementations

API/
  Controllers/    -- One per domain area
  DTOs/
```

All write operations should go through named use-cases. All reads can go directly through query-optimized repository methods (CQRS-lite).

---

## Implementation Order (Suggested)

1. **Categorías** — small, unblocks Productos completely.
2. **Descuentos** — needed to make Ventas fully accurate.
3. **Usuarios + Auth** — needed by Clientes, Venta del Día, and admin-gated endpoints.
4. **Clientes** — prerequisite for Notas de Crédito.
5. **Notas de Crédito** — most complex; implement Create, then AddItems, then Pago.
6. **Ventas Hechas — Devoluciones** — extends the already-implemented Ventas.
7. **Venta del Día** — standalone, low complexity.
8. **Notificaciones** — standalone.
9. **Requisición** — standalone, low DB complexity.
10. **Email service** — cross-cutting, wire in after the features that use it are done.
