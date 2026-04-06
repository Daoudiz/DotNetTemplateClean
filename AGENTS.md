
# AGENTS.md — Angular Frontend Generation Spec

## Project Overview
This repository contains a .NET backend for a REST API that uses .NET 10.
The frontend is a separate Angular 21 application.

Codex should know:
- Backend is a C# minimal API returning JSON with MediatR.
- Angular app is located in ./frontend (relative path).
- .NET Web API is located in ./backend.

## Backend API Standards
- Endpoints return JSON.
- Pagination uses a PaginatedList<T> model.
- Query parameters follow standard HTTP query format and should be URL-encoded.
- Naming conventions:
  - DTOs use PascalCase.
  - Angular models should use camelCase.

## Angular Styling Rules (Strict)

- Inline styles in HTML templates are strictly forbidden.
- All styles must be moved to the component SCSS file.
- Always use `styleUrls` in Angular components.
- Any generated code that includes inline styles must be refactored into SCSS.

## Interfaces Generation Requirements
- Generate TypeScript interfaces for all backend DTOs.
- Preserve property names and types.
- Use strict typing and optional properties if applicable.

## Services General Requirements
- Use Angular `HttpClient`.
- Build query params strings automatically from filters.
- Return an Observable with pagination results.
- Follow strict typing.
- Use URL format: `${this.apiUrl}/entityName/endpointName` (replace entityName/endpointName with the correct backend endpoint).

## Service Conventions
##Catch HTTP errors and forward them appropriately.
- Follow Angular best practices for service methods.




