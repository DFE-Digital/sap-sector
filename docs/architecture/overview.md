# Architecture overview

SAP Sector is a sector-facing school information service. Authenticated users can search for schools, view establishment and performance information, and compare schools using curated and derived education datasets.

Upstream education datasets are ingested by a data pipeline, which cleans, normalises and loads them into PostgreSQL. The pipeline owns the database structure and is the only thing that writes to it. A web application hosted in Azure Kubernetes Service (AKS) reads from that database read-only, and uses a search index for search journeys. Users authenticate through DfE Sign-in (DSI), and the service is authenticated by default.

![High-level Diagram](../_assets/HLD.png "High Level Diagram showing all major components")

This page is a short orientation only. The detail is in the documents below.

---

## Where to find things

| If you want to know                                                                                    | Read                                                                  |
| ------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------- |
| system boundary, users, data sources, data flows, C4 context and container views, non-functional posture | [High-Level Design (HLD)](./high-level-design.md)                     |
| application structure, projects and layers, components, patterns, implementation detail                  | [Low-Level Design (LLD)](./low-level-design.md)                       |
| database entities, relationships, materialised views and field-level detail                              | [Entity Relationship Diagram (ERD)](./entity-relationship-diagram.md) |
| why a specific decision was made                                                                         | [Architecture Decision Records](../adrs/)                             |

---

## Key architectural characteristics

**The pipeline owns the schema and the application only reads.** The data pipeline defines and refreshes the database structure. There are no runtime migrations and no ORM-managed schema. See [Data ownership and access model](./high-level-design.md#51-data-ownership-and-access-model) in the HLD.

**The query surface is materialised views, not base tables.** The application queries pre-shaped views rather than working out relationships at runtime. See [Data ownership and access model](./high-level-design.md#51-data-ownership-and-access-model) in the HLD.

**Search currently runs on a Lucene index.** The plan is to move to PostgreSQL full-text search, in line with what SAP Public are doing. See [Search data and direction of travel](./high-level-design.md#53-search-data-and-direction-of-travel) in the HLD.

**The service is authenticated by default.** DfE Sign-in provides identity, and a fallback authorization policy protects routes unless they are explicitly opened. See [Users and user types](./high-level-design.md#4-users-and-user-types) in the HLD.

**Ofsted data is used indirectly.** It arrives through upstream departmental datasets rather than a direct integration, and feeds the similar schools comparison. See [Summary table of data sources and logical domains](./high-level-design.md#64-summary-table-of-data-sources-and-logical-domains) in the HLD.
