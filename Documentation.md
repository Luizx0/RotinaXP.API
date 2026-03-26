# RotinaXP - Backend API

## 📌 Visão Geral
API REST desenvolvida em ASP.NET Core para gerenciar usuários, tarefas, progresso e recompensas em um sistema de produtividade gamificado.

---

## 🏗️ Tecnologias

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Npgsql
- JWT Authentication

---

## 📁 Estrutura do Projeto


/Controllers
/Models
/DTOs
/Services
/Data
/Migrations


---

## ⚙️ Configuração Inicial

### 1. Clonar projeto

git clone ...


### 2. Configurar banco (appsettings.json)


"ConnectionStrings": {
"DefaultConnection": "Host=localhost;Port=5432;Database=rotinaxp;Username=postgres;Password=senha"
}


### 3. Rodar migrations


Add-Migration InitialCreate
Update-Database


---

## 🔐 Autenticação

- JWT Token
- Login retorna token
- Rotas protegidas com `[Authorize]`

---

## 📦 Entidades

### Usuario
- Id
- Nome
- Email
- SenhaHash
- Pontos

### Tarefa
- Id
- Nome
- Descricao
- Concluida
- Prioridade
- Frequencia
- UsuarioId

### ProgressoDiario
- Id
- Data
- TarefasConcluidas
- PontosGanhos
- UsuarioId

### Recompensa
- Id
- Nome
- CustoPontos
- UsuarioId
- DataResgate

---

## 🔌 Endpoints

### 🔐 Auth

#### POST /api/auth/register
Cria usuário

#### POST /api/auth/login
Retorna JWT

---

### 👤 Usuário

#### GET /api/user
Retorna dados do usuário

#### PUT /api/user
Atualiza dados

#### DELETE /api/user
Deleta conta

---

### ✅ Tarefas

#### GET /api/tasks
Lista tarefas

#### POST /api/tasks
Cria tarefa

#### PUT /api/tasks/{id}
Edita tarefa

#### DELETE /api/tasks/{id}
Remove tarefa

#### PATCH /api/tasks/{id}/complete
Marca como concluída

---

### 📊 Progresso

#### GET /api/progress
Lista histórico

#### GET /api/progress/{date}
Busca por data

---

### 🎁 Recompensas

#### GET /api/rewards
Lista recompensas

#### POST /api/rewards
Cria recompensa

#### POST /api/rewards/{id}/redeem
Resgata recompensa

---

## 🧠 Regras de Negócio

- Ao concluir tarefa:
  - Soma pontos ao usuário
  - Atualiza progresso diário

- Resgatar recompensa:
  - Verifica saldo
  - Deduz pontos

---

## 🚀 Fluxo da Aplicação

1. Usuário faz login
2. Recebe JWT
3. Front envia token
4. API valida token
5. API acessa banco
6. Retorna dados

---

## 📌 Próximos Passos

- Refresh Token
- Logs
- Validação com FluentValidation
- Testes unitários