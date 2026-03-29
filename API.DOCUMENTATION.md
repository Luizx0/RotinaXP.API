# RotinaXP - Backend API

## Como Rodar o Projeto

### Pre-requisitos

- .NET 9 SDK
- PostgreSQL local (porta padrao 5432)
- Banco RotinaXP criado no PostgreSQL

### 1. Configure a senha do banco

Opcao A - User Secrets (recomendado):

dotnet user-secrets set "Database:Password" "sua_senha_aqui"

Opcao B - Variavel de ambiente:

Windows PowerShell:
$env:ROTINAXP_DB_PASSWORD = "sua_senha_aqui"

Linux/macOS:
export ROTINAXP_DB_PASSWORD=sua_senha_aqui

### 2. Aplique as migrations

Use o projeto de infraestrutura e startup da WebApi:

dotnet ef database update --project src/Infrastructure/RotinaXP.API.Infrastructure/RotinaXP.API.Infrastructure.csproj --startup-project src/WebApi/RotinaXP.API.csproj

### 3. Execute a API

dotnet run --project src/WebApi/RotinaXP.API.csproj

A API sobe em:
- HTTP: http://localhost:5252
- HTTPS: https://localhost:7024

### 4. Swagger

Disponivel em Development:
http://localhost:5252/swagger

### 5. Health checks

- /health/live
- /health/ready
- /health

### 6. Testes

dotnet test RotinaXP.API.sln

---

## Visao Geral

API REST de produtividade gamificada. Usuarios criam tarefas e recompensas. Concluir tarefas concede pontos e atualiza progresso diario. Resgatar recompensas debita pontos.

## Stack

| Tecnologia | Versao |
|---|---|
| .NET / ASP.NET Core | 9 |
| Entity Framework Core | 9.0.1 |
| PostgreSQL (Npgsql) | 9.0.4 |
| JWT Bearer | 9.0.1 |
| OpenTelemetry | 1.10+ |
| Swagger | 7.0.0 |
| xUnit | 2.9.2 |

## Estrutura Atual da Solucao

src/
- Core/
  - RotinaXP.API.Domain/
    - Entities/
  - RotinaXP.API.Application/
    - DTOs/
    - Features/
    - Interfaces/
- Infrastructure/
  - RotinaXP.API.Infrastructure/
    - Authorization/
    - Persistence/
      - Data/
      - Migrations/
    - Services/
- Shared/
  - RotinaXP.API.Shared/
    - Helpers/
- WebApi/
  - Controllers/
  - Middleware/
  - Program.cs
  - appsettings.json

tests/
- Integration/
- Unit/
- RotinaXP.API.Tests.csproj

## Arquitetura (resumo)

- WebApi: camada de entrada HTTP (controllers, middlewares, pipeline).
- Application: DTOs, use cases e contratos de aplicacao.
- Domain: entidades e regras centrais de dominio.
- Infrastructure: EF Core, servicos concretos, migrations e autorizacao.
- Shared: helpers reutilizaveis entre camadas.

## Seguranca e Operacao

- JWT com autenticacao bearer.
- Policy ResourceOwner para protecao por dono de recurso.
- Middleware de correlation id.
- Exception handling com ProblemDetails.
- Rate limiting global.
- Health checks live e ready.
- Telemetria com OpenTelemetry (traces e metrics).

## Endpoints Principais

- Auth:
  - POST /api/auth/register
  - POST /api/auth/login
- Users:
  - GET /api/users
  - GET /api/users/{id}
  - POST /api/users
  - PUT /api/users/{id}
  - DELETE /api/users/{id}
- Tasks:
  - GET /api/tasks
  - GET /api/tasks/{id}
  - GET /api/tasks/user/{userId}
  - POST /api/tasks
  - PUT /api/tasks/{id}
  - DELETE /api/tasks/{id}
- Rewards:
  - GET /api/rewards
  - GET /api/rewards/{id}
  - GET /api/rewards/user/{userId}
  - POST /api/rewards
  - PUT /api/rewards/{id}
  - DELETE /api/rewards/{id}
  - POST /api/rewards/{id}/redeem
- DailyProgress:
  - GET /api/dailyprogress
  - GET /api/dailyprogress/{id}
  - GET /api/dailyprogress/user/{userId}

## Notas de Escalabilidade ja implementadas

- Paginacao com metadados (incluindo hasNext e hasPrevious).
- Leitura com AsNoTracking e projecao para DTO.
- Controle de concorrencia otimista com RowVersion.
- Operacoes criticas com update atomico e transacao.
- Indices para carga alta e restricao de unicidade no progresso diario.
