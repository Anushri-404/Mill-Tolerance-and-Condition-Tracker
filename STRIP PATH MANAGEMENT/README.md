# Strip Path Management (SPM)

A web application for logging and managing equipment observations along strip paths, with dropdown-driven data entry, stored procedure-backed persistence, and a clean repository/controller architecture on the backend.

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
- Stored procedures for observation logging and reporting

## Project Structure

```
strip-path-management/
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
└── frontend/
    └── src/
        ├── components/
        │   ├── common/          # DateInput, SelectField, TextInput, TextArea, FileInput
        │   ├── form/             # LogObservationForm
        │   ├── layout/           # PageHeader, PageTabs, ActionLinks
        │   └── report/           # ReportForm, ReportTable
        ├── pages/
        │   ├── LogObservationPage.jsx
        │   └── ReportPage.jsx
        ├── App.jsx
        └── main.jsx
```

## Prerequisites

- Node.js (LTS) and npm
- .NET SDK 8.0+
- Oracle XE (local via Docker, or native install)

## Setup

### 1. Database
Set up an Oracle XE instance (Docker recommended for local dev):
```bash
docker run -d -p 1521:1521 -e ORACLE_PASSWORD=<your_password> gvenzl/oracle-xe
```
Run the schema/stored procedure scripts against it (see `/db` if included, or request scripts separately — not included in this share for security).

### 2. Backend
```bash
cd backend
dotnet restore
```
Create your own `appsettings.Development.json` with a connection string:
```json
{
  "ConnectionStrings": {
    "OracleDb": "User Id=<user>;Password=<password>;Data Source=localhost:1521/XEPDB1;"
  }
}
```
Then run:
```bash
dotnet run
```
API will be available at the port shown in the console (e.g., `https://localhost:5001`).

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

## Features

- Log equipment observations via a structured, dropdown-driven form with field-level validation
- View and generate reports on logged observations
- Backend endpoints for CRUD operations on observation data, backed by Oracle stored procedures

## Status / Known Limitations

- Currently in active development
- Backend Oracle integration is being finalized (moving off mock data mode)

## License
This project is licensed under the MIT License.    



                                                                     -Anushri