# Auto Escola Back-End (Arquitetura Sênior / SaaS)

Este repositório contém o Back-End remodelado em **C# (.NET 8)** seguindo princípios de **Clean Architecture**, **Domain-Driven Design (DDD)**, **RBAC (Controle de Acesso Baseado em Roles)** e **Padronização de Erros (RFC 7807 ProblemDetails)**.

---

## Como Rodar Imediatamente (Zero-Config)

O projeto está pré-configurado com **Entity Framework Core In-Memory** (`"UseInMemoryDatabase": true` no `appsettings.json`) e um **Data Seeder automático** (`DbInitializer.cs`), permitindo rodar e testar todos os fluxos no Swagger imediatamente sem precisar subir instância do SQL Server primeiro.

```bash
dotnet restore
dotnet run
```

Acesse no navegador a URL exibida no terminal (ex: `http://localhost:5000`) — o **Swagger UI** abrirá direto na raiz com suporte ao botão **Authorize (JWT)**.

*(Para usar **SQL Server**, basta mudar `"UseInMemoryDatabase": false` no `appsettings.json` e rodar `dotnet ef database update`).*

---

## Credenciais Prontas para Teste (Demo Mode / Seed)

Você pode logar no endpoint `POST /api/auth/login` usando tanto o **E-mail** quanto o **CPF**:

| Perfil (Role) | E-mail | CPF | Senha | `profileId` Gerado |
| :--- | :--- | :--- | :--- | :--- |
| **ADMIN** | `admin@autoescola.com` | `00000000001` | `Admin@123` | `null` |
| **STUDENT** | `aluno@autoescola.com` | `12345678909` | `Aluno@123` | `bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1` |
| **INSTRUCTOR** | `roberto@autoescola.com` | `11122233344` | `Instrutor@123` | `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1` |
| **INSTRUCTOR** | `fernanda@autoescola.com` | `55566677788` | `Instrutor@123` | `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2` |

---

## O Que Foi Remodelado Nesta Arquitetura

1. **Identidade Unificada (`User` + Perfis 1:1):**
   * Autenticação centralizada na entidade `User` com hash `BCrypt`, geração de `AccessToken` (JWT contendo `role` e `profileId`) e `RefreshToken`.
   * Fluxo de **Primeiro Acesso** (`MustChangePassword = true`) quando a secretaria cadastra um novo aluno em `POST /api/management/students` (gera senha temporária com os 6 primeiros dígitos do CPF).
2. **Motor de Agendamento com Validações Reais do DETRAN (`LessonBookingService.cs`):**
   * **Anti-Double-Booking:** Impede colisão de horário para Aluno, Instrutor e Veículo simultaneamente.
   * **Compatibilidade de Categoria:** Impede que um aluno de categoria `B` agende uma moto (`A`) ou um instrutor não habilitado.
   * **Bloqueio de Frota:** Impede agendamento em veículos com status `MAINTENANCE` ou `INACTIVE`.
   * **Limite Diário:** Máximo de 3 aulas práticas por dia para o mesmo aluno.
   * **Regra de Cancelamento de 24h:** Alunos só podem cancelar aulas autonomamente com até 24h de antecedência.
3. **Endpoints pensados para UX Moderna:**
   * `GET /api/lessons/available-slots?date=2026-10-20&category=B`: Alimenta o **Wizard de Agendamento em 3 Passos** do Front-End retornando cada horário do dia e quais duplas de `Instrutor + Veículo` estão livres naquele slot.
   * `GET /api/dashboard/student/{studentId}`: Alimenta a barra de progresso da CNH (Exame Médico -> Teórico -> Aulas Práticas `12/20` -> Status de aptidão para prova prática) + card da próxima aula.
   * `GET /api/dashboard/admin`: Alimenta os KPIs operacionais da secretaria.
