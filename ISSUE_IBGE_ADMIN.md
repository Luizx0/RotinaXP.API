Title: Implementar integração Admin com API do IBGE (Entrega Intermediária)

Descrição:

Implementar uma área exclusiva para administradores que consome dados públicos do IBGE e disponibiliza indicadores educacionais via API interna e pela CLI.

Objetivos técnicos:

- Implementar cliente HTTP para a API do IBGE (`IbgeClient`).
- Criar serviço `IIbgeService` / `IbgeService` para orquestração e transformação.
- Expor endpoints protegidos por policy `RequireAdmin` em `IbgeController`.
- Adicionar teste de integração que valida mapeamento e rota.
- Atualizar documentação e criar guia rápido (`QUICKSTART.md`).

APIs utilizadas:

- `https://servicodados.ibge.gov.br/api/v1/localidades/estados` (estados)
- `https://apisidra.ibge.gov.br/values/{tableId}` (SIDRA - indicadores)

Checklist:

- [ ] Criar branch `entrega-intermediaria`
- [x] Implementar `IbgeClient` (Infrastructure)
- [x] Implementar `IIbgeService` e `IbgeService` (Application/Infrastructure)
- [x] Adicionar `IbgeController` com policy `RequireAdmin`
- [x] Registrar HttpClient e service no DI
- [x] Adicionar teste de integração (mocked HttpClient)
- [x] Atualizar `API.DOCUMENTATION.md` e adicionar `QUICKSTART.md`
- [ ] Abrir Pull Request e referenciar esta issue (`Closes #<id>`)

Valor da feature:

Fornece contexto estatístico e institucional para administradores, agrega valor analítico ao sistema e demonstra integração com API pública — contribuição de alto valor técnico para a entrega intermediária.
