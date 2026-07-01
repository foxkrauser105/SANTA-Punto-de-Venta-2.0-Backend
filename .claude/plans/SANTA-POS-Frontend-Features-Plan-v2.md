# SANTA POS 2.0 — Frontend Feature Plan (React + TypeScript) — v2

> **Context:** Brand-new React + TS SPA consuming the .NET 10 backend API.
> This version is corrected against the **actual implemented backend** (not the legacy plan).
> Discrepancies from the original plan are marked **[CORRECTED]**.

---

## Key Backend Corrections vs. Original Plan

| Topic | Original plan said | Actual backend |
|---|---|---|
| `descuento` flag | Numeric amount | `bool` — just a flag (was discount applied?) |
| `VentaDia` PK | `fecha DATE` | `id_ventadia INT IDENTITY` |
| Auth | Deferred | Full JWT + roles implemented now |
| NC product codes | "special logic" | NC01 (full payment) / NC02 (partial) must exist as real `productos` rows |
| `NotaCreditoDto.Items` | Always populated | `null` by default from the mapping; fetch separately with `/items` or `/pagos` endpoints |
| `Venta.ImporteVenta` | Column `importe_venta` | Column is actually named `venta` in the DB (mapped via EF) |
| Password storage | ENCRYPTBYPASSPHRASE | BCrypt hash stored as `VARBINARY` (UTF-8 bytes of the BCrypt string) |
| Categories | `categoria.txt` file | `GET /api/categorias` — DB-backed table |
| Requisición | `requisicion.txt` | `GET/PUT/DELETE /api/requisicion` per authenticated user (JWT identity) |

---

## Authentication Model

**[CORRECTED]** Auth is fully implemented with JWT Bearer tokens.

- `POST /api/auth/login` → returns `{ token: string, usuclave: string, nombre: string, rol: string }`
- Token contains: `NameIdentifier` (usuclave), `Name`, `Role` ("User" | "Admin")
- Store token in memory (not `localStorage`) — use an AuthContext; attach as `Authorization: Bearer {token}` header.
- Role-based UI: hide Admin-only controls when `rol !== "Admin"`.
- Session expiry: JWT has configurable `ExpiresInMinutes`; on 401 response, redirect to login.

**[CORRECTED — new endpoint needed]** The original plan references `POST /api/auth/verify-admin` for the `PasswordGateDialog`. This endpoint does **not exist**. Instead:
- Validate the role from the JWT claim client-side (`rol === "Admin"`).
- The `PasswordGateDialog` is only needed for the Venta del Día hidden-fields toggle (F9), which shows sensitive data — validate the password by calling `POST /api/auth/login` with the current user's credentials.
- For route protection (Usuarios, Clientes), rely on the JWT role claim directly.

---

## Global Patterns

### Input: Lookup / List-of-Values (LOV) Fields

The legacy app used a modal dialog triggered by **F9**. Implement a `<LookupField>` component:
- Renders as a text input + search icon button.
- Typing triggers debounced API search (500 ms).
- Results appear in a dropdown popover (max 8 rows).
- Selecting a row fills the code field and auto-populates a description field.
- **F9** / clicking the icon opens a full modal table.
- Used in: Notificaciones (product, usuario, recibe), Notas de Crédito (numCliente).

### Price / Amount Display

All monetary amounts: `Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' })`.

### Confirmation Dialogs

Reusable `<ConfirmDialog>` with "Sí" / "No" buttons for every destructive action.

### Toast / Error Notifications

Use a toast stack (e.g., `react-hot-toast`) for non-destructive feedback.

### PasswordGateDialog

**[CORRECTED]** Does NOT call a backend endpoint. It calls `POST /api/auth/login` with the current user's credentials to verify. On success, proceeds; on failure shows inline error. Only used for the Venta del Día sensitive-fields toggle.

### Table Behaviour

Use TanStack Table v8 with click-to-select, double-click callback, sortable columns, and client-side filtering on cached data.

---

