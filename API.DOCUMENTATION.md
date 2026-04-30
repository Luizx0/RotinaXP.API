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

| Tecnologia            | Versao |
| --------------------- | ------ |
| .NET / ASP.NET Core   | 9      |
| Entity Framework Core | 9.0.1  |
| PostgreSQL (Npgsql)   | 9.0.4  |
| JWT Bearer            | 9.0.1  |
| OpenTelemetry         | 1.10+  |
| Swagger               | 7.0.0  |
| xUnit                 | 2.9.2  |

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

## Nova Feature: IBGE Education Admin (Área Administrador)

Resumo rápido:

- Objetivo: permitir que administradores consultem dados institucionais e indicadores educacionais do IBGE diretamente pela API e pela CLI.
- Acesso: somente contas com papel `Admin` (policy `RequireAdmin`).

### Rotas a adicionar (exemplos)

- `GET /admin/ibge/estados` — retorna lista de estados (id, sigla, nome)
- `GET /admin/ibge/indicadores` — parâmetros: `indicadorId`, `ano`, `uf` (opcional)

### Arquitetura proposta

- `RotinaXP.API.Infrastructure/Clients/IbgeClient.cs` — realiza chamadas HTTP ao IBGE.
- `RotinaXP.API.Application/Interfaces/IIbgeService.cs` — contrato de serviço.
- `RotinaXP.API.Application/Services/IbgeService.cs` — orquestra chamadas e transforma dados.
- `src/WebApi/Controllers/Admin/IbgeController.cs` — endpoints HTTP protegidos.

### Exemplo de implementação (C# simplificado)

IIbgeService:

```csharp
public interface IIbgeService {
  Task<IEnumerable<IbgeStateDto>> GetStatesAsync();
  Task<IbgeIndicatorDto> GetIndicatorAsync(string indicadorId, int ano, string uf = null);
}
```

IbgeClient (esboço):

```csharp
public class IbgeClient {
  private readonly HttpClient _http;
  public IbgeClient(HttpClient http) => _http = http;
  public async Task<IEnumerable<IbgeStateDto>> GetStatesAsync() {
    var r = await _http.GetFromJsonAsync<List<IbgeStateDto>>("/api/v1/localidades/estados");
    return r;
  }
}
```

Controller (esboço):

```csharp
[Authorize(Policy = "RequireAdmin")]
[Route("admin/ibge")]
public class IbgeController : ControllerBase {
  private readonly IIbgeService _svc;
  public IbgeController(IIbgeService svc) => _svc = svc;
  [HttpGet("estados")]
  public async Task<IActionResult> Estados() => Ok(await _svc.GetStatesAsync());
}
```

### Controle de Acesso (prática simples)

- Usar policy `RequireAdmin` configurada em `Program.cs`.
- Para desenvolvimento local, permitir `X-User-Role: Admin` apenas em `Development` (não recomendado em produção).

### Teste de Integração (exemplo)

- Criar `tests/Integration/IbgeIntegrationTests.cs`.
- Mockar `HttpMessageHandler` para retornar JSON de `estados` e validar que `IbgeService` mapeia corretamente para `IbgeStateDto`.
- O teste valida: requisição HTTP, desserialização e regras básicas de normalização.

### CLI

- Comando sugerido: `rotinaxp admin ibge estados` e `rotinaxp admin ibge indicador --id <id> --ano 2021 --uf SP`.
- CLI deve enviar token JWT com claim `role=Admin` ou usar credencial local para desenvolvimento.

### Boas práticas e observações

- Registrar `IbgeClient` com `AddHttpClient("IBGE", c => c.BaseAddress = new Uri("https://servicodados.ibge.gov.br"));`
- Tratar timeouts e retry (Polly) para robustez.
- Mapear apenas campos necessários e não persistir dados sensíveis.

## Issue e Git Flow (resumo)

- Branch: `entrega-intermediaria`
- Commits pequenos e atômicos: `feat(admin-ibge): add IbgeClient`, `feat(admin-ibge): add service and controller`, `test(integration): add IbgeClient tests`.
- PR: referenciar a issue e usar `Closes #<n>` para fechar automaticamente.

## Deploy (prático)

- Backend: publicar como container Docker e implantar em Azure App Service ou DigitalOcean App.
- CLI: empacotar como dotnet tool ou release ZIP/EXE no GitHub Releases.

## Ideias futuras (resumo rápido)

- Dashboards e mapas choropleth por UF/município.
- Relatórios programados por email para administradores.
- Integração com INEP para dados escolares por rede.
- ML para previsão de tendências educacionais.

---

Nota: criei também um guia rápido de execução em `QUICKSTART.md` para rodar a API e executar comandos básicos.
