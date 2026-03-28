## Generation Requirements
### TypeScript Interfaces
Create interfaces for:
- `PersonnelListDto`
- `PaginatedList<T>`
- Request query filters (e.g., `GetPersonnelsWithFiltersQuery`)
- Put the generated interfaces in ./frontend/src/app/models/organisation/personnel.model.ts

### Angular Service
Generate an Angular `HttpClient` service in the frontend project:
- Angular 21
- method: 'getPersonnel'
- Build query params string automatically from filters
- Use strict typing
- Return an Observable with pagination result
- Put the generated service in ./frontend/src/app/models/organisation/personnel.service.ts

Service conventions:
- File name: personnel.service.ts
- Use Angular best practices for DI
- HTTP errors are catched by the errors handlers middleware


