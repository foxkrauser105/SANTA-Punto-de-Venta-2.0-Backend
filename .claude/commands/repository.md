# /repository — Repository implementation rules

## Always extend BaseRepository

Every repository **must** extend `BaseRepository<TEntity, TKey>` and implement `IBaseRepository<TEntity, TKey>` unless one of the two exceptions below applies.

```csharp
// Interface
public interface IXxxRepository : IBaseRepository<Xxx, int>
{
    // domain-specific methods only; CreateAsync / UpdateAsync / DeleteAsync / GetByIdAsync / GetAllAsync come for free
}

// Implementation
public class XxxRepository(SantaContext context) : BaseRepository<Xxx, int>(context), IXxxRepository
{
    protected override DbSet<Xxx> GetDbSet() => _context.Xxxs;

    // override GetByIdAsync / DeleteAsync only when you need eager-loading or a custom lookup key
    // add domain-specific methods on top
}
```

### Why
`BaseRepository` provides `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `GetByIdAsync`, and `GetAllAsync` for free. Skipping it duplicates those five methods in every repository that avoids it — a maintenance liability.

---

## Exceptions — when NOT to use BaseRepository

### 1. Composite primary key
`BaseRepository<T, TKey>` takes a single `TKey`. EF Core's `FindAsync` requires `params object[]` for composite keys, which does not map cleanly to the generic. Skip `BaseRepository` when the entity's PK is composite.

**Examples in this codebase:** `NotaCreditoRepository` (PK: `numcliente` + `ncfolio`), `RequisicionRepository` (PK: `usuclave` + `id_producto`).

### 2. No meaningful single-entity CRUD
Some entities are always operated on as a collection owned by a parent (e.g., a user's requisition list). If `GetByIdAsync`, `CreateAsync`, `DeleteAsync` would never be called individually and would be misleading on the interface, omit `IBaseRepository` and define only the methods that make sense.

---

## Override rules

- Override `GetByIdAsync` when the entity must be fetched with `Include`/`ThenInclude` instead of a plain `FindAsync`.
- Override `DeleteAsync` when deletion requires fetching first (e.g., by a non-PK field) before removing.
- Override `GetAllAsync` when all rows should always be returned with navigation properties.
- Do **not** re-implement `CreateAsync` or `UpdateAsync` unless you need transactional logic — the base versions cover the standard case.

---

## Checklist when adding a new repository

1. Does the entity have a **single-column PK**? → extend `BaseRepository<TEntity, TKey>`.
2. Does it have a **composite PK**? → skip `BaseRepository`, implement only the methods needed.
3. Did you register the pair as `Scoped` in `Program.cs`?
   ```csharp
   builder.Services.AddScoped<IXxxRepository, XxxRepository>();
   ```
4. Did you implement `GetDbSet()`?
5. Are domain-specific methods added to the **interface** as well, not just the class?
