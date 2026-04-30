# RotinaXP

RotinaXP é uma aplicação para organização da rotina de estudos com backend em .NET (API) e cliente CLI.

## Entrega Intermediária: IBGE Education Admin

Esta entrega adiciona uma área exclusiva para administradores que consome dados do IBGE e exibe indicadores educacionais.

Principais arquivos adicionados:

- `src/Infrastructure/RotinaXP.API.Infrastructure/Clients/IbgeClient.cs`
- `src/Infrastructure/RotinaXP.API.Infrastructure/Services/IbgeService.cs`
- `src/Core/RotinaXP.API.Application/Interfaces/Services/IIbgeService.cs`
- `src/Core/RotinaXP.API.Application/DTOs/IbgeDTOs.cs`
- `src/WebApi/Controllers/Admin/IbgeController.cs`
- `tests/Integration/IbgeIntegrationTests.cs`
- `API.DOCUMENTATION.md` (atualizado)
- `QUICKSTART.md` (adicionado)

Como testar localmente:

1. Criar branch:

```powershell
git checkout -b entrega-intermediaria
```

2. Rodar migrations e iniciar a API (veja `QUICKSTART.md`):

```powershell
dotnet ef database update --project src/Infrastructure/RotinaXP.API.Infrastructure/RotinaXP.API.Infrastructure.csproj --startup-project src/WebApi/RotinaXP.API.csproj
dotnet run --project src/WebApi/RotinaXP.API.csproj
```

3. Criar usuário e usar token JWT com claim `role=Admin` para consultar endpoints:

- `GET /admin/ibge/estados`
- `GET /admin/ibge/indicadores?indicadorId=<id>&ano=2021&uf=SP`

Observações:

- Para desenvolvimento/integração de teste, o header `X-User-Role: Admin` é aceito pela policy `RequireAdmin`.

---

Consulte `API.DOCUMENTATION.md` para detalhes de arquitetura, implementação e ideias futuras.
