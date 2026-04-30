# Frontend: Admin Area & IBGE Integration (Implementação)

Objetivo: instruir como adaptar o frontend (CLI ou SPA) para separar funcionalidades entre `user` e `admin`, e criar a tela/rota exclusiva de admin que consome os endpoints do backend para mostrar dados do IBGE por meio da ligacao do backend e frontend com o AXIOS.

1. Autenticação e autorização

- Backend fornece JWT com claim `role` (ex.: `"role": "Admin"`).
- Frontend deve armazenar token (localStorage para SPA; arquivo de config para CLI) e incluir header `Authorization: Bearer <token>` em chamadas.
- Para proteger rotas no frontend, verificar claim `role` no token decodificado; somente mostrar link/menu `Admin` se `role == Admin`.

2. Rotas e navegação

- SPA (React/Vue/Angular): adicionar rota protegida `/admin/ibge`.
  - Ex.: `PrivateRoute` que exige `isAuthenticated && user.role === 'Admin'`.
- CLI: adicionar comando `rotinaxp admin ibge estados` e `rotinaxp admin ibge indicador --id <id> --ano 2021 --uf SP`.

3. Consumo de endpoints backend

- Endpoints disponíveis (backend):
  - `GET /admin/ibge/estados` — lista de estados (id, sigla, nome)
  - `GET /admin/ibge/indicadores?indicadorId=<id>&ano=2021&uf=SP`

- Requisição HTTP (exemplo fetch):
  ```js
  const res = await fetch("/admin/ibge/estados", {
    headers: { Authorization: `Bearer ${token}` },
  });
  const states = await res.json();
  ```

4. Tela Admin (UI)

- Layout sugerido:
  - Menu lateral: IBGE > [Estados] [Indicadores]
  - Estados: tabela (Sigla | Nome | ID) com busca simples por nome/sigla
  - Indicadores: formulário com `indicadorId`, `ano`, `uf` e botão `Buscar` que exibe série/valores
- UX: oferecer botão `Export CSV` e visualização gráfica mínima (chart simples por período).

5. Tratamento de erros e loading

- Exibir spinner/estado de carregamento ao buscar dados.
- Se backend retornar 401/403, redirecionar para login e mostrar mensagem de permissão negada.
- Tratar erros 5xx com mensagem amigável e botão tentar novamente.

6. Testes e validação

- Testes unitários para componente de admin (mock fetch/responses).
- Testes E2E (Playwright/Cypress) que:
  - Faz login como admin
  - Acessa `/admin/ibge/estados` e verifica tabela

7. Exemplo de implementação mínima (pseudo-React)

- Verificar role:
  ```js
  function isAdmin(token) {
    const payload = JSON.parse(atob(token.split(".")[1]));
    return payload.role === "Admin";
  }
  ```
- Componente que busca estados:
  ```js
  useEffect(() => {
    setLoading(true);
    fetch("/admin/ibge/estados", {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then((r) => r.json())
      .then(setStates)
      .finally(() => setLoading(false));
  }, []);
  ```

8. Observações de segurança

- Não exibir dados admin para usuários sem role.
- Validar sempre no backend (front-end só melhora UX).
- Em produção, não use `X-User-Role` header — é apenas para testes locais.

9. Integração com CLI

- Se a interface principal for CLI, implemente subcomando `admin ibge` que solicita token (ou usa token salvo) e faz as mesmas chamadas ao backend, exibindo tabela no terminal.

10. Pontos de extensão futura

- Dashboard com mapas (leaflet/d3) para visualizar indicadores por município/UF.
- Cache no frontend para evitar consultas repetidas.

Pronto para qualquer exemplo de código específico (React/Vue/CLI Node) se quiser que eu gere.