## Application Shell

### Layout

```
┌─────────────────────────────────────────────────────────────┐
│  SIDEBAR (left, collapsible)                                 │
│  ┌────────────────────────────────────────────────────┐    │
│  │ [SANTA Logo]                                        │    │
│  │ → Venta          ← default on load                  │    │
│  │   Productos                                         │    │
│  │   Venta del Día                                     │    │
│  │   Ventas Hechas                                     │    │
│  │   Requisición                                       │    │
│  │   Notificaciones                                    │    │
│  └────────────────────────────────────────────────────┘    │
│                                                              │
│  MAIN CONTENT AREA (right)                                   │
│                                                              │
│  NOTIFICATION BAR (bottom, collapsible)                      │
│  Polls GET /api/notificaciones?status=1 every 10 s          │
└─────────────────────────────────────────────────────────────┘
```

**Notes:**
- Notification bar polls `GET /api/notificaciones?status=1` (status is `int` **[CORRECTED]**: `1` = active, `0` = inactive, `2` = finished — not string "Activo").
- SPA routing; no full page reloads.
- Warn on tab close if cart has items.

---

## Module: Acceso (Login)

Full-page overlay on app start.

```
┌──────────────────────────────────────┐
│           SANTA POS                   │
│   Usuario: [___________________]      │
│   Contraseña: [___________________]   │
│   [Ingresar]                          │
└──────────────────────────────────────┘
```

- `POST /api/auth/login` → `{ token, usuclave, nombre, rol }`
- JWT stored in memory (AuthContext). Attach via `Authorization: Bearer {token}` on every request.
- Disabled users return HTTP 409 from the backend with the DomainException message; display that message.

---

## Module: Ventas (POS — Sale Cart)

Default screen on app load.

### Layout

```
┌──────────────────────────────────────────────────────────────┐
│  [Código: ___________] [Cantidad: _____] [+ Añadir]          │
├──────────────────────────────────────────────────────────────┤
│  SALE CART TABLE                                             │
│  Código | Nombre | Precio | Cantidad | Importe               │
│  (right-click row → "Eliminar")                              │
├──────────────────────────────────────────────────────────────┤
│  [✓ Redondeo aplicado]            Total: $XXX.XX             │
│                                                              │
│  [Limpiar]   [Agregar Nota]   [Actualizar Nota]              │
│                                                              │
│  Pago: $[__________]                                         │
│  Pagado: $XX.XX  /  [Resta/Sobra]: $XX.XX                   │
│                                                              │
│  [Confirmar Compra]                                          │
└──────────────────────────────────────────────────────────────┘
```

### Behaviours

**Adding a product:**
1. User types a product code and optional quantity (default 1). Press **Enter** or click **Añadir**.
2. Call `GET /api/productos/{codigo}/precio?cantidad={cumulativeQty}`.
   - `cumulativeQty` = quantity typed + quantity already in cart for that product.
3. Response: `{ precio: decimal, aplicaDescuento: bool }` **[CORRECTED]**: field is `aplicaDescuento` (bool), not `tieneDescuento`.
   - Backend also returns a 409 if the product doesn't exist (DomainException); catch and offer "Add product" / "Edit code" drawer.
4. `status = 0` products: the price endpoint will still return a price; check the product status from `GET /api/productos/{id}` if needed, or handle the 404/409 response.
5. Row color: `aplicaDescuento === true` → highlight price, quantity, and total in green.

**Descuento flag in cart rows:**
- **[CORRECTED]** The `descuento` field on `CreateRegistroVentaDto` is a `bool` (was discount applied?), not an amount. Set it from `aplicaDescuento` returned by the price endpoint.

**MXN rounding:**
```
fraction = total - Math.floor(total)
[0, 0.25)  → Math.floor(total)
[0.25, 0.75) → Math.floor(total) + 0.50
[0.75, 1)  → Math.ceil(total)
```

**Confirmar Compra → POST /api/ventas**

