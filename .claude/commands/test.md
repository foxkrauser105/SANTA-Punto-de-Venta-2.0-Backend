# /test — Run tests and verify coverage

Run the full test suite and report results:

```bash
dotnet test SANTA.PoS.sln --logger "console;verbosity=normal"
```

---

## Testing conventions for this project

### Tech stack
- **Framework:** xUnit
- **Mocking:** Moq 4.x — `new Mock<IInterface>()`
- **AutoMapper:** always use the real `MappingProfile` via `MapperFactory.Create()` (never mock `IMapper`)
- **Required usings:** xUnit is NOT in implicit usings — always add `using Xunit;` explicitly

### Test class structure
- One file per service: `<ServiceName>Tests.cs` in `SANTA.PoS.Tests/Services/`
- Constructor sets up `_sut` (system under test) and mocks
- Shared valid test data goes in a `private static` factory method at the top

### Naming convention
`MethodName_Condition_ExpectedBehaviour`

Examples:
- `CreateAsync_EmptyItems_ThrowsDomainException`
- `GetPrecioParaVentaAsync_ActiveDiscountAndQuantityMet_ReturnsDiscountPrice`

### Coverage expectations
Every service method must have **at least one success path and one failure path** test.

Business rules that must always be covered:
- `null` / empty guard → `DomainException`
- "not found" entity lookup → `DomainException`
- Duplicate / conflict check → `DomainException`
- Happy path → verifies the correct repository method was called and/or the returned DTO has expected values

### Repository mocks
- `_repoMock.Setup(r => r.GetByIdAsync("X")).ReturnsAsync((Entity?)null)` for not-found cases
- Use `.Callback<T>(x => captured = x)` when you need to assert on the entity passed into a write method
- Use `.Verify(r => r.Method(...), Times.Once)` to confirm the right repo call was made

### DomainException assertions
```csharp
var ex = await Assert.ThrowsAsync<DomainException>(() => _sut.Method(dto));
Assert.Contains("keyword", ex.Message); // verify the right exception was thrown
```

### Emptiness checks
Per project rule: never use `.Any()` for emptiness. Use `.Count == 0` for `IList<T>`/`ICollection<T>`, or `.Count() == 0` for bare `IEnumerable<T>`.

### What NOT to test
- Repository implementations (require a real DB — integration tests only)
- Controller routing (covered by integration tests)
- AutoMapper mappings that have no custom `ForMember` (the `MapperFactory` setup already validates the profile compiles)
