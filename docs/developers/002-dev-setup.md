# SAPSec.Web – Local Development Setup

This guide explains how to run the **SAPSec.Web** project locally.

---

## Prerequisites

Ensure the following are installed:

* **.NET SDK 8**
* **Node.js (LTS) and npm**
* **PostgreSQL** (or Docker if Postgres is containerised)

Verify installations:

```bash
dotnet --version
node --version
npm --version
```

---

## Clone the repository

```bash
git clone https://github.com/DFE-Digital/sap-sector.git
cd sap-sector
```

---

## Navigate to the Web project

```bash
cd SAPSec.Web
```

---

## Install frontend dependencies

Frontend assets (CSS/JS) require npm dependencies:

```bash
npm install
```

Re-run this if assets appear broken or missing.

---

## Configure user secrets

The following secrets are required to run the web app locally.
Values are provided by the team and **must not be committed**.

```json
{
  "LOGIT_HTTP_URL": "",
  "LOGIT_API_KEY": "",
  "DsiConfiguration:ClientId": "",
  "DsiConfiguration:ClientSecret": "",
  "ConnectionStrings:PostgresConnectionString": ""
}
```

### Set secrets locally

```bash
dotnet user-secrets init

dotnet user-secrets set "LOGIT_HTTP_URL" "<value>"
dotnet user-secrets set "LOGIT_API_KEY" "<value>"

dotnet user-secrets set "DsiConfiguration:ClientId" "<value>"
dotnet user-secrets set "DsiConfiguration:ClientSecret" "<value>"

dotnet user-secrets set "ConnectionStrings:PostgresConnectionString" "<value>"
```

Verify:

```bash
dotnet user-secrets list
```

---

## Database setup (SAPData + local Postgres)

SAPSec.Web expects a local Postgres database populated using the **SAPData** project.

> **Note:** CSV data files must be obtained from the team/storage account and **must not be committed**.

### 1) Install Postgres locally

Install Postgres on your machine (or run it via Docker) and make sure you can connect with `psql`.

### 2) Create a local Postgres database

Create an empty database for local development.

### 3) Get the CSV source data

Download **all CSV files** from the **sap-sector** storage account `s189t01sapsecdptssa`, container `schooldata`, into:

* `SAPData/DataMap/SourceFiles`

Do **not** check these files into git.

### 4) Generate SQL scripts using SAPData

From the repo root:

```bash
cd SAPData

dotnet run
```

This generates the SQL scripts used to create/populate tables and views.

### 5) Run all SQL scripts via psql

From the SQL script directory, run:

```bash
psql -d <DATABASE_NAME>
```

Then inside `psql`:

```sql
\i run_all.sql
```

#### If `run_all.sql` fails due to file encoding

Some scripts can fail due to encoding issues.

Fix by re-saving the failing script(s) as **UTF-8 without signature**:

* Visual Studio: **File → Save As… → Save with encoding… → Unicode (UTF-8 without signature)** (Code page 65001)

Re-run:

```sql
\i run_all.sql
```

### 6) Point SAPSec.Web at the local database

Set the connection string in user secrets:

```bash
dotnet user-secrets set "ConnectionStrings:PostgresConnectionString" "<value>"
```

---

## Run the application

From the `SAPSec.Web` directory:

```bash
dotnet run
```

The application URLs will be shown in the console output.

---

## Common issues

### Frontend assets not loading

```bash
npm install
dotnet run
```

### Search index not initialised

* Required CSV file is missing
* Verify CSV path/configuration and rerun

### Schools not displayed

* Incorrect Postgres connection string
* Database not seeded
* Required scripts not run

---

## Debug checklist

1. Check console logs (first error is usually the root cause)
2. Verify secrets:

   ```bash
   dotnet user-secrets list
   ```
3. Confirm database connectivity using a Postgres client
4. Re-run:

   ```bash
   npm install
   dotnet run
   ```

---

If issues persist, share the **first startup error or stack trace** with the team.
