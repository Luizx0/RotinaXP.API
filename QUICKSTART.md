# Quickstart — RotinaXP API

Este guia rápido mostra os comandos mínimos para rodar a API localmente e testar a nova integração IBGE.

Requisitos:

- .NET 9 SDK
- PostgreSQL rodando (porta 5432)

1. Configurar senha do banco (exemplo via User Secrets):

```powershell
cd src/WebApi
dotnet user-secrets set "Database:Password" "sua_senha_aqui"
```

2. Aplicar migrations:

```powershell
cd <repo-root>
# rodar a partir da raiz do repo
dotnet ef database update --project src/Infrastructure/RotinaXP.API.Infrastructure/RotinaXP.API.Infrastructure.csproj --startup-project src/WebApi/RotinaXP.API.csproj
```

3. Rodar a API:

```powershell
dotnet run --project src/WebApi/RotinaXP.API.csproj
```

4. Testar endpoint IBGE (após implementação do controller):

```powershell
# Exemplo CURL local (assumindo token JWT com role Admin)
curl -H "Authorization: Bearer <TOKEN_COM_ROLE_ADMIN>" http://localhost:5252/admin/ibge/estados
```

5. Rodar testes:

```powershell
dotnet test RotinaXP.API.sln
```

6. Branch e PR (fluxo rápido):

```powershell
git checkout -b entrega-intermediaria
# fazer commits pequenos
git push -u origin entrega-intermediaria
# abrir PR no GitHub e referenciar a issue: Closes #<issue-number>
```

Notas:

- Para desenvolvimento sem JWT, documentar header `X-User-Role: Admin` e habilitar apenas em `ASPNETCORE_ENVIRONMENT=Development`.
- Para chamadas ao IBGE na infra, registre um HttpClient nomeado `IBGE` com `AddHttpClient`.

---

Guia rápido criado para testar e validar localmente. Para detalhes da arquitetura e passos de implementação, consulte `API.DOCUMENTATION.md`.
