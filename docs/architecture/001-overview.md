# Architecture overview

SAP Sector currently uses the following high-level architectural approach:

![High-level Diagram](../_assets/HLD.png "High Level Diagram showing all major components")


The main entry point to the application is an **ASP.NET Core MVC application** targeting **.NET 8**, hosted in Azure Kubernetes Service (AKS) and serving sector-facing web endpoints.

**SAP Sector is primarily an authenticated service**. Users access the platform through **DfE Sign-in (DSI)**, and the application applies authorization by default through its fallback policy. The service is intended to support sector users such as schools, trusts, and local authority users who need to search for schools, view school detail, and compare schools using curated and derived education datasets.

---

## SAPSec.Web

`SAPSec.Web` is the presentation layer of the service.

It contains the web application concerns required to receive requests, manage user context, and render the user experience.

### Responsibilities

- **MVC controllers**
  - receive and coordinate HTTP requests
  - invoke application logic
  - shape responses for the UI

- **Views and ViewModels**
  - render user-facing pages
  - use the **GOV.UK Design System** and DfE styling/components
  - shape data for presentation without embedding business rules

- **Authentication and authorization integration**
  - integrates with **DfE Sign-in (DSI)**
  - applies authentication and access policies
  - supports protected routes by default

- **Middleware and web concerns**
  - session handling
  - anti-forgery protections
  - security headers / content security policy
  - forwarded headers and HTTPS behaviour
  - health endpoints and request pipeline configuration


---

## SAPSec.Core

`SAPSec.Core` contains the business logic and application abstractions.

This project represents the centre of the application and is where domain behaviour should live.

### Responsibilities

- **Core / domain entities**
  - represent the business concepts used throughout the service
  - currently include school- and dataset-related concepts used by search, detail, and comparison journeys

- **Interfaces**
  - define contracts for repositories, services, and technical dependencies
  - support dependency inversion so that the Core remains independent of infrastructure details

- **Use cases / application services**
  - provide clear entry points for specific application behaviours
  - orchestrate retrieval, validation, transformation, and business rules before returning data to the web layer

- **Rules and validation**
  - contain service-level business checks and behavioural constraints
  - ensure logic is implemented consistently and remains testable

The core should not depend on Web and should not contain direct infrastructure or framework-specific implementation.

---

## SAPSec.Infrastructure

`SAPSec.Infrastructure` is the implementation layer for persistence, search, and external technical integrations.

It is responsible for connecting the application to data stores, search technologies, and supporting technical services.

### Responsibilities

- **Repositories**
  - retrieve and prepare data for use by the Core
  - currently use **PostgreSQL** access patterns implemented with **Dapper** and **Npgsql**
  - support asynchronous querying and retrieval of authoritative service data

- **Lucene search**
  - provides the implementation for search capabilities
  - contains the indexing and query logic required for school search
  - exposes search through abstractions so that controllers do not depend directly on Lucene

- **Generated / packaged data**
  - includes generated or packaged JSON assets used by the application
  - supports structured loading of supporting datasets and reference information

- **External integrations**
  - technical integration points that do not belong in the Core
  - operational or storage-related dependencies may be implemented here

Infrastructure is where technical choices are expressed, but those choices should remain behind interfaces where possible.

---

## SAPData

To provide data in a reliable and repeatable format, the repository includes a dedicated **SAPData** area.

This part of the repository supports ingestion, transformation, and loading of the datasets needed by the SAP Sector service.

### Responsibilities

- **Data acquisition**
  - source external datasets from upstream providers
  - support recurring or scheduled refreshes

- **Data mapping and shaping**
  - use metadata-driven approaches to describe how source data should be transformed
  - ensure source data can be normalised into the shape required by the service

- **SQL generation**
  - generate SQL scripts from metadata and source structures
  - produce deterministic outputs for raw, staging, and curated layers

- **Pipeline execution**
  - support automated execution through GitHub Actions
  - detect whether files have changed using hashes
  - avoid unnecessary work when source data has not changed

- **Database refresh / rebuild**
  - load processed data into PostgreSQL
  - support downstream use by the application and search index

The broad goal of `SAPData` is to make data refreshes reliable, repeatable, and auditable.

---

## Search and data model approach

SAP Sector uses a **dual data-access model**:

- **PostgreSQL** is the **authoritative source of structured data**
- **Lucene** is a **derived search index** optimised for user search behaviour

This allows the service to use the most appropriate technology for each need:

- **Lucene** for fast and flexible text search
- **PostgreSQL** for authoritative retrieval, comparison logic, and structured data access

This also means that schema or indexing changes may require index rebuilds, while PostgreSQL remains the source of truth.

---

## Authentication and access approach

The service integrates with **DfE Sign-in (DSI)** for user authentication.

At a high level:

- users authenticate through DSI before accessing the service
- the web application establishes identity and claims context
- authorization policies are then applied to secure the service
- application routes are protected by default unless explicitly opened

---

## Hosting and runtime model

The service is designed to run in **Azure Kubernetes Service (AKS)**.

At runtime this enables:

- container-based deployment
- health-checked rollout behaviour
- support for review/test environments
- secure distributed hosting
- operational integration with deployment, monitoring, and maintenance processes

The repository also contains a **maintenance page** that can be deployed separately and used during maintenance or failover scenarios.

---

## Testing approach

The repository includes a broad testing structure under `Tests/`, including:

- unit tests
- integration tests
- web tests
- end-to-end tests
- UI tests
- accessibility tests

This reflects a layered test strategy where:

- business logic is tested in isolation
- infrastructure and integration points are tested explicitly
- user journeys and accessibility behaviours are validated at higher levels

---



## Summary

SAP Sector follows a layered architecture that separates:

- **presentation concerns** in `SAPSec.Web`
- **business logic** in `SAPSec.Core`
- **technical implementation** in `SAPSec.Infrastructure`
- **data ingestion and preparation** in `SAPData`

This approach supports maintainability, testability, and clear separation of concerns, while allowing the platform to combine curated education data, secure authenticated access, and search-driven user journeys in a scalable cloud-hosted service.