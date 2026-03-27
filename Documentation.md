# RotinaXP — Backend API

## Como Rodar o Projeto

### Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- PostgreSQL rodando localmente (porta padrão `5432`)
- Banco de dados `RotinaXP` criado no PostgreSQL com usuário `postgres`

### 1. Configure a senha do banco

A senha **nunca** é commitada. Defina por um dos métodos abaixo:

**Opção A — .NET User Secrets (recomendado em desenvolvimento):**
```bash
dotnet user-secrets set "Database:Password" "sua_senha_aqui"
```

**Opção B — Variável de ambiente:**
```bash
# Windows PowerShell
$env:ROTINAXP_DB_PASSWORD = "sua_senha_aqui"

# Linux / macOS
export ROTINAXP_DB_PASSWORD=sua_senha_aqui
```

### 2. Aplique as migrations

```bash
dotnet ef database update
```

### 3. Execute a API

```bash
dotnet run
```

A API sobe em:
- HTTP: `http://localhost:5252`
- HTTPS: `https://localhost:7024`

### 4. Acesse o Swagger

Disponível apenas no ambiente `Development`:

```
http://localhost:5252/swagger
```

### 5. Verifique o health check

```
GET http://localhost:5252/health
```

Responde `Healthy` quando a API e o banco de dados estão funcionando.

### 6. Execute os testes

```bash
dotnet test
```

---

## Visão Geral

REST API de produtividade gamificada. Usuários criam tarefas e recompensas. Concluir tarefas concede pontos e registra progresso diário. Resgatar recompensas desconta pontos.

## Stack

| Tecnologia | Versão |
|---|---|
| .NET / ASP.NET Core | 9 |
| Entity Framework Core | 9.0.1 |
| PostgreSQL (Npgsql) | 9.0.4 |
| BCrypt.Net-Next | 4.0.3 |
| Swagger (Swashbuckle) | 7.0.0 |
| xUnit (testes) | 2.9.2 |

## Estrutura do Projeto

```
Controllers/       — Endpoints HTTP (AuthController, UsersController, TasksController, RewardsController, DailyProgressController)
Data/              — ApplicationDbContext (EF Core)
DTOs/              — Contratos de entrada e saída (UserDTO.cs, GamificationDTOs.cs)
Middleware/        — ExceptionHandlingMiddleware (erros 500 padronizados com ProblemDetails)
Migrations/        — Histórico de migrations EF Core
Models/            — Entidades de domínio (User, TaskItem, Reward, DailyProgress)
Services/          — Regras de negócio (UserService, TaskService, RewardService, DailyProgressService)
tests/             — Testes de integração e unitários (xUnit)
```

## Entidades de Domínio

### User
| Campo | Tipo |
|---|---|
| Id | int |
| Name | string |
| Email | string (único) |
| PasswordHash | string (BCrypt, nunca exposto em respostas) |
| Points | int |

### TaskItem
| Campo | Tipo |
|---|---|
| Id | int |
| Title | string |
| IsCompleted | bool |
| UserId | int (FK) |

### Reward
| Campo | Tipo |
|---|---|
| Id | int |
| Title | string |
| PointsCost | int |
| UserId | int (FK) |

### DailyProgress
| Campo | Tipo |
|---|---|
| Id | int |
| Date | DateTime (UTC) |
| CompletedTasksCount | int |
| UserId | int (FK) |

## DTOs de Resposta

Todos os endpoints retornam DTOs — nunca entidades de domínio diretamente.

- `UserDTO` — Id, Name, Email, Points
- `TaskDTO` — Id, Title, IsCompleted, UserId
- `RewardDTO` — Id, Title, PointsCost, UserId
- `DailyProgressDTO` — Id, Date, CompletedTasksCount, UserId

## Endpoints

### Auth

