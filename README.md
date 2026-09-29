# Auto Escola 3ESA

API REST para gerenciamento de instrutores, alunos e agendamento de instruções
de uma auto escola. Projeto base fornecido pela FIAP; evoluído a partir daí
por Caio Alexandre.

## Stack

- Java 25 + Spring Boot 4
- MySQL 8 + Flyway (migrations)
- Spring Security + JWT
- Swagger / OpenAPI (springdoc)

## Como rodar

### 1. Subir o banco de dados

Com Docker instalado, na raiz do projeto:

```bash
docker compose up -d
```

Isso sobe um MySQL em `localhost:3306` já com os bancos `autoescola3esa` e
`autoescola3esa_test` criados. Os dados ficam em um volume, então persistem
entre reinicializações do container.

Se preferir um MySQL instalado localmente em vez de Docker, crie os dois
bancos manualmente:

```sql
create database autoescola3esa;
create database autoescola3esa_test;
```

### 2. Rodar a aplicação

Pelo IntelliJ, execute a classe `AutoEscola3EsaApplication`, ou pelo terminal:

```bash
./mvnw spring-boot:run
```

O Flyway roda as migrations automaticamente na primeira subida, incluindo a
criação de um usuário administrador padrão.

### 3. Acessar

- API: `http://localhost:8085`
- Swagger UI: `http://localhost:8085/swagger-ui.html`
- Health check: `http://localhost:8085/health_check`

## Login padrão

A migration `V11` cria um usuário admin para o primeiro acesso:

| Login | Senha    |
|-------|----------|
| admin | admin123 |

**Troque essa senha (ou remova o usuário e crie outro) em qualquer ambiente
que não seja local/dev.**

## Configuração

As credenciais e o segredo do JWT saem do `application.properties` e vêm de
variáveis de ambiente, com defaults que reproduzem o setup local descrito
acima:

| Variável       | Default                  | Descrição                          |
|----------------|---------------------------|-------------------------------------|
| `DB_HOST`      | `localhost`                | Host do MySQL                       |
| `DB_PORT`      | `3306`                      | Porta do MySQL                      |
| `DB_NAME`      | `autoescola3esa`            | Banco principal                     |
| `DB_NAME_TEST` | `autoescola3esa_test`       | Banco usado pelos testes            |
| `DB_USERNAME`  | `root`                      | Usuário do banco                    |
| `DB_PASSWORD`  | `fiap`                      | Senha do banco                      |
| `JWT_SECRET`   | `12345678-troque-em-producao` | Segredo usado para assinar o JWT |
| `SERVER_PORT`  | `8085`                      | Porta da aplicação                  |

Para rodar com valores diferentes, defina as variáveis antes de subir a
aplicação, por exemplo:

```bash
export DB_PASSWORD=minhasenha
export JWT_SECRET=um-segredo-bem-maior-que-esse
./mvnw spring-boot:run
```

## Endpoints

| Método | Rota             | Acesso        | Descrição                          |
|--------|------------------|---------------|--------------------------------------|
| POST   | `/login`         | Público       | Autenticação, retorna JWT            |
| POST   | `/instrutores`   | ADMIN         | Cadastra instrutor                   |
| GET    | `/instrutores`   | ADMIN, USER   | Lista instrutores ativos (paginado)  |
| GET    | `/instrutores/{id}` | ADMIN      | Detalha instrutor                    |
| PUT    | `/instrutores`   | ADMIN         | Atualiza instrutor                   |
| DELETE | `/instrutores/{id}` | ADMIN      | Exclui (logicamente) instrutor       |
| POST   | `/alunos`        | ADMIN         | Cadastra aluno                       |
| GET    | `/alunos`        | ADMIN, USER   | Lista alunos ativos (paginado)       |
| GET    | `/alunos/{id}`   | ADMIN         | Detalha aluno                        |
| PUT    | `/alunos`        | ADMIN         | Atualiza aluno                       |
| DELETE | `/alunos/{id}`   | ADMIN         | Exclui (logicamente) aluno           |
| POST   | `/usuarios`      | ADMIN         | Cadastra usuário (senha em BCrypt)   |
| GET    | `/usuarios`      | ADMIN         | Lista usuários (paginado)            |
| POST   | `/instrucoes`    | ADMIN, USER   | Agenda instrução                     |
| GET    | `/instrucoes`    | ADMIN, USER   | Lista instruções ativas (paginado)   |
| GET    | `/instrucoes/{id}` | ADMIN, USER | Detalha instrução                    |
| DELETE | `/instrucoes/{id}` | ADMIN, USER | Cancela instrução (mín. 30 min de antecedência) |

## Pendências conhecidas (Fase 3)

Algumas exceções ainda não têm tratamento no `TratadorGlobalErros` e por enquanto
retornam **500** em vez do código HTTP correto:
- `ValidacaoException` (ex: login duplicado ao cadastrar usuário, regras de agendamento)
- `AlunoNotFoundException`
- Token JWT inválido ou expirado

Isso está previsto para a Fase 3 do plano de ação.

## Próximos passos

Consulte o plano de ação em andamento (Fases 2 a 6: CRUD de Aluno e Usuário,
tratamento de erros, Swagger completo, testes e preparação para o front
desktop).
