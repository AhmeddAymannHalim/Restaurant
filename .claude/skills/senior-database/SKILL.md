---
name: senior-database
description: Verifies that a project's database really contains what its code saves. Works on any project and any SQL database from just a connection string - reads the code to find every place that saves, updates or deletes, derives what the data must look like, then queries the database and reports missing, wrong, orphaned or stale data. Use when the user says senior-database, "check the database", "is the data saved correctly", or gives a connection string to verify.
---

# Senior Database

Input: a **connection string** (from the user, or found in `appsettings*.json`, `.env`, `web.config`). Nothing else is project-specific: the code tells you what to expect, the database tells you what is there, and the difference is the report.

**Read-only.** Run SELECT only. Never INSERT/UPDATE/DELETE, never open a write transaction, never run migrations, never edit code during the audit. Fixes are proposed and applied only after the user approves them. Schema changes go through the project's own migration tool, not raw SQL. Never print the password from the connection string; mask it. Never commit or push.

## 1. Connect

1. Get the connection string from the environment's settings file, unless the user gives one explicitly:
   - **Development** -> `appsettings.Development.json` (`ConnectionStrings`).
   - **Production** -> `appsettings.Production.json` if it exists, otherwise `appsettings.json`.
   - Decide the environment from the user's words, then `ASPNETCORE_ENVIRONMENT` / `launchSettings.json`. Default to Development. Auditing Production only when the user says so, and say which file and server you are connecting to before running anything.
   - If the chosen file has no connection string, fall back to `appsettings.json`, and tell the user which one was used.
2. Pick a way to run queries, in order: an MCP SQL tool already connected to that database; `sqlcmd`; `psql`/`mysql` for those engines; otherwise a throwaway script outside the project (PowerShell with `System.Data.SqlClient` for SQL Server) that takes the connection string as an argument and runs a SELECT.
3. Confirm with `SELECT DB_NAME()` (or the engine's equivalent) that it is the intended database, and report the engine and version.

## 2. Read the code: where does it save?

Find the persistence layer and list every write path before touching the database.

- Detect the stack: EF Core (`DbContext`, `DbSet`, `SaveChanges`, `Migrations/`), Dapper/ADO.NET (`INSERT`/`UPDATE`/`DELETE`, stored procedures, `.Execute`), or other ORM.
- Build a **write map**: for each repository/service method that creates, updates or deletes, note the entities/tables it touches, the fields it sets, and every side effect saved in the same operation (child lines, journal/audit rows, stock or balance adjustments, status flags, reverse operations on edit and delete).
- Read the models and mapping/configuration for: required fields, FKs and delete behavior, unique indexes, precision, enums, default values.
- Read the validators and business rules; they are the invariants the data must satisfy (totals, signs, allowed statuses, uniqueness).

Output the write map in a few lines before querying, so the user can correct a misunderstanding early.

## 3. Read the database

- List tables, columns, types, nullability, PKs, FKs, unique indexes, row counts.
- If the project uses migrations: compare the applied migrations table (`__EFMigrationsHistory` or equivalent) with the migration files. Report unapplied or missing ones.
- Compare model vs database: tables/columns in the code but not in the DB, and the reverse.

## 4. Verify, from the write map

Turn each write path and invariant into a query. Always give a count and up to 10 example keys; zero rows = pass.

- **Completeness:** a header row without its lines, lines without header, an operation that should have produced a related row (journal entry, movement, audit log, payment link) but did not, and the reverse (side-effect rows whose source is gone).
- **Referential integrity:** for every FK, child rows with no parent; also logical links without a FK (shared ids, `ReferenceCode`/code columns, string references).
- **Arithmetic:** stored totals vs the sum of their lines, paid + remaining = total, debit = credit per entry, stored balance/quantity vs the sum of the movements that should produce it.
- **State consistency:** status/flag columns vs the rows they imply (posted but no entry, closed but unpaid, active but deleted parent).
- **Constraint drift:** NULLs where the model says required, values outside the enum range, negative values where the rules forbid, duplicates where uniqueness is intended (codes, emails, names), gaps in sequences.
- **Update/delete correctness:** after the code's edit and delete paths, check no leftovers remain (old lines, old entries, old movements) and nothing was removed that should be kept.
- **Stale or junk data:** test rows, rows with default/placeholder values, rows never referenced by anything.
- **Encoding:** broken Arabic/Unicode text (`?` or mojibake) in text columns, and columns typed `varchar` where `nvarchar` is needed.

Prefer checks the code makes meaningful over a generic checklist; add engine-specific checks (collation, identity reseed gaps) only if relevant.

## 5. Report

```
Database audit - <date>
Target: <engine/version>, <database>, <server> (password masked)
Write paths found: <n> (list the main ones)
Schema: <n> tables, <pending migrations>, <model/DB differences>

CRITICAL (wrong money, stock or missing saves)
  <check>: <count> rows - keys 12, 15, ...
    code says: <what the code should have saved, file:line>
    database has: <what is there>
    proposed fix: <update | delete | re-run operation | code fix | new migration>

WARNING (inconsistent but harmless)
  ...

PASSED
  <check names>

Not checked: <anything that could not be verified and why>
```

Say plainly when something could not be verified. Do not claim "100% saved" unless every check ran and passed.

End with a numbered list of proposed fixes and ask which to apply. For data fixes, show the exact statement, run it inside a transaction the user approved, and re-run the related check afterwards. For code bugs found, point to the file and line; fix them only if asked.
