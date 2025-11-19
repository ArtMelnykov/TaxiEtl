# Taxi Data ETL

Small .NET 8 CLI that takes the yellow taxi CSV, cleans it up, and loads the interesting columns into SQL Server. I split the solution the same way I usually do for ETL toy projects:

- `TaxiDataETL.Core` – models plus the ETL pipeline (CSV reader, normalization, simple in-memory deduplication, dump of duplicates).
- `TaxiDataETL.Data` – `SqlConnectionFactory` and a repository that wraps `SqlBulkCopy`.
- `TaxiDataETL.CLI` – entry point with HostBuilder, DI wiring, Serilog.

I used the sample CSV from the task and checked the output with the queries listed below.

## What you need

- .NET 8 SDK
- SQL Server (local install/Docker/Azure SQL all work)
- Input CSV (I keep a copy under `src/TaxiDataETL.CLI/data/sample-cab-data.csv` for reference)

## Getting the project ready (local SQL Server)

1. Create database `TaxiDb` on your SQL Server instance and run `sql/01_create_db_and_tables.sql`. The script builds `dbo.TaxiTrips` with the required columns.
2. Update `ConnectionStrings:TaxiDb` in `src/TaxiDataETL.CLI/appsettings.json` or override it via `ConnectionStrings__TaxiDb`. I keep secrets outside Git; `dotnet user-secrets` or env vars are fine.
3. Adjust CSV settings if your paths differ: `Csv:InputPath`, `Csv:DuplicatesOutputPath`, `Csv:BatchSize`, `Csv:SourceTimeZone` (needed to convert EST timestamps to UTC).

## Getting the project ready (Docker Compose)

`docker-compose.yml` spins up everything for you: `db` (SQL Server), `db-init` (one-off schema creator), `taxidata-etl` (the CLI), and `db-test-sql` (optional verification queries).

1. Copy `.env.template` to `.env` and populate it.
   - `MSSQL_SA_PASSWORD` must meet SQL Server complexity rules.
   - Keep `DB_CONNECTION_STRING=Server=db;Database=TaxiDb;User Id=sa;Password=<your_password>;TrustServerCertificate=True;Encrypt=True;`. Inside Docker, the hostname for SQL Server is `db`, so no other changes are needed.
2. Build the CLI image (or rebuild after code changes) and start SQL Server:
   ```bash
   docker compose build
   docker compose up -d db
   ```
3. Initialize the database by running the helper container. It waits for SQL Server to be healthy, creates `TaxiDb` if needed, and executes `sql/01_create_db_and_tables.sql`:
   ```bash
   docker compose run --rm db-init
   ```
4. Run the ETL. This command mounts `src/TaxiDataETL.CLI/data` into the container and processes `sample-cab-data.csv`:
   ```bash
   docker compose run --rm taxidata-etl
   ```
   Use `docker compose up taxidata-etl` if you prefer to keep the container attached and let Compose handle dependencies automatically.
5. (Optional) Execute the assignment-check queries against the freshly loaded data:
   ```bash
   docker compose run --rm db-test-sql
   ```
6. To ingest another CSV or change where duplicates land, either edit the `Csv__*` variables under the `taxidata-etl` service or override them inline, e.g. `docker compose run -e Csv__InputPath=/app/data/2020.csv --rm taxidata-etl`. Adjust the bind mount if the source file lives outside `src/TaxiDataETL.CLI/data`.

## Running the ETL

```bash
dotnet restore
dotnet build
dotnet run --project src/TaxiDataETL.CLI
```

Under the hood it:

1. Streams the CSV with `CsvHelper`.
2. Trims text fields, turns `store_and_fwd_flag` into `Yes/No`, converts pickup/dropoff time from the source timezone to UTC.
3. Tracks duplicates by `(PickupDateTime, DropoffDateTime, PassengerCount)` and writes them to `duplicates.csv`.
4. Pushes batches (configurable) via `SqlBulkCopy`.

## SQL helpers

- `sql/01_create_db_and_tables.sql` – recreates the schema.
- `sql/02_queries.sql` – quick sanity checks: total rows plus duplicates grouped by the key.
- `sql/03_test_assignment_queries_check.sql` – ready queries from the spec: average tips per PULocationID, top-100 by distance, top-100 by travel time, search by `PULocationID`.

## Sanity check after the run

1. Run `sql/02_queries.sql` to confirm the row count and catch any duplicates.
2. Look at `duplicates.csv` to see what was filtered out.
3. Execute `sql/03_test_assignment_queries_check.sql` to make sure the schema serves the four queries from the assignment.

## Notes and caveats

- Deduplication lives in memory right now. Works for the sample file, but at ~10 GB I'd switch to a staging table with a unique index or `MERGE` to keep memory stable.
- Validation is basic: if `CsvHelper` can parse the values, I trust them. A production version would add FluentValidation/custom checks and better logging for rows we skip.
- Docker SQL Server was acting up on my machine, so I ran the scripts against a local instance. Once Docker is back I only need to swap the connection string.

## If the input grows to ~10 GB

1. Move deduplication into SQL (staging table + unique index or `MERGE`).
2. Add indexes that match the expected queries (e.g., by `PULocationID`, by distance, and maybe a computed duration column with an index).
3. Consider parallel ingestion and `SqlBulkCopy` tweaks (`TableLock`, multiple batches in flight).
4. Add stronger validation/metrics so it’s easier to see progress and failures during a long run.

## Open TODOs

- Tests for normalization/deduplication.
- Validation layer for input rows.
- Database-backed deduplication so repeated runs don’t insert the same data.
- Clean up `appsettings.template.json` so it mirrors the real configuration keys.
