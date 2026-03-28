
# AGENTS.md — Angular Frontend Generation Spec

## Project Overview
This repository contains a .NET backend for a REST API that use .NET 10
The frontend is a separate Angular 21 application.

Codex should know:
- Backend is C# minimal API returning JSON
- Angular app is located in ./frontend (relative path)
- .NET web api is located in ./backend

## Backend API Standards
- Endpoints return JSON
- Pagination uses a PaginatedList<T> model
- Query parameters follow standard HTTP query format
- Naming conventions:
  - DTOs use PascalCase
  - Angular models should use camelCase
  
## Services general Requirements
frontend interfaces are : 
- located in ./frontend/src/app/models
- Use Angular `HttpClient` service
- Build query params string automatically from filters
- Use strict typing
- Return an Observable with pagination result



