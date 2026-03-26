# RotinaXP - Backend API

## Overview
REST API built with ASP.NET Core to manage users, tasks, daily progress, and rewards in a gamified productivity system.

## Tech Stack
- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Npgsql
- Swagger / OpenAPI

## Project Structure
- /Controllers
- /Models
- /DTOs
- /Services
- /Data
- /Migrations

## Initial Setup
1. Clone the repository.
2. Configure the database connection in appsettings.json.
3. Apply migrations.

## Domain Entities
### User
- Id
- Name
- Email
- PasswordHash
- Points

### TaskItem
- Id
- Title
- IsCompleted
- UserId

### DailyProgress
- Id
- Date
- CompletedTasksCount
- UserId

### Reward
- Id
- Title
- PointsCost
- UserId

## Available Endpoints
### Users
- GET /api/users
- GET /api/users/{id}
- POST /api/users
- PUT /api/users/{id}
- DELETE /api/users/{id}

### Tasks
- GET /api/tasks
- GET /api/tasks/{id}
- GET /api/tasks/user/{userId}
- POST /api/tasks
- PUT /api/tasks/{id}
- DELETE /api/tasks/{id}

## Notes
- Naming is standardized in English across models, DTOs, services, controllers, and migrations.
- Migrations were regenerated to match the current standardized model.