# Pull Request / Branch Instructions

Use this template and commands to open the PR for the entrega-intermediaria work.

Commands (local):

```powershell
# create and switch to branch
git checkout -b entrega-intermediaria

# stage changes
git add .

# commit
git commit -m "fix: resolve tests, add IBGE integration, admin auth and tests"

# push
git push -u origin entrega-intermediaria
```

PR body (copy to GitHub PR description):

Title: feat(admin-ibge): integrate IBGE data for admin area

Body:

This PR implements the IBGE integration for the Admin area. It adds:

- `IbgeClient` (Infrastructure) — typed HttpClient to call IBGE
- `IIbgeService` and `IbgeService` — application service to transform IBGE responses
- `IbgeController` — protected endpoints under `/admin/ibge`
- Authorization: `RequireAdmin` policy and `AdminHandler` to allow role or `X-User-Role` header in dev/testing
- Integration test `tests/Integration/IbgeIntegrationTests.cs` (injects fake IBGE responses)
- Documentation updates (`API.DOCUMENTATION.md`, `QUICKSTART.md`)

Closes: Closes #<issue-number>

Checklist (use PR tasks):

- [ ] Code review
- [ ] CI green
- [ ] Merge to `entrega-intermediaria`

Notes:

- After merge to `entrega-intermediaria`, open a PR to `main` when ready.
- Use the `PR_ENTREGA_INTERMEDIARIA.md` file as a more detailed reference if needed.
