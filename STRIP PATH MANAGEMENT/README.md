# Strip Path Management (SPM)
A full-stack web application for logging and reporting on the condition of equipment along the **strip path** — the sequence of rolls, guides, and rollers that steel strip travels through in the mill. It replaces manual, paper-based equipment observation logs with a structured digital form and a searchable reporting view, so engineers can record wear and defects against a specific piece of equipment, attach supporting photos, and pull filtered history for maintenance review — all backed by a React frontend, an ASP.NET Core Web API, and Oracle.

---

## Features
- **Cascading, dropdown-driven equipment selection** — pick a section, then Equipment Level 1, then Equipment Level 2, sourced from the `SPGET_SPM_MASTERLIST` stored procedure, so every observation is logged against a valid, existing piece of equipment
- **Auto-populated baseline specs** — selecting an Equipment Level 2 item pulls its known diameter, hardness, roll coat, maintenance philosophy, replacement frequency, and touch point, so the observer only enters what's changed
- **Structured observation logging** — captures defect type, affected portion, defect details, new measurements (diameter, hardness, lining/bearing condition, bakelite guide plate condition), severity status, and key maintenance dates (audit date, last roll change, last bearing greasing)
- **File attachments** — upload a photo or document as evidence alongside an observation, retrievable later from the report view
- **Filterable reporting** — search and filter logged observations by date range, section, and equipment level, with direct links to any attached files
- **Graceful offline/dev mode** — automatically falls back to an in-memory mock dataset if Oracle isn't configured or reachable, so the frontend stays fully usable during local development without a live DB
- Clean repository/controller architecture on the backend (`ISpmRepository` / `SpmRepository` / `SpmController`)
- No update/delete endpoints currently — the API supports logging new observations, reporting on them, and retrieving attachments

---

## Tech Stack
**Frontend**
- React (Vite)
- Component-based structure with reusable form fields (`DateInput`, `SelectField`, `TextInput`, `TextArea`, `FileInput`)

**Backend**
- ASP.NET Core Web API
- Repository pattern (`ISpmRepository` / `SpmRepository`) for data access
- Controller layer (`SpmController`) exposing REST endpoints

**Database**
- Oracle (Oracle XE for local development)
- Parameterized SQL for observation logging and reporting, plus a stored procedure (`SPGET_SPM_MASTERLIST`) for cascading dropdown lookups (sections, equipment levels)
- Falls back to an in-memory mock dataset automatically if no Oracle connection string is configured or the database is unreachable, so the frontend stays usable during local development without a live DB

## Project Structure
```
strip_path/
├── backend/
│   ├── Controllers/
│   │   └── SpmController.cs
│   ├── Models/
│   │   └── SpmModels.cs
│   ├── Repositories/
│   │   ├── ISpmRepository.cs
│   │   └── SpmRepository.cs
│   ├── Program.cs
│   ├── backend.csproj
│   └── appsettings.json        # not included in repo — see Configuration
│
├── frontend/
│   └── src/
│       ├── components/
│       │   ├── common/          # DateInput, SelectField, TextInput, TextArea, FileInput
│       │   ├── form/             # LogObservationForm
│       │   ├── layout/           # PageHeader, PageTabs, ActionLinks
│       │   └── report/           # ReportForm, ReportTable
│       ├── data/                 # formOptions.js — static form option lists
│       ├── pages/
│       │   ├── LogObservationPage.jsx
│       │   └── ReportPage.jsx
│       ├── App.jsx
│       └── main.jsx
│
└── Database/
    └── spm_schema.sql
```

## Prerequisites
- Node.js (LTS) and npm
- .NET SDK 8.0+
- Oracle XE (local via Docker, or native install) — optional for local dev, since the backend falls back to mock data without it

## Setup

### 1. Database
Set up an Oracle XE instance (Docker recommended for local dev):
```bash
docker run -d -p 1521:1521 -e ORACLE_PASSWORD=<your_password> gvenzl/oracle-xe
```
Run `Database/spm_schema.sql` against it to create the required tables and the `SPGET_SPM_MASTERLIST` stored procedure.

### 2. Backend
```bash
cd backend
dotnet restore
```
Create your own `appsettings.Development.json` with a connection string:
```json
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=<user>;Password=<password>;Data Source=localhost:1521/XEPDB1;"
  }
}
```
Then run:
```bash
dotnet run
```
API will be available at the port shown in the console (e.g., `https://localhost:5001`). If the connection string above is left empty or the database is unreachable, the API automatically serves data from a built-in mock dataset instead.

### 3. Frontend
```bash
cd frontend
npm install
npm run dev
```
App will be available at `http://localhost:5173` by default.

## Configuration Notes
- `appsettings.json` / `appsettings.Development.json` are **excluded** from this share — they contain database connection strings. Create your own locally using the template above.
- Frontend expects the backend API base URL to be configured (check for a `.env` or config constant pointing to the API — update to match your local backend port).

## API Endpoints
| Method | Route | Purpose |
|---|---|---|
| GET | `/api/spm/sections` | List available plant sections |
| GET | `/api/spm/equip-l1?section=` | List Equipment Level 1 for a section |
| GET | `/api/spm/equip-l2?section=&equipL1=` | List Equipment Level 2 for a section/L1 |
| GET | `/api/spm/grey-parts?equipL2Id=` | Fetch baseline specs for an Equipment Level 2 item |
| GET | `/api/spm/observation-types` | List observation/defect types |
| GET | `/api/spm/affected-portions` | List affected-portion options |
| POST | `/api/spm/save-observation` | Save a new observation (with optional file attachment) |
| GET | `/api/spm/report` | Fetch filtered observation report |
| GET | `/api/spm/attachment/{fileName}` | Download an attached file |

## Status / Known Limitations
- Currently in active development
- Runs against real Oracle data when a connection string is configured and reachable; otherwise falls back to mock data automatically

---

## Architecture
```
React Frontend
        │
        ▼
ASP.NET Core Web API
        │
Repository Pattern
        │
Oracle Database (or in-memory mock, if unreachable)
```

---

## Repository Pattern
The backend follows the Repository Pattern for better separation of concerns.
```
Controller
      │
      ▼
Repository Interface
      │
      ▼
Repository Implementation
      │
      ▼
Oracle Database
```

---

## Future Enhancements
- Authentication & Authorization
- Update & delete endpoints for logged observations
- Pagination
- Advanced search filters
- Export report to Excel/PDF
- Unit Testing
- Docker Support

---

## Author
**Anu Shri**
Computer Science Engineering Student
GitHub: https://github.com/Anushri-404

---

## License
This project is intended for internal use as part of a Tata Steel internship and is not currently licensed for external distribution.


                                                             -Anushri