Payload shape (maps to `CreateVentaDto`):
```json
{
  "importeVenta": 123.50,
  "fecha": "2026-06-21",
  "registroVentas": [
    {
      "idProducto": "P001",
      "precio": 25.00,
      "cantidad": 2,
      "descuento": true,
      "numcliente": null,
      "ncfolio": null
    }
  ]
}
```
**[CORRECTED]** `descuento` is a `bool`, not a number.

---

## Module: Productos (Product Management)

### Layout

```
┌──────────────────────────────────────────────────────────────┐
│  [Buscar: ___________]                       Menu: [Añadir ▼]│
│  PRODUCTS TABLE                                              │
│  Código | Nombre | Precio | Cantidad | Marca | Fecha Ult Act │
│  (auto-refreshes every 10 s if user is not typing)           │
├──────────────────────────────────────────────────────────────┤
│  Selected product details (read-only labels)                 │
└──────────────────────────────────────────────────────────────┘
```

- Search: `GET /api/productos?q=...` (debounced 500 ms).
- Auto-refresh every 10 s when user is not typing.
- Show only `status = 1` products by default.

**"Añadir" dropdown:**
- **Producto** → `POST /api/productos`
- **Entrada de Producto** → `PATCH /api/productos/{id}/stock` **[CORRECTED — new endpoint needed]** The backend has `PUT /api/productos/{id}` for full update. There is no dedicated `/stock` endpoint; use `PUT /api/productos/{id}` with only `cantidad` changed, or confirm with the backend team to add a PATCH endpoint.
- **Editar Producto** → `PUT /api/productos/{id}`
- **Desactivar Producto** → `PATCH /api/productos/{id}/status` (Admin only); requires `[Authorize(Roles="Admin")]` on the backend — the UI must have Admin role.
- **Productos en Cero** → client filter: `productos.filter(p => p.cantidad <= 0)`
- **Productos Inactivos** → `GET /api/productos` filtered by `status = 0`
- **Descuentos** → Descuentos drawer
- **Usuarios** → requires `rol === "Admin"` check from JWT; if admin, show full Usuarios view; if not, show read-only.
- **Clientes** → requires login (show login modal if not authenticated, or just use the current JWT)
- **Notas de Crédito** → opens standalone NC browser

### Sub-module: Añadir / Editar Producto

- Categoría dropdown: `GET /api/categorias`
- Inline "+" next to Categoría: `POST /api/categorias` (Admin only)
- If barcode (`idProducto`) changes on edit: use `PATCH /api/productos/{id}/codigo` **[CORRECTED]**: this endpoint exists. Send `{ "nuevoCodigo": "..." }`.

---

## Module: Descuentos (Drawer)

```
┌───────────────────────────────────────────┐
│  [Buscar: ___________]                    │
│  DISCOUNTS TABLE                          │
│  Status | Código | Nombre | Cantidad Mín | Precio Desc │
│  (green row = status=1, red = status=0)   │
├───────────────────────────────────────────┤
│  Código: [LookupField]  Nombre: (auto)   │
│  Cantidad Mínima: [___]                   │
│  Precio Descuento: [___]                  │
│                                           │
│  [Agregar/Aplicar]   [Activar/Desactivar] │
└───────────────────────────────────────────┘
```

- `GET /api/descuentos` — all discounts.
- No existing rule → "Agregar" → `POST /api/descuentos`
- Existing rule → "Aplicar" → `PUT /api/descuentos/{id}`
- Toggle → `PATCH /api/descuentos/{id}/status` (Admin only)

**[CORRECTED]** `DescuentoDto` shape:
```ts
{ idProducto: string; cantidadMinima: number; precioDescuento: number; status: number }
```
`status` is an `int` (1 = active, 0 = inactive), not a boolean.

---

## Module: Clientes (Customers)

- Table: `GET /api/clientes`
- Next folio: `GET /api/clientes/next-folio`
- Create: `POST /api/clientes`
- Update: `PUT /api/clientes/{numcliente}`
- Telefono: minimum 10 characters (backend validates and returns 409).

