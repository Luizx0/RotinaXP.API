# Planejamento - RotinaXP API

Esta pasta centraliza o planejamento tecnico do projeto apos a reorganizacao para arquitetura em camadas.

## Objetivos

- Escalar com seguranca e previsibilidade.
- Evoluir sem quebrar contratos HTTP existentes.
- Melhorar operacao, testes e produtividade da equipe.

## Estrutura desta pasta

- Roadmap.md: fases de evolucao por prioridade.
- Backlog.md: lista priorizada de entregas.
- Riscos.md: riscos ativos, impacto e mitigacoes.
- Notas Privadas.md: espaco pessoal para rascunhos e ideias (ignorado no Git).

## Ideias de evolucao do produto (grandes)

- Chat em tempo real entre usuarios para accountability de rotina.
- Modo comunidade com grupos, desafios semanais e ranking por grupo.
- Feed estilo rede social para compartilhar metas cumpridas e progresso.
- Perfis publicos (com privacidade configuravel) para acompanhar amigos.
- Sistema de posts com curtidas, comentarios e hashtags de produtividade.
- Marketplace de recompensas entre usuarios (troca de pontos por beneficios).
- Eventos sazonais (streak month, sprint de foco, torneios de habito).

## Ideias com IA (alto impacto)

- Agente de IA para planejar semana automaticamente com base no historico.
- Sugestao inteligente de tarefas diarias com prioridade dinamica.
- Resumo semanal automatico com pontos fortes, gargalos e recomendacoes.
- Coach conversacional para acompanhar objetivos e manter consistencia.
- Classificacao automatica de tarefas por categoria e dificuldade.
- Detector de risco de abandono de habito com alertas preventivos.

## Ideias medias (intermediarias)

- Notificacoes push e lembretes por janela de foco.
- Regras de automacao (se concluir X tarefas, criar recompensa Y).
- Templates de rotina por perfil (estudo, fitness, trabalho, familia).
- Calendario de progresso com visao mensal.
- Importacao e exportacao de dados do usuario.
- Sistema de badges e conquistas por marcos.

## Ideias pequenas e faceis (quick wins)

- Filtro e ordenacao avancada em tarefas e recompensas.
- Busca por titulo em tarefas/recompensas.
- Endpoint de dashboard consolidado (tarefas, pontos, streak, progresso diario).
- Tema claro/escuro no frontend (quando existir cliente web).
- Melhorias de UX na API: mensagens de erro mais claras por contexto.
- Endpoint para duplicar tarefa recorrente.
- Marcar tarefa como favorita.
- Configuracao de meta diaria de tarefas e alerta quando atingir.

## Ritmo sugerido

- Revisao semanal de backlog e riscos.
- Revisao quinzenal de arquitetura e SLOs.
- Revisao por release dos indicadores de performance e erro.
