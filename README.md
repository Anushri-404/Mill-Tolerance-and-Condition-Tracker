# Mill Equipment Digitization Projects — Tata Steel Internship

## About This Repository

This repository contains two full-stack web applications built during a summer internship at **Tata Steel**, aimed at digitizing manual, paper-based, and legacy shop-floor processes used in the rolling mill:

1. **Strip Path Management (SPM)** — a system for logging and reporting equipment condition observations along the strip path.
2. **Roll Chock Master (RCM)** — a modernized replacement for a legacy Oracle Forms application used to record and validate roll chock dimensional inspections.

Both were conceived, guided, and reviewed by mentors at Tata Steel, and were built as an introduction to industrial software development — from understanding shop-floor requirements to shipping a working internal tool.

## Internship Context

Coming into this internship, I had no prior hands-on experience with C# or the .NET ecosystem. My mentors set the direction for both projects: they identified the manual processes worth digitizing, decided on the technology stack (React for the frontend, ASP.NET Core Web API in C# for the backend, and Oracle as the database, since it is what the plant's existing systems already run on), and specified the naming conventions to follow for database tables, columns, and API fields — largely so the new applications would stay consistent with, and could sit alongside, Tata Steel's existing Oracle-based plant systems.

Over the course of the internship, I learned:
- **C# and .NET Core** from the ground up — controllers, dependency injection, async repositories, and building a Web API from scratch.
- How to design a backend against **existing legacy database schemas** rather than a clean-slate schema, which meant adapting to naming conventions and data shapes that were already in production use.
- How to translate a **legacy Oracle Forms screen** (in the case of Roll Chock Master) into an equivalent modern web experience, replicating its field-level behavior (like auto-populated defaults and conditional field validation).
- Practical front-to-back feature delivery: building React forms, wiring them to a C# API, and validating everything against real plant data and tolerance standards.

## The Two Projects

### 1. Strip Path Management (`/strip_path`)
A tool for logging and reporting on the condition of equipment along the strip path (the route steel strip travels through the mill's rolls, guides, and rollers). Operators/engineers can log observations against specific equipment — diameters, hardness, lining and bearing condition, defect severity — attach supporting photos, and generate filtered audit reports by section, equipment, and date range.

See [`strip_path/README.md`](./strip_path/README.md) for full details.

### 2. Roll Chock Master (`/roll_chock`)
A digitized replacement for a legacy Oracle Forms tool used to inspect **roll chocks** (the housings that hold and support work rolls in the mill). It records precise dimensional measurements (inside diameters, lining thicknesses at multiple points) for each chock and automatically validates them against tolerance standards, replacing manual paper-based tolerance checks.

See [`roll_chock/README.md`](./roll_chock/README.md) for full details.

## Industrial Relevance

Rolling mills depend on the condition of rolls, guides, and chocks staying within tight tolerances — worn or out-of-spec components directly affect strip quality (surface defects, dimensional inaccuracy) and can lead to unplanned equipment failure. Historically, these checks were recorded on paper or through disconnected legacy tools, making it hard to track trends over time or flag issues early.

Both applications address this in similar ways:
- **Replacing paper/manual logs with structured digital records**, so every observation or inspection is timestamped, attributable, and searchable.
- **Building a historical audit trail** per piece of equipment, making it possible to see how a roll, chock, or guide has degraded over time and plan replacements proactively instead of reactively.
- **Enforcing data quality at the point of entry** — dropdown-driven equipment hierarchies (SPM) and automatic tolerance validation (RCM) reduce transcription errors and catch out-of-tolerance components immediately, rather than after a failure.
- **Making inspection data reportable**, so maintenance planning and reliability teams can filter and export data by section, date range, or equipment instead of digging through paper logs or legacy screens.

In short, both tools aim to make preventive maintenance decisions faster and more data-driven, which reduces unplanned downtime and improves product quality — directly relevant to a rolling mill's throughput and yield.

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | React (Vite) |
| Backend | ASP.NET Core Web API (C#) |
| Database | Oracle |

## Repository Structure

```
├── strip_path/       # Strip Path Management (frontend + backend)
└── roll_chock/        # Roll Chock Master (frontend + backend)
```

Each project folder contains its own frontend, backend, and setup instructions — see the individual READMEs linked above.
