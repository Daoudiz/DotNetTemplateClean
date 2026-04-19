# 🚀 DotNetTemplateClean

Reusable full-stack template built with **.NET 10** and **Angular 21 (CoreUI)**, following **Clean Architecture** principles.  
It is designed to accelerate new project setup while keeping backend and frontend concerns cleanly separated.

## ✨ Features

- **Backend (.NET 10)**
  - Clean Architecture layers (Domain, Application, Infrastructure, WebAPI)
  - REST API with JSON responses
  - MediatR-based request handling
- **Frontend (Angular 21 + CoreUI)**
  - Angular application scaffolded around CoreUI components
  - Ready-to-use UI base for admin/business apps
- **Template support (`dotnet new`)**
  - Install locally and generate new solutions quickly
- **Database selection at template creation**
  - SQL Server (`sqlserver`)
  - PostgreSQL (`postgresql`)

## 📦 Installation

### 🧩 1) Install template locally (from this repository)

```bash
dotnet new install .
```

### 🌐 2) Install from GitHub (specific tag/version)

```bash
dotnet new install https://github.com/Daoudiz/DotNetTemplateClean
```

Example:

```bash
dotnet new install https://github.com/Daoudiz/DotNetTemplateClean
```

## ▶️ Usage

Template short name: **`ca-angular`**

```bash
dotnet new ca-angular -n MyApp --Database sqlserver
```

```bash
dotnet new ca-angular -n MyApp --Database postgresql
```

## 🗄️ Database Configuration

The backend expects `ConnectionStrings:DBConnection`.  
Use .NET User Secrets in development:

1. Go to the Web API project folder:

```bash
cd backend/src/DotNetTemplateClean.WebAPI
```

2. Set the connection string.

### 🧱 SQL Server example

```bash
dotnet user-secrets set "ConnectionStrings:DBConnection" "Server=localhost,1433;Database=MyAppDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
```

### 🐘 PostgreSQL example

```bash
dotnet user-secrets set "ConnectionStrings:DBConnection" "Host=localhost;Port=5432;Database=MyAppDb;Username=postgres;Password=YourStrong!Passw0rd"
```

## 🛠️ Run the Project

### ⚙️ Backend

```bash
cd backend/src/DotNetTemplateClean.WebAPI
dotnet run
```

### 🎨 Frontend

```bash
cd frontend
npm install
ng serve
```

## 🗂️ Project Structure

```text
backend/
  src/
  tests/
frontend/
tests/ (backend tests are in backend/tests)
```

Current backend test projects:

- `backend/tests/Domain.UnitTests`
- `backend/tests/Application.UnitTests`
- `backend/tests/Application.FunctionalTests`

## 📝 Notes

- Backend and frontend are intentionally separated to keep deployment and development workflows flexible.
- The Angular frontend is copied as-is by the template, while backend naming is parameterized by template values.
- Default CORS development origin includes `http://localhost:4200`.

## 🧰 Technologies Used

### ⚙️ Backend

- .NET 10 (ASP.NET Core Web API)
- C#
- Entity Framework Core
- MediatR
- Serilog
- Swagger / OpenAPI

### 🎨 Frontend

- Angular 21
- CoreUI for Angular
- TypeScript
- RxJS
- SCSS
- PrimeNG

### 🧪 Testing

- NUnit
- ASP.NET Core integration/functional testing stack

### 🚀 DevOps & Tooling

- .NET Template Engine (`dotnet new`)
- npm / Angular CLI
- GitHub (template/package distribution)

## 🏗️ Project Architecture

This template follows **Clean Architecture** to enforce separation of concerns, testability, and long-term maintainability.

### 🧠 Domain

- Contains core business entities, value objects, enums, and domain rules.
- Has no dependency on infrastructure or frameworks.
- Represents the business truth of the system.

### 📐 Application

- Contains use cases and application logic.
- Implements patterns like **CQRS** (commands/queries) with **MediatR**.
- Defines contracts/interfaces consumed by infrastructure.
- Handles validation, orchestration, and DTO mapping.

### 🔌 Infrastructure

- Implements technical concerns defined by application contracts.
- Hosts persistence (EF Core), external services, authentication plumbing, and logging integration.
- Wires database providers (SQL Server/PostgreSQL).

### 🌍 Web/API

- Entry point of the backend (ASP.NET Core Web API).
- Configures middleware, dependency injection, routing, CORS, security, and API endpoints.
- Exposes JSON-based REST endpoints.

### 🖥️ Frontend (Angular)

- Separate Angular 21 application based on CoreUI.
- Consumes backend APIs and provides UI, navigation, and stateful client interactions.
- Keeps presentation concerns isolated from backend business logic.

### 🧩 Key Architectural Patterns

- **Clean Architecture** for clear boundaries
- **CQRS** for separating reads and writes
- **Dependency Injection (DI)** for loose coupling
- **Repository/Service abstractions** through interfaces in application layer
