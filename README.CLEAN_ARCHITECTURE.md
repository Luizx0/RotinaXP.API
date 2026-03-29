# RotinaXP - Guia de Migracao para Clean Architecture

Este arquivo guarda o passo a passo para evoluir o projeto para Clean Architecture sem interromper o desenvolvimento.

## Objetivo

Migrar gradualmente para uma arquitetura escalavel e de facil manutencao, mantendo os endpoints atuais funcionando durante a transicao.

## Estrutura alvo

```text
RotinaXP.API.sln
  src/
    RotinaXP.Domain/
    RotinaXP.Application/
    RotinaXP.Infrastructure/
    RotinaXP.API/
  tests/
    RotinaXP.Domain.Tests/
    RotinaXP.Application.Tests/
    RotinaXP.API.Tests/
```

## Regras de dependencia

- Domain nao depende de nenhuma outra camada.
- Application depende apenas de Domain.
- Infrastructure depende de Application e Domain.
- API depende de Application e usa Infrastructure via DI.

## Passo a passo

### 1) Criar projetos de camada

Executar na raiz da solucao:

```bash
dotnet new classlib -n RotinaXP.Domain -o src/RotinaXP.Domain
dotnet new classlib -n RotinaXP.Application -o src/RotinaXP.Application
dotnet new classlib -n RotinaXP.Infrastructure -o src/RotinaXP.Infrastructure

dotnet sln RotinaXP.API.sln add src/RotinaXP.Domain/RotinaXP.Domain.csproj
dotnet sln RotinaXP.API.sln add src/RotinaXP.Application/RotinaXP.Application.csproj
dotnet sln RotinaXP.API.sln add src/RotinaXP.Infrastructure/RotinaXP.Infrastructure.csproj
```

### 2) Conectar referencias entre camadas

```bash
dotnet add src/RotinaXP.Application/RotinaXP.Application.csproj reference src/RotinaXP.Domain/RotinaXP.Domain.csproj
dotnet add src/RotinaXP.Infrastructure/RotinaXP.Infrastructure.csproj reference src/RotinaXP.Domain/RotinaXP.Domain.csproj
dotnet add src/RotinaXP.Infrastructure/RotinaXP.Infrastructure.csproj reference src/RotinaXP.Application/RotinaXP.Application.csproj
dotnet add RotinaXP.API.csproj reference src/RotinaXP.Application/RotinaXP.Application.csproj
dotnet add RotinaXP.API.csproj reference src/RotinaXP.Infrastructure/RotinaXP.Infrastructure.csproj
```

### 3) Mover entidades para Domain

Migrar entidades de negocio:

- User
- TaskItem
- Reward
- DailyProgress

Diretriz: manter Domain sem EF annotations e sem dependencias externas.

### 4) Criar casos de uso em Application

Criar:

- Interfaces de repositorio (IUserRepository, ITaskRepository, IRewardRepository, IDailyProgressRepository)
- IUnitOfWork
- Use cases (RegisterUser, LoginUser, CompleteTask, RedeemReward)

Diretriz: regras de negocio ficam em Domain/Application, nunca em controller.

### 5) Implementar infraestrutura

Em Infrastructure:

- ApplicationDbContext
- Repositorios EF Core
- Mapeamentos Fluent API
- Implementacao de UnitOfWork

Boas praticas para escala:

- Indices por UserId e Date
- Restricoes unicas quando necessario
- Concorrencia otimista para atualizacao de pontos

### 6) Reduzir controllers a orquestracao HTTP

Controllers devem:

- Validar input
- Chamar use case
- Converter resultado para status HTTP

Sem regra de negocio nos controllers.

### 7) Organizar DI por camada

Criar extension methods:

- AddApplication()
- AddInfrastructure(IConfiguration)

Registrar no Program da API.

### 8) Testes por camada

- Domain.Tests: regras puras de dominio
- Application.Tests: fluxos de caso de uso com mocks/fakes
- API.Tests: contrato HTTP e integracao

## Ordem recomendada de migracao de modulos

1. Auth e Users
2. Tasks e gamificacao
3. Rewards e redeem
4. DailyProgress

## Definicao de pronto por modulo

- Endpoints existentes mantidos
- Testes passando
- Regra de negocio coberta em teste
- Nenhum acesso direto a DbContext fora de Infrastructure

## Dica de execucao

Trabalhe por branches curtas e pequenas entregas. Em cada modulo migrado, rode testes de regressao antes do merge.