---

## Module: Notas de Crédito

### Mode: Crear / Actualizar (from Ventas cart)

`POST /api/notas-credito` payload (`CreateNotaCreditoDto`):
```json
{
  "numcliente": 1,
  "ncfolio": 5,
  "fechaCompromiso": "2026-07-01",
  "items": [
    {
      "idProducto": "P001",
      "cantidad": 2,
      "precio": 25.00,
      "importe": 50.00,
      "descuento": true
    }
  ]
}
```
**[CORRECTED]** `descuento` in items is `bool`.

- Open nota check: `GET /api/notas-credito/{numcliente}/open`
  - Returns the nota if open ("AU" or "PC" status), null if none.
  - If open and `fechaCompromiso < today` → backend will reject new nota with 409 "vencida".
- `fechaCompromiso` must be strictly after today (backend validates).
- Update items: `PUT /api/notas-credito/{numcliente}/{ncfolio}/items`

### Mode: Pagar (standalone)

- `GET /api/notas-credito/{numcliente}` — list all notes for client.
- `GET /api/notas-credito/{numcliente}/{ncfolio}` — detail (does NOT include items by default **[CORRECTED]** — `Items` is `null` in the mapping).
- `GET /api/notas-credito/{numcliente}/{ncfolio}/pagos` — payment history.
- `POST /api/notas-credito/{numcliente}/{ncfolio}/pagos` — register payment.
  - Body: `{ "montoPagado": 50.00 }`
  - Backend determines NC01/NC02 usage; those products must exist in `productos`.

---

## Module: Ventas Hechas (Past Sales / Returns)

- `GET /api/ventas?fecha=YYYY-MM-DD`
- `GET /api/ventas/{id}` — includes `registroVentas[]`.
- **[CORRECTED]** Each `RegistroVentaDto` has `descuento: bool`, not a number. Green row = `descuento === true`.
- Full return (Admin): `DELETE /api/ventas/{id}`
- Single line return (Admin): `DELETE /api/ventas/{id}/detalle/{idRegistro}`
- Backend restores `Producto.Cantidad` and adjusts `Venta.ImporteVenta` transactionally.

---

## Module: Venta del Día

- `GET /api/venta-dia/{fecha}` — returns `VentaDiaDto` or 404.
- `GET /api/venta-dia/{fecha}/venta-pos` — returns `{ ventaAbarroteCalculada: decimal }` (sum of `venta.venta` for that date).
- `POST /api/venta-dia` — create.
- `PUT /api/venta-dia/{fecha}` — update.

**[CORRECTED]** `VentaDiaDto` shape (all fields present, backend computes derived ones):
```ts
{
  idVentadia: number;       // PK (IDENTITY)
  fecha: string;            // DateOnly
  inicio: number;
  monedas: number;
  usoMonedas: number;
  proveedores: number;
  gasto: number;
  quedo: number;            // auto: proveedores - gasto
  saldoInicial: number;
  saldoFinal: number;
  caja: number;
  ventaSaldo: number;       // auto: saldoInicial - saldoFinal
  ventaAbarrote: number;
  total: number;            // auto: monedas + ventaSaldo + ventaAbarrote + caja
  final: number;            // auto: monedas + saldoFinal + ventaSaldo + ventaAbarrote + caja
}
```

Auto-calculated fields are computed by the backend on create/update. Display them read-only; recalculate live on the frontend for immediate feedback, but the backend is the source of truth.

**Send email:** `POST /api/venta-dia/{fecha}/enviar` — not implemented yet in the backend. Skip for now or confirm with the backend team.

**Sensitive field toggle (F9):**
- Default: hide `ventaAbarrote` and `ventaSaldo` columns.
- F9 or "Mostrar" button: verify current user's password via `POST /api/auth/login`; on success show fields.

---

## Module: Usuarios (User Management)

Requires `rol === "Admin"` from JWT. Non-admin sees a read-only list.

- `GET /api/usuarios` — list all.
- `POST /api/usuarios` — create (Admin).
- `PUT /api/usuarios/{usuclave}` — update (Admin).
- `PATCH /api/usuarios/{usuclave}/status` — toggle active/inactive (Admin).
- `PUT /api/usuarios/{usuclave}/password` — change password.
  - Admin: no current password required.
  - Self-service: current password required (verified by backend against BCrypt hash).

**[CORRECTED]** `UsuarioDto` includes `rol: string` ("User" | "Admin"). Display it in the table.

---

## Module: Notificaciones

**[CORRECTED]** `status` is an `int`, not a string:
- `1` = Activa
- `0` = Inactiva
- `2` = Terminada

Filter endpoint: `GET /api/notificaciones?idMensaje=&tipo=&prioridad=&status=&usuclave=&usuclaveRecibe=&fechaAlta=&fechaTermino=`

Actions:
- Create: `POST /api/notificaciones`
- Inactivar: `PATCH /api/notificaciones/{id}/inactivar` (sets status → 0)
- Terminado: `PATCH /api/notificaciones/{id}/terminar` (sets status → 2)

---

## Module: Requisición

- On load: `GET /api/requisicion` (returns the current user's draft, identified by JWT).
- Add/update: `PUT /api/requisicion`
- Clear: `DELETE /api/requisicion`
- Send: `POST /api/requisicion/enviar` → generates HTML email via SMTP and clears the list.

**[CORRECTED]** The API uses the JWT `NameIdentifier` claim (`usuclave`) to identify the current user — no user parameter in the URL.

`RequisicionItemDto` shape:
```ts
{ idProducto: string; nombreProducto: string; usuclave: string; cantidad: number }
```

---

## DTO Shapes Reference (Corrected)

### CreateRegistroVentaDto
```ts
{ idProducto: string; precio?: number; cantidad: number; descuento: boolean; numcliente?: number; ncfolio?: number }
```

### DescuentoDto
```ts
{ idProducto: string; cantidadMinima: number; precioDescuento?: number; status: number }
```

### NotaCreditoDto
```ts
{
  numcliente: number; ncfolio: number; status: string; // "AU" | "PC" | "CO" | "CA"
  fechaAlta: string; fechaCompromiso: string;
  monto: number; montoPagado: number;
  items: RegistroNotaCreditoDto[] | null; // null unless fetched separately
}
```

### RegistroNotaCreditoDto
```ts
{ idProducto: string; cantidad: number; precio: number; importe: number; descuento: boolean; fechaSurtido: string }
```

---

## State Management

- **Cart state:** Zustand or React Context. Persist across navigation.
- **Auth state:** AuthContext with `{ token, usuclave, nombre, rol }`. Checked on every protected action.
- **Server state:** TanStack Query (react-query) for all API calls.
- **Notification polling:** `refetchInterval: 10_000` on the active notifications query.
- **Requisition draft:** `localStorage` backup + server sync on every change.

---

## Component Inventory

| Component | Used In |
|-----------|---------|
| `<LookupField>` | Notas de Crédito, Notificaciones |
| `<ConfirmDialog>` | All destructive actions |
| `<PasswordGateDialog>` | Venta del Día F9 sensitive toggle only |
| `<DataTable>` | Every module |
| `<ToastStack>` | Global |
| `<NotificationBar>` | App shell bottom |
| `<NumericInput>` | All numeric/price fields |
| `<CurrencyDisplay>` | All monetary amounts |

---

## Pending Backend Endpoints (Not Yet Implemented)

These are referenced in the original plan but not yet in the backend:

| Endpoint | Module | Status |
|---|---|---|
| `POST /api/venta-dia/{fecha}/enviar` | Venta del Día | Not implemented |
| `PATCH /api/productos/{id}/stock` | Productos | Not implemented — use `PUT /api/productos/{id}` for now |
| Zero-stock email notification | VentaService | Not implemented |
