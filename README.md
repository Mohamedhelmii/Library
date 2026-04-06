# Library API

Library is a lightweight RESTful Web API built with ASP.NET Core (.NET 8) and C# 12 for managing books and authors. It follows a layered architecture using Repository and UnitOfWork patterns, returns DTOs, and uses a `GeneralResponse` wrapper for consistent client responses.

## Key features
- CRUD operations for books and authors.
- Global search: `GET /api/Search/GlobalSearch?SearchTerm={term}` — searches titles, descriptions, categories and author fields and excludes soft-deleted records.
- Soft-delete support and relationship-aware DTOs.

## Tech stack
- .NET 8 (ASP.NET Core)
- C# 12
- SQL Server (Docker image provided)

## Prerequisites
- Visual Studio 2022 (or later) with .NET 8 SDK
- Docker & Docker Compose (optional for containerized setup)

## Quick start — Visual Studio
1. Clone the repository.
2. Open the solution in Visual Studio 2022.
3. Update the connection string in `appsettings.json`.
4. Restore NuGet packages, build and run (F5). Use Swagger UI or Postman to call endpoints.

## Quick start — Docker
Create a `.env` (or set environment variables) with at least:# Library