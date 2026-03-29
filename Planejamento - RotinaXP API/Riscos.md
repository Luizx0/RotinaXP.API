# Riscos e Mitigacoes

## Risco 1 - Regressao de contrato HTTP

- Impacto: Alto
- Probabilidade: Media
- Mitigacao:
  - Testes de integracao cobrindo status code e shape de resposta.
  - Validacao em homologacao antes de cada release.

## Risco 2 - Drift entre migrations e modelo

- Impacto: Alto
- Probabilidade: Media
- Mitigacao:
  - Processo padrao para gerar/aplicar migration com startup-project correto.
  - Checagem automatica no CI com banco efemero.

## Risco 3 - Crescimento de latencia em consultas

- Impacto: Medio
- Probabilidade: Media
- Mitigacao:
  - Monitorar p95/p99 por endpoint.
  - Revisar indices e estrategia de paginacao conforme volume.

## Risco 4 - Acoplamento entre camadas

- Impacto: Medio
- Probabilidade: Media
- Mitigacao:
  - Manter dependencia apontando para dentro (WebApi -> Application -> Domain).
  - Bloquear referencias indevidas com revisao tecnica e analyzers.
