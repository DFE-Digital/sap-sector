# `sap-sector` Technical Improvement Plan

**Author:** Based on GSII Testability Review v0.1 ([aahmed-dfe/school-profiles-design-feedback](https://github.com/aahmed-dfe/school-profiles-design-feedback))
**Last Updated:** June 2026
**Status:** Living document

---

## Overview

These are some of the agreed standards for `sap-sector`. They are not tickets — they define **how we write code going forward**. 
Every PR is expected to follow them.

| # | Standard | Status |
|---|---|---|
| 1 | [New features must be built as UseCases](#standard-1--new-features-must-be-built-as-usecases) | Active |
| 2 | [Every new UseCase must have unit tests](#standard-2--every-new-usecase-must-have-unit-tests) | Active |
| 3 | [CancellationToken must be propagated](#standard-3--cancellationtoken-must-be-propagated) | Active |
| 4 | [New Core DTOs must be immutable](#standard-4--new-core-dtos-must-be-immutable) | Active |
| 5 | [Infrastructure types must not enter Core](#standard-5--infrastructure-types-must-not-enter-core) | Active |
| 6 | [Integration test strategy](#standard-6--integration-test-strategy) | TODO |
| 7 | [Key domain identifiers must use ValueObjects](#standard-7--key-domain-identifiers-must-use-valueobjects) | Active |
| 8 | [Search must go through ISearchServiceAdaptor](#standard-8--search-must-go-through-isearchserviceadaptor) | TODO |
| 9 | [Existing Services are retrofitted to UseCases incrementally](#standard-9--existing-services-are-retrofitted-to-usecases-incrementally) | Active |
| 10 | [Measures are modelled as a domain concept](#standard-10--measures-are-modelled-as-a-domain-concept) | Active |

---

## Standard 1 — New Features Must Be Built as UseCases
### Currently this is getting addressed in the SPIKE: Use Cases https://trello.com/c/G7VRi351

**Rule:** All new application behaviour must be implemented using the `IUseCase<TIn, TOut>` pattern. No new `Service` classes should be introduced for application logic.

**Why:** Services are vague, data-centric, and cannot express intent or be easily substituted in tests. UseCases make behaviour explicit, testable, and replaceable.

### Pattern

```csharp
// 1. Define the contract
public interface IUseCase<TIn, TOut>
{
    Task<TOut> HandleAsync(TIn request, CancellationToken cancellationToken);
}

// 2. Implement internally
internal sealed class GetSchoolDetailsUseCase
    : IUseCase<GetSchoolDetailsRequest, GetSchoolDetailsResponse>
{
    private readonly IEstablishmentRepository _repository;

    public GetSchoolDetailsUseCase(IEstablishmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetSchoolDetailsResponse> HandleAsync(
        GetSchoolDetailsRequest request,
        CancellationToken cancellationToken)
    {
        var school = await _repository.GetByUrnAsync(request.Urn, cancellationToken);
        return new GetSchoolDetailsResponse(school);
    }
}

// 3. Register via composition root
public static IServiceCollection AddSchoolDetails(this IServiceCollection services)
{
    services.TryAddScoped<
        IUseCase<GetSchoolDetailsRequest, GetSchoolDetailsResponse>,
        GetSchoolDetailsUseCase>();
    return services;
}

// 4. Resolve in controller via interface only — never the concrete type
public class SchoolController : Controller
{
    private readonly IUseCase<GetSchoolDetailsRequest, GetSchoolDetailsResponse> _getSchoolDetails;

    public SchoolController(
        IUseCase<GetSchoolDetailsRequest, GetSchoolDetailsResponse> getSchoolDetails)
    {
        _getSchoolDetails = getSchoolDetails;
    }
}
```

### Reference

[`FindSimilarSchools.cs`](https://github.com/DFE-Digital/sap-sector/blob/main/SAPSec.Core/Features/SimilarSchools/UseCases/FindSimilarSchools.cs) — existing reference implementation in the codebase.

---

## Standard 2 — Every New UseCase Must Have Unit Tests

**Rule:** Unit tests must be written in the same PR as the UseCase. No new UseCase is considered done without tests. Tests must not touch the database, file system, or HTTP.

**Why:** The test suite is currently dominated by UI/integration tests. Unit tests run at build, give fast feedback, and validate business logic in isolation.

### Pattern

```csharp
public class GetSchoolDetailsUseCaseTests
{
    [Fact]
    public async Task HandleAsync_ValidUrn_ReturnsSchoolDetails()
    {
        // Arrange
        var mockRepository = new Mock<IEstablishmentRepository>();
        mockRepository
            .Setup(r => r.GetByUrnAsync("123456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Establishment("123456", "Test School"));

        var useCase = new GetSchoolDetailsUseCase(mockRepository.Object);

        // Act
        var response = await useCase.HandleAsync(
            new GetSchoolDetailsRequest("123456"),
            CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.SchoolName.Should().Be("Test School");
    }

    [Fact]
    public async Task HandleAsync_UnknownUrn_ThrowsNotFoundException()
    {
        // Arrange
        var mockRepository = new Mock<IEstablishmentRepository>();
        mockRepository
            .Setup(r => r.GetByUrnAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("School not found"));

        var useCase = new GetSchoolDetailsUseCase(mockRepository.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            useCase.HandleAsync(
                new GetSchoolDetailsRequest("000000"),
                CancellationToken.None));
    }
}
```

### Conventions

- **Location:** `Tests/SAPSec.Core.Tests/Features/{FeatureName}/UseCases/`
- **Minimum coverage per UseCase:** happy path, not-found/empty result, error/exception
- **All dependencies mocked** — no real database, file system, or HTTP calls

---

## Standard 3 — CancellationToken Must Be Propagated

**Rule:** All new async UseCase methods, repository interfaces, and service calls must accept and forward a `CancellationToken`. Controllers must pass `HttpContext.RequestAborted`.

**Why:** Without `CancellationToken`, async operations (database queries, HTTP calls) cannot be cancelled when a user navigates away or a request times out, wasting compute and holding connections open unnecessarily.

### Pattern

```csharp
// UseCase contract — token on the interface
public interface IUseCase<TIn, TOut>
{
    Task<TOut> HandleAsync(TIn request, CancellationToken cancellationToken);
}

// Repository interface — token flows down
public interface IAttendanceRepository
{
    Task<AttendanceMeasures> GetByUrnAsync(string urn, CancellationToken cancellationToken);
}

// Controller — pass the ASP.NET Core request token
public async Task<IActionResult> Attendance(string urn, CancellationToken cancellationToken)
{
    var response = await _getAttendanceMeasures.HandleAsync(
        new GetAttendanceMeasuresRequest(urn),
        cancellationToken); // ← NOT CancellationToken.None

    return View(response);
}
```

### What Not To Do

```csharp
// Swallows cancellation — do not do this
await _repository.GetByUrnAsync(urn, CancellationToken.None);

// Missing token entirely — do not do this
Task<AttendanceMeasures> GetByUrnAsync(string urn);
```

---

## Standard 4 — New Core DTOs Must Be Immutable

**Rule:** All response types returned from UseCases must use C# `record` types or `init`-only properties. No public `set` is permitted on any new DTO in the Core layer.

**Why:** Mutable DTOs allow controllers and views to accidentally mutate domain data, embedding business logic where it does not belong and introducing hard-to-trace bugs.

### Pattern

```csharp
// Preferred — record
public record GetSchoolDetailsResponse(
    string Urn,
    string Name,
    string PhaseOfEducation);

// Not allowed on new types
public class GetSchoolDetailsResponse
{
    public string Urn { get; set; }   // mutable — not permitted
    public string Name { get; set; }
}
```

### Mapping to DTOs

Use a mapper within the UseCase to project from the domain model to the response DTO. The DTO is never constructed by a caller — only by the UseCase itself.

```csharp
public async Task<GetSchoolDetailsResponse> HandleAsync(
    GetSchoolDetailsRequest request,
    CancellationToken cancellationToken)
{
    var school = await _repository.GetByUrnAsync(request.Urn, cancellationToken);
    return _mapper.Map(school); // ← DTO constructed here, immutable from this point
}
```

---

## Standard 5 — Infrastructure Types Must Not Enter Core

**Rule:** Types shaped by database schema, JSON file structure, or any persistence format belong in `SAPSec.Infrastructure` only. Core must define its own application models. Repositories must map from infrastructure types to Core models before returning.

**Why:** When persistence-shaped types live in Core, a database or file format change forces a Core change, coupling layers that clean architecture deliberately separates.

### Pattern

```csharp
//  Do not put this in SAPSec.Core
public class EstablishmentDataRecord
{
    public string urn { get; set; }             // database column name
    public string establishment_name { get; set; }
}

// Infrastructure layer only — shaped by the JSON/DB source
// Location: SAPSec.Infrastructure/Repositories/Records/
internal sealed class EstablishmentDataRecord
{
    public string urn { get; set; }
    public string establishment_name { get; set; }
}

// Core layer — shaped by application need
// Location: SAPSec.Core/Features/SchoolDetails/
public sealed class Establishment
{
    public Establishment(string urn, string name)
    {
        Urn = urn;
        Name = name;
    }
    public string Urn { get; }
    public string Name { get; }
}

// Repository maps at the boundary — Core never sees the raw record
public sealed class JsonEstablishmentRepository : IEstablishmentRepository
{
    private readonly IMapper<EstablishmentDataRecord, Establishment> _mapper;

    public async Task<Establishment> GetByUrnAsync(string urn, CancellationToken ct)
    {
        var record = await _source.ReadAsync(urn, ct);
        return _mapper.Map(record); // ← mapping stays in Infrastructure
    }
}
```

### Layer Rules

| Layer | May reference | May NOT reference |
|---|---|---|
| `SAPSec.Core` | Nothing outside Core | Infrastructure, Web |
| `SAPSec.Infrastructure` | Core contracts | Web |
| `SAPSec.Web` | Core contracts | Infrastructure directly |

---

## Standard 6 — Integration Test Strategy

> ⚠️ **TODO** — Strategy to be agreed. Placeholder below captures the agreed direction.

**Direction:** Integration tests should use `WebApplicationFactory` to exercise the full HTTP pipeline. Test data fixtures (JSON files) should be checked into the `Tests/` directory. The current `FakeRepository` approach is to be phased out for new features.

**References:**
- [Issue #4 — Integration test at build](https://github.com/aahmed-dfe/school-profiles-design-feedback/issues/4)
- [Issue #2 — Data testing](https://github.com/aahmed-dfe/school-profiles-design-feedback/issues/2)

---

## Standard 7 — Key Domain Identifiers Must Use ValueObjects

**Rule:** Domain identifiers with known rules (`URN`, `LocalAuthorityCode`, `AcademicYear`) must be represented as strongly-typed ValueObjects, not raw `string` or `int` primitives. New code must use these types. Existing code is migrated incrementally.

**Why:** Primitive obsession means any arbitrary string can be passed as a URN with no compile-time safety and no centralised validation. ValueObjects catch invalid values at construction and remove parsing responsibility from all consumers.

### Pattern

```csharp
// URN — 5 to 7 digit numeric string
public readonly struct UniqueReferenceNumber
{
    public UniqueReferenceNumber(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (!value.All(char.IsDigit) || value.Length is < 5 or > 7)
            throw new ArgumentException("URN must be 5–7 digits.", nameof(value));

        Value = value;
    }

    public string Value { get; }
    public override string ToString() => Value;
}

// Local Authority Code — numeric, 0–999
public readonly struct LocalAuthorityCode
{
    public LocalAuthorityCode(int code)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(code);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(code, 999);
        Code = code;
    }

    public int Code { get; }
}

// Usage in a Core model
public sealed class Establishment
{
    public Establishment(UniqueReferenceNumber urn, LocalAuthorityCode laCode)
    {
        Urn = urn;
        LocalAuthority = laCode;
    }

    public UniqueReferenceNumber Urn { get; }
    public LocalAuthorityCode LocalAuthority { get; }
}
```

### Migration Approach

ValueObjects are introduced **incrementally**, one model per sprint. A raw `string urn` on an existing type is not changed until that type is touched by feature work — no big-bang migration.

**Location:** `SAPSec.Core/Domain/ValueObjects/`

---

## Standard 8 — Search Must Go Through `ISearchServiceAdaptor`

> ⚠️ **TODO** — Interface design to be agreed prior to implementation.

**Direction:** The existing Lucene implementation should be wrapped behind an `ISearchServiceAdaptor` contract. This decouples the application from Lucene and enables the planned migration to Postgres text search without changing any consumer.

```csharp
// Target interface — to be agreed
public interface ISearchServiceAdaptor
{
    Task<SearchResult> SearchAsync(SearchQuery query, CancellationToken cancellationToken);
}

// Registration — swapping implementations becomes a one-line change
// Today:
services.AddLuceneSearch();

// When Postgres is ready:
services.AddPostgresSearch();
```

**References:**
- [`infrastructure/share-infrastructure.md`](https://github.com/aahmed-dfe/school-profiles-design-feedback/blob/main/infrastructure/share-infrastructure.md)
- [Issue #5 — Feedback](https://github.com/aahmed-dfe/school-profiles-design-feedback/issues/5): *"Aim to move to Postgres and away from Lucene"*

---

## Standard 9 — Existing Services Are Retrofitted to UseCases Incrementally
### Currently this is getting addressed in the SPIKE:Use Cases https://trello.com/c/G7VRi351

**Rule:** Existing `Service` classes are not replaced all at once. Each sprint, Services consumed by controllers that are being touched for feature work are converted to UseCases as part of that ticket. No Service-to-UseCase conversion should be a standalone large refactor.

**Why:** A big-bang rewrite is high risk. Incremental conversion tied to feature work ensures each conversion is tested and reviewed in context.

### Retrofit Priority Order

| Service | Consumed by | Priority |
|---|---|---|
| `ISchoolDetailsService` | `SchoolController`, `SimilarSchoolsController` | High — touched most often |
| `IUserService` | `OrganisationController` | Medium |
| `IDsiClient` | Auth flows | Low — external dependency |

### Pattern

Follow Standard 1 for the target UseCase structure. The conversion steps for any Service are:

1. Create `{Action}UseCase` in `SAPSec.Core/Features/{Feature}/UseCases/`
2. Move logic from `Service` into `UseCase`
3. Update DI registration — replace `AddScoped<IService, Impl>` with `AddScoped<IUseCase<TIn,TOut>, Impl>`
4. Update controller constructor to receive `IUseCase<TIn, TOut>`
5. Write unit tests for the new UseCase
6. Delete the old `Service` interface and implementation once all consumers are migrated

---

## Standard 10 — Measures Are Modelled as a Domain Concept
> ⚠️ **TODO** — To be agreed prior to implementation. Currently this is getting addressed in the SPIKE:Components https://trello.com/c/2nK4EItQ

**Rule:** A `Measure` (e.g. Attendance, Progress8, Attainment8) is a first-class domain concept with a typed identifier and a typed value. New measure-related features must use these types. Raw `decimal?`, `string`, or `null` must not be passed between layers as measure data.

**Why:** A measure value can take three distinct forms — scalar, estimated with a confidence interval, or redacted. Without a domain model, every consumer must handle all three forms independently, duplicating logic and spreading invariants across the codebase.

### Value Forms

| Form | Example | Type |
|---|---|---|
| Scalar | `32.59` | `ScalarMeasureValue` |
| Estimated | Progress8: `-1.27 to 0.88 (0.21)` | `EstimatedMeasureValue` |
| Redacted | `NA`, `SUPP`, `LOWCOV` | `RedactedMeasureValue` |

### Pattern

```csharp
// Measure identifier — what is being measured and for whom
public sealed record MeasureIdentifier(string Name, string Scope, string Cohort);
// e.g. new MeasureIdentifier("Attendance", "National", "Girls")

// Value forms — use pattern matching to handle each form
public abstract record MeasureValue;
public sealed record ScalarMeasureValue(decimal Value) : MeasureValue;
public sealed record EstimatedMeasureValue(decimal Point, decimal Lower, decimal Upper)
    : MeasureValue;
public sealed record RedactedMeasureValue(string Reason) : MeasureValue; // NA, SUPP, LOWCOV

// Domain contract
public interface IMeasure
{
    MeasureIdentifier Id { get; }
    MeasureValue Value { get; }
    bool CanCompareTo(IMeasure other);
}

// Implementation — comparison rules live here, not in views or controllers
public sealed class Measure : IMeasure
{
    public MeasureIdentifier Id { get; }
    public MeasureValue Value { get; }

    public bool CanCompareTo(IMeasure other)
    {
        // Cannot compare across different measure types
        if (!Id.Name.Equals(other.Id.Name, StringComparison.OrdinalIgnoreCase))
            return false;

        // Cannot compare a redacted value to anything
        if (Value is RedactedMeasureValue || other.Value is RedactedMeasureValue)
            return false;

        return true;
    }
}

// Consumer — handles all forms cleanly via pattern matching
string DisplayMeasure(IMeasure measure) => measure.Value switch
{
    ScalarMeasureValue s       => s.Value.ToString("0.00"),
    EstimatedMeasureValue e    => $"{e.Point:0.00} ({e.Lower:0.00} to {e.Upper:0.00})",
    RedactedMeasureValue r     => r.Reason,
    _                          => "Unknown"
};
```

### Location

`SAPSec.Core/Domain/Measures/`

---

## Appendix — Reference Links

| Resource | Link |
|---|---|
| Testability Review v0.1 | [aahmed-dfe/school-profiles-design-feedback](https://github.com/aahmed-dfe/school-profiles-design-feedback) |
| Build to contract — UseCases | [core/build-to-contract-usecases.md](https://github.com/aahmed-dfe/school-profiles-design-feedback/blob/main/core/build-to-contract-usecases.md) |
| Use ValueObjects | [core/use-value-objects.md](https://github.com/aahmed-dfe/school-profiles-design-feedback/blob/main/core/use-value-objects.md) |
| Keep data concerns in Infrastructure | [core/keep-data-concerns-in-infrastructure.md](https://github.com/aahmed-dfe/school-profiles-design-feedback/blob/main/core/keep-data-concerns-in-infrastructure.md) |
| Keep Core DTOs immutable | [core/keep-core-dtos-immutable.md](https://github.com/aahmed-dfe/school-profiles-design-feedback/blob/main/core/keep-core-dtos-immutable.md) |
| Use CancellationTokens | [core/use-cancellation-tokens.md](https://github.com/aahmed-dfe/school-profiles-design-feedback/blob/main/core/use-cancellation-tokens.md) |
| Share infrastructure | [infrastructure/share-infrastructure.md](https://github.com/aahmed-dfe/school-profiles-design-feedback/blob/main/infrastructure/share-infrastructure.md) |
| Model the problem | [core/model-the-problem.md](https://github.com/aahmed-dfe/school-profiles-design-feedback/blob/main/core/model-the-problem.md) |
| Existing reference UseCase | [FindSimilarSchools.cs](https://github.com/DFE-Digital/sap-sector/blob/main/SAPSec.Core/Features/SimilarSchools/UseCases/FindSimilarSchools.cs) |