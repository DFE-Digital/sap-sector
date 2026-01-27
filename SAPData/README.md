# SAP Data Platform

## For stakeholders

This repository implements an **automated, repeatable data ingestion and analytics pipeline** for UK school data.
It downloads official public datasets, detects changes, ingests them into PostgreSQL, and builds a curated
data layer used for display in web front-end.

Key outcomes:
- Fully automated daily ingestion (with early-exit when data has not changed)
- Strong data governance via raw → staging → curated layers
- Deterministic, auditable SQL generation
- Scalable foundation for public and sector websites

The system is designed to be **low-risk, transparent, and maintainable**, with all transformations defined in SQL
and all schema generation driven from metadata.

---

## Overview

This repository contains the tooling, SQL, and pipeline configuration used to ingest, normalise, and publish
UK school datasets into a PostgreSQL-based analytics warehouse.

The workflow is intentionally simple and deterministic:
1. Download raw source files
2. Normalise and clean CSV inputs
3. Generate SQL to create raw tables and load data
4. Build staging, dimension, fact tables, and views
5. Apply indexes and validation checks

---

## Prerequisites for running locally

Before running locally, ensure you:

1. Obtain the raw data files and place them in `SAPData/DataMap/SourceFiles`
2. Have pgAdmin installed locally (includes psql SQL Shell which is needed to run '02_copy_into_raw_local.sql')

---

## Why raw data files are not stored in Git

Raw data files are **intentionally excluded** from this repository.

Reasons:
- The source CSV files are large and frequently exceed GitHub size limits
- The data is reproducible from official public sources
- Storing raw data in Git provides no audit or recovery benefit

Instead:
- Raw files are downloaded during the pipeline run
- File hashes are computed and stored to detect changes
- The pipeline exits early if no data has changed

This keeps the repository lightweight and avoids unnecessary version churn.

---

## SQL generation approach

All SQL is **generated automatically** by a .NET console application.

Generated files include:
- `01_create_raw_tables.sql`
- `02_copy_into_raw.sql`
- `02_copy_into_raw_local.sql`
- View, index, and validation scripts

The generation process ensures:
- Stable, deterministic table names (using short hashes)
- Safe handling of malformed CSV rows
- Consistent schemas across pipeline runs
- Full traceability from source file → raw table → analytics view

---

## Why there are two COPY scripts

Two COPY scripts are generated intentionally:

### `02_copy_into_raw.sql`
Used by the **CI/CD pipeline**.
- Uses standard PostgreSQL `COPY` - this command cannot be run locally in pgpAdmin
- Assumes files are accessible to the database host

### `02_copy_into_raw_local.sql`
Used for **local development**.
- Uses `\copy` via the `psql` client (use SQL Shell included with pgAdmin)
- Reads files from the developer’s local filesystem

---

## Pipeline execution (GitHub Actions)

The ingestion pipeline runs:
- On a daily schedule
- Manually via workflow dispatch

High-level steps:
1. Checkout repository
2. Install PostgreSQL client and .NET SDK
3. Download raw datasets
4. Compute and compare file hashes
5. Exit early if unchanged
6. Generate SQL scripts
7. Execute `run-all.sql`
8. Commit updated hashes

---

## Local development

Typical local workflow:
1. Place raw CSVs in `SAPData/DataMap/SourceFiles`
2. Run the SQL generator (`dotnet run`, or set as startup project in Visual Studio and run in VS)
3. Execute `02_copy_into_raw_local.sql` using psql SQL Shell (Command: "\i '<PATH TO SQL FOLDER>/Sql/02_copy_into_raw_local.sql'")
4. Run `run-all.sql` against a local PostgreSQL instance, or run individual sql files as needed.
5. Set file encoding to UTF8 without signature code page 65001

---

## Design principles

- SQL-first transformations
- Metadata-driven generation
- No hidden runtime logic
- Idempotent, restartable pipeline
- Clear separation of concerns

---

## Repository structure (simplified)

- `SAPData/`
  - `DataMap/SourceFiles` – downloaded source files (not committed)
  - `DataMap/CleanedFiles` – normalised CSVs (not committed)
  - `Sql/` – generated SQL scripts
- `scripts/` – PowerShell helpers
- `.github/workflows/` – pipeline definition

---


