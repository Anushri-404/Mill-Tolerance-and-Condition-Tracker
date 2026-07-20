# Roll Chock Master

A full-stack web application for managing and querying **Roll Chock** information. The application provides an intuitive React frontend with an ASP.NET Core Web API backend connected to an Oracle database for retrieving and managing chock-related records.

---

## Features

- Search Roll Chock records by **Chock ID** and **Chock Type**
- Dynamic lookup data for dropdowns
- RESTful ASP.NET Core Web API
- React + Vite frontend
- Oracle Database integration
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
- Other lookup data

---

### Search Chock

```
GET /api/chock/query
```

Example:

```
GET /api/chock/query?chockId=1001&chockType=TYPE-A
```

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
Anushri