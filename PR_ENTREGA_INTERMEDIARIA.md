Title: feat(admin-ibge): integrate IBGE data for admin area

Description:

This PR implements the IBGE integration for the Admin area. It adds:

- `IbgeClient` (Infrastructure) — typed HttpClient to call IBGE
- `IIbgeService` and `IbgeService` — application service to transform IBGE responses
- `IbgeController` — protected endpoints under `/admin/ibge`
- Authorization: `RequireAdmin` policy and `AdminHandler` to allow role or `X-User-Role` header in dev/testing
- Integration test `tests/Integration/IbgeIntegrationTests.cs` (injects fake IBGE responses)
- Documentation updates (`API.DOCUMENTATION.md`, `QUICKSTART.md`)

Closes: Closes #<issue-number>

Checklist:

- [ ] Code review
- [ ] CI green
- [ ] Merge to `entrega-intermediaria` branch

Notes:

- The admin endpoints require a user with `role=Admin` in JWT. For development/testing the header `X-User-Role: Admin` is accepted by the authorization handler.
