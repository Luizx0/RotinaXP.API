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
4. Set database password outside source control via `Database:Password` or `ROTINAXP_DB_PASSWORD`.

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
### Auth
- POST /api/auth/register
- POST /api/auth/login

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

### Rewards
- GET /api/rewards
- GET /api/rewards/{id}
- GET /api/rewards/user/{userId}
- POST /api/rewards
- PUT /api/rewards/{id}
- DELETE /api/rewards/{id}
- POST /api/rewards/{id}/redeem

### DailyProgress
- GET /api/dailyprogress
- GET /api/dailyprogress/{id}
- GET /api/dailyprogress/user/{userId}

## Notes
- Naming is standardized in English across models, DTOs, services, controllers, and migrations.
- Migrations were regenerated to match the current standardized model.
- Request validation now uses DataAnnotations with automatic ASP.NET Core model validation.
- User email now has a unique database index (`IX_Users_Email`) and duplicate attempts return HTTP 409.
- Auth and Users creation/update endpoints may return HTTP 400 (validation) or HTTP 409 (email conflict).
- Completing a task via `PUT /api/tasks/{id}` awards 10 points and increments the user's daily progress for the current UTC day.
- Redeeming a reward via `POST /api/rewards/{id}/redeem` deducts points from the reward owner and removes the reward.