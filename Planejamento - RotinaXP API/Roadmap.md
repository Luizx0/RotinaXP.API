# Roadmap Tecnico

## Fase 1 - Fundacao (curto prazo)

- Consolidar a estrutura em camadas (Core, Infrastructure, Shared, WebApi).
- Garantir cobertura minima de testes para fluxos criticos (auth, tasks, rewards).
- Padronizar contratos de erro e paginacao em todos endpoints.
- Validar migrations e processo de deploy em ambiente de homologacao.

## Fase 2 - Escala de dominio (medio prazo)

- Introduzir interfaces de aplicacao para todos servicos restantes.
- Expandir vertical slices para Users e DailyProgress.
- Adicionar politicas de cache para consultas de leitura frequente.
- Criar testes de concorrencia para cenarios de pontos e resgate.

## Fase 3 - Operacao robusta (medio/longo prazo)

- Definir SLOs (latencia, erro, disponibilidade) e alertas no stack de observabilidade.
- Instrumentar tracing de ponta a ponta com padrao de correlacao.
- Implementar estrategia de retries idempotentes para operacoes sensiveis.
- Formalizar runbooks operacionais e playbooks de incidente.

## Fase 4 - Evolucao arquitetural (longo prazo)

- Separar casos de uso de escrita e leitura quando houver gargalo real.
- Avaliar mensageria para eventos de dominio de alta frequencia.
- Revisar particionamento de dados e indices conforme crescimento de carga.
