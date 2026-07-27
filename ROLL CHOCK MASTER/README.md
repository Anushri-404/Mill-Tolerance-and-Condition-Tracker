# Roll Chock Master
A full-stack web application for recording and validating dimensional inspections of **roll chocks** — the housings that hold and support the work rolls in a rolling mill and let them rotate under load. It's a modernized, browser-based replacement for a legacy Oracle Forms screen that engineers previously used to check chock measurements against paper tolerance charts by hand — built with a React frontend, an ASP.NET Core Web API backend, and Oracle for storage.

---

## Features
- **Search Roll Chock records** by **Chock ID** and **Chock Type**, pulling back the chock's saved measurements plus its tolerance standard
- **Dynamic lookup data for dropdowns** — chock types and chock makers sourced from the plant's existing reference data
- **Type-aware form configuration** — selecting a Chock Type automatically applies its default status code and tolerance standard, and disables measurement fields that don't apply to that type, mirroring the field-level triggers of the legacy Oracle Forms screen this replaces
- **Automatic tolerance validation** — inside diameter measurements (A1/A2/B1/B2/C1/C2) and lining measurements at top/bottom and inner/outer positions are checked against per-type tolerance standards instead of a manual paper chart
- **Save & update** — persists new or updated chock inspection records straight to the plant's Oracle database
- RESTful ASP.NET Core Web API
- React + Vite frontend
- Repository Pattern architecture
- Responsive and clean UI

---

## Tech Stack
### Frontend
- React 18
- Vite
- JavaScript (ES6+)
- CSS

### Backend
- ASP.NET Core Web API
- C#
- Repository Pattern

### Database
- Oracle Database
- Oracle Managed Data Access

---

## Project Structure
```
Roll-Chock-Master
│
├── backend
│   ├── Controllers
│   ├── Models
│   ├── Repositories
│   ├── Program.cs
│   ├── appsettings.json
│   └── RollChockBackend.csproj
│
├── frontend
│   ├── src
│   ├── package.json
│   ├── vite.config.js
│   └── index.html
│
├── sql
│   ├── 01_create_schema_and_tables.sql
│   ├── 02_tables_as_chockuser.sql
│   └── 03_seed_data.sql
│
├── .gitignore
├── .gitattributes
└── README.md
```

---

## Prerequisites
Before running the project, install:
- .NET 8 SDK (or compatible version)
- Node.js (v18 or later)
- Oracle Database
- Visual Studio / VS Code

---

## Database Setup
Navigate to the **sql** folder and execute the scripts in the following order:
```
01_create_schema_and_tables.sql
02_tables_as_chockuser.sql
03_seed_data.sql
```

Update the Oracle connection string inside:
```
backend/appsettings.json
```

Example:
```json
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=your_user;Password=your_password;Data Source=localhost:1521/XEPDB1;"
  }
}
```

---

## Backend Setup
```bash
cd backend
dotnet restore
dotnet run
```
The API runs on:
```
http://localhost:5210
```

---

## Frontend Setup
```bash
cd frontend
npm install
npm run dev
```
The frontend runs on:
```
http://localhost:5174
```

---

## API Endpoints

### Get Lookup Data
```
GET /api/chock/lookups
```
Returns dropdown values such as:
- Chock Type
- Chock Maker

### Search Chock
```
GET /api/chock/query?chockId=&chockType=
```
Example:
```
GET /api/chock/query?chockId=1001&chockType=TYPE-A
```
Returns the chock's saved measurements plus the tolerance standard for its type.

### Get Type Configuration
```
GET /api/chock/type-config?chockType=&chockId=
```
Returns the default status code, applicable tolerance limits, and the list of measurement fields to disable for the given chock type.

### Save Chock Record
```
POST /api/chock/save
```
Creates or updates a chock's inspection record with the submitted measurements.

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
Oracle Database
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
- JWT Security
- Pagination
- Search Filters
- Export to Excel/PDF
- Unit Testing
- Docker Support

---

## Author
**Anu Shri**
Computer Science Engineering Student
GitHub: https://github.com/Anushri-404

---

## License

This project is licensed under the MIT License.

                                                             -Anushri
