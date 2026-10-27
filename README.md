# TaskBoard

Full-stack task management app: ASP.NET Core API + Angular SPA.  
Users register/login with JWT, then create and manage their own tasks on a status board.

## Tech stack

- **Backend:** .NET 10, ASP.NET Core Minimal APIs, EF Core, SQL Server
- **Auth:** JWT Bearer, BCrypt password hashing
- **Frontend:** Angular 21, standalone components
- **API docs:** Swagger / Swashbuckle

## Repository layout

```text
TaskBoard/
├── backend/
│   └── TaskBoard.Api/
├── frontend/
├── TaskBoard.slnx
└── README.md
```

## Prerequisites

- .NET 10 SDK
- SQL Server (LocalDB or full instance) with a database created via EF migrations
- Node.js 20+ and npm
- User Secrets for the API (connection string + JWT key) — not committed to Git

## Configure the API (User Secrets)

From `backend/TaskBoard.Api`:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=TaskBoard;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
dotnet user-secrets set "Jwt:Key" "REPLACE_WITH_A_LONG_RANDOM_SECRET"
dotnet user-secrets set "Jwt:Issuer" "TaskBoard"
dotnet user-secrets set "Jwt:Audience" "TaskBoard"
```

Adjust `Server=` if your SQL instance name differs.

Apply migrations if the database is not up to date:

```powershell
cd backend/TaskBoard.Api
dotnet ef database update
```

## Run the backend

```powershell
cd backend/TaskBoard.Api
dotnet run --launch-profile http
```

- API: `http://localhost:5063`
- Swagger: `http://localhost:5063/swagger`

## Run the frontend

```powershell
cd frontend
npm install
npm start
```

- App: `http://localhost:4200`
- Talks to the API at `http://localhost:5063` (see `frontend/src/environments/environment.ts`)

Keep both processes running while developing.

## Git workflow

- `dev` — day-to-day work; commit and push here
- `main` — stable; update via Pull Request from `dev`
- After a PR is merged into `main`, sync locally:

```powershell
git checkout dev
git fetch origin
git merge origin/main
git push origin dev
```

## Features

- Register / login (JWT stored in browser `localStorage`)
- Auth and guest route guards
- Task CRUD with ownership (users only see their tasks)
- Board columns: To Do, In Progress, Done
- CORS allowed for `http://localhost:4200`
- On API `401`, frontend clears the token and redirects to login
