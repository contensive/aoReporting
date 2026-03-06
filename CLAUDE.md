# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**aoReporting** is a reporting addon library for the **Contensive CMS platform**. It compiles to `Reporting.dll` and provides various site analytics reports (page views, visits, users online, email tracking, etc.).

It is built on the Contensive Architecture documented here:
- [Contensive Architecture](https://raw.githubusercontent.com/contensive/Contensive5/refs/heads/master/patterns/contensive-architecture.md)

- **Language**: C#
- **Framework**: .NET Framework 4.8 (net48)
- **Key dependencies**: `Contensive.CPBaseClass`, `Contensive.DbModels`, `Newtonsoft.Json`

## Build

```bash
# From repo root
dotnet build server/aoReportingCSharp/aoReporting.csproj
# Or open server/aoReporting.sln in Visual Studio
```

## Deployment Build

```bash
# Run the deployment build script
scripts/build.cmd
```

There is no test project in this solution.

## Architecture

### Addon Pattern
All reports extend `AddonBaseClass` and implement `Execute(CPBaseClass cp)`. They return HTML strings rendered as report pages. Report addons live in `server/aoReportingCSharp/Addons/`. Legacy VB.NET-era code being phased out is in `AddonsLegacy/`.

### Models
- `ApplicationModel` — base model with error handling, JSON serialization, and `IDisposable` support
- `VisitSummaryModel` — extends `Contensive.Models.Db.VisitSummaryModel`, handles visit data aggregation and housekeeping
- `ChartViewModel` — Google Charts visualization data
- `ViewingSummaryModel` — page viewing aggregation

### Controllers
- `GenericController` — static helper methods (date encoding/formatting)
- `HousekeepController` — housekeeping coordination

### Housekeeping
`HousekeepTask` addon runs hourly and daily tasks for data aggregation (visit summaries, viewing summaries).

## Code Conventions

- **Constants**: request names prefixed with `rn` (e.g., `rnButton`, `rnAccountId`); addon GUIDs stored as `addonGuid*` constants
- **Error handling**: wrap addon execution in try-catch, report via `cp.Site.ErrorReport(ex)`
- **Data access**: use `CPCSBaseClass` with `using` statements for proper disposal
- **Date handling**: use `cp.Db.EncodeSQLDate()` for SQL date encoding; OADate format (double) for storage
- **HTML generation**: `StringBuilder` for building report HTML output
- **String formatting**: prefer string interpolation over concatenation