| Método | Rota | Corpo | Respostas |
|---|---|---|---|
| POST | `/api/auth/register` | `{ name, email, password }` | 201 LoginResponse / 400 / 409 |
| POST | `/api/auth/login` | `{ email, password }` | 200 LoginResponse / 400 / 404 |

### Users

| Método | Rota | Corpo | Respostas |
|---|---|---|---|
| GET | `/api/users` | — | 200 UserDTO[] |
| GET | `/api/users/{id}` | — | 200 UserDTO / 404 |
| POST | `/api/users` | `{ name, email, password }` | 201 UserDTO / 400 / 409 |
| PUT | `/api/users/{id}` | `{ name?, email? }` | 204 / 400 / 404 / 409 |
| DELETE | `/api/users/{id}` | — | 204 / 404 |

### Tasks

| Método | Rota | Corpo | Respostas |
|---|---|---|---|
| GET | `/api/tasks` | — | 200 TaskDTO[] |
| GET | `/api/tasks/{id}` | — | 200 TaskDTO / 404 |
| GET | `/api/tasks/user/{userId}` | — | 200 TaskDTO[] / 404 |
| POST | `/api/tasks` | `{ title, isCompleted, userId }` | 201 TaskDTO / 400 |
| PUT | `/api/tasks/{id}` | `{ title?, isCompleted? }` | 200 `{ message, pointsAwarded }` / 400 / 404 / 409 |
| DELETE | `/api/tasks/{id}` | — | 204 / 404 |

> Concluir uma tarefa (`isCompleted: true`) concede **10 pontos** ao usuário e registra ou incrementa o `DailyProgress` do dia atual (UTC).  
> Tentar reabrir uma tarefa já concluída retorna **409 Conflict**.

### Rewards

| Método | Rota | Corpo | Respostas |
|---|---|---|---|
| GET | `/api/rewards` | — | 200 RewardDTO[] |
| GET | `/api/rewards/{id}` | — | 200 RewardDTO / 404 |
| GET | `/api/rewards/user/{userId}` | — | 200 RewardDTO[] / 404 |
| POST | `/api/rewards` | `{ title, pointsCost, userId }` | 201 RewardDTO / 400 |
| PUT | `/api/rewards/{id}` | `{ title?, pointsCost? }` | 204 / 400 / 404 |
| DELETE | `/api/rewards/{id}` | — | 204 / 404 |
| POST | `/api/rewards/{id}/redeem` | — | 200 `{ message, pointsRemaining }` / 400 / 404 |

> Resgatar uma recompensa desconta `pointsCost` do usuário e remove a recompensa.  
> Se o usuário não tiver pontos suficientes, retorna **400 Bad Request**.

### DailyProgress

| Método | Rota | Respostas |
|---|---|---|
| GET | `/api/dailyprogress` | 200 DailyProgressDTO[] |
| GET | `/api/dailyprogress/{id}` | 200 DailyProgressDTO / 404 |
| GET | `/api/dailyprogress/user/{userId}` | 200 DailyProgressDTO[] / 404 |

### Health Check

| Método | Rota | Descrição |
|---|---|---|
| GET | `/health` | Retorna `Healthy` com status do banco de dados |

## Validação e Erros

- DataAnnotations em todos os DTOs de entrada com validação automática pelo ASP.NET Core.
- Respostas de erro seguem o padrão **ProblemDetails** (RFC 7807):
  - **400** — falha de validação (inclui detalhes por campo)
  - **404** — recurso não encontrado
  - **409** — conflito (email duplicado ou tarefa concluída não pode ser reaberta)
  - **500** — erro interno (inclui `traceId` para rastreamento)
- Email tem índice único no banco (`IX_Users_Email`); tentativas duplicadas retornam 409.

## Segurança

- Senhas armazenadas com hash BCrypt (nunca em texto plano).
- Senha do banco externalizada via User Secrets ou variável de ambiente.
- Respostas nunca expõem `passwordHash` nem propriedades de navegação do EF Core.