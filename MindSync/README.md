# MindSync — API

Backend do MindSync, uma plataforma de mixagem sonora (frequências binaurais + texturas ambiente) para mitigação de estresse ocupacional. Feito em .NET 10 com Clean Architecture, CQRS (MediatR) e Dapper.

## Stack

- **.NET 10** / ASP.NET Core Web API
- **Dapper** / acesso a dados via SQL parametrizado (sem ORM completo, sem stored procedures)
- **MediatR** / CQRS (Commands e Queries) com pipeline de validação
- **FluentValidation** / validação de entrada, plugada automaticamente no pipeline do MediatR
- **BCrypt.Net** / hash de senha
- **JWT** (via cookie `HttpOnly`) / autenticação
- **SQL Server** (containerizado)
- **xUnit + NSubstitute + FluentAssertions** / testes unitários e de integração
- **Docker / Docker Compose**

## Arquitetura

O projeto segue Clean Architecture, com dependência sempre apontando para dentro (Domain não conhece nada das outras camadas):

```
MindSync.Domain            # Entidades, Value Objects, interfaces de Repository — sem dependência de nenhuma outra camada
MindSync.Application       # Commands, Queries, Handlers, Validators, DTOs de resposta — depende só do Domain
MindSync.Infrastructure    # Implementação dos Repositories (Dapper), TokenService, BCryptPasswordHasher, AudioMixComposer
MindSync.Api               # Controllers, middlewares, configuração de DI, Program.cs
MindSync.Tests             # Testes unitários (Domain/Application) e de integração (Api)
```

### Fluxo de uma requisição

```
Controller → mediator.Send(command/query)
           → ValidationBehaviour (roda o FluentValidation registrado para aquele request, se existir)
           → Handler (Application) → Repository (Infrastructure) → SQL Server
```

Os `Validators` e o `ValidationBehaviour` não têm nenhuma chamada explícita no código, pois são plugados via reflection (`AddValidatorsFromAssembly`) e resolvidos pelo pipeline genérico do MediatR (`IPipelineBehavior<,>`). Isso é intencional: qualquer `Command`/`Query` que tenha um `Validator` correspondente é validado automaticamente antes de chegar ao Handler.

### Padrão de entidades de Domínio

Toda entidade tem:
- Um construtor público para criação de uma nova instância (valida invariantes de negócio e lança `DomainException` se algo for inválido).
- Um construtor **privado sem parâmetros**, usado exclusivamente pelo Dapper via reflection para materializar o objeto a partir do resultado de uma query (`QueryAsync<T>`). Ele não aparece em nenhuma busca de "usages" da IDE, mas é ativamente necessário. **Removê-lo quebra o mapeamento automático do Dapper**.

## Como rodar

### Pré-requisitos
- Docker e Docker Compose

### Passo a passo

1. Copie o arquivo de exemplo de variáveis de ambiente na raiz:
   ```bash
   cp .env.example .env
   ```
2. Preencha o `.env` com valores reais (senha do SQL Server e segredo JWT são independentes um do outro):
   ```env
   MSSQL_SA_PASSWORD=Sua_Senha_F0rte!
   JWT_SECRET=substitua-por-um-segredo-longo-e-aleatório-de-pelo-menos-32-caracteres
   ```
3. Suba a stack:
   ```bash
   docker-compose up -d --build
   ```

Isso sobe o SQL Server, espera o serviço ficar pronto, cria o schema e o catálogo de faixas de áudio automaticamente (sem nenhum passo manual no banco), e então sobe a API. A criação do banco é feita pelo `DatabaseInitializer` (`MindSync.Infrastructure/Data/DatabaseInitializer.cs`), que executa os scripts `SqlServer_Setup.sql` e `AudioTracks_Seed.sql` (embutidos como Embedded Resource na DLL) a cada boot. Como os dois scripts são idempotentes (`IF NOT EXISTS`), rodar de novo em boots seguintes não tem efeito nenhum.

A API fica disponível em `http://localhost:5000`, com Swagger em `http://localhost:5000/swagger`.

## Testes

```bash
dotnet test
```

O projeto de testes (`MindSync.Tests`) tem duas categorias:
- **Testes unitários** (`UnitTests/`) / Domain e Application, com dependências mockadas via NSubstitute. Não precisam de nenhuma infraestrutura externa.
- **Testes de integração** (`IntegrationTests/`) / sobem o `Program.cs` real via `WebApplicationFactory<Program>`, e por isso **precisam de um SQL Server acessível** (rode `docker-compose up -d --build` antes, ou aponte para uma instância local).

O `Dockerfile` roda os testes unitários automaticamente durante o build da imagem como *quality gate* (se algum teste unitário quebrar, o `docker build` falha antes de gerar a imagem final). Os testes de integração são deliberadamente excluídos desse gate (não há SQL Server acessível durante o build da imagem).

## Endpoints principais

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/auth/register` | Cria um novo usuário |
| `POST` | `/api/auth/login` | Autentica e seta o cookie `HttpOnly` com o JWT |
| `GET` | `/api/auth/me` | Retorna o Id do usuário autenticado |
| `POST` | `/api/sessions` | Cria uma sessão de estresse |
| `PATCH` | `/api/sessions/{id}/complete` | Finaliza e avalia uma sessão |
| `GET` | `/api/sessions/users/{id}` | Histórico de sessões do usuário |
| `POST` | `/api/mixes/sessions/{id}` | Obtém ou cria a mixagem de áudio de uma sessão |
| `GET` | `/api/favoritedLists/mine` | Obtém as listas favoritadas do usuário logado |
| `GET` | `/api/favoritedLists/{id}` | Obtém uma lista favoritada do usuário logado |
| `POST` | `/api/favoritedLists` | Cria uma lista favoritada |
| `PUT` | `/api/favoritedLists/{id}` | Renomeia uma lista favoritada |
| `DELETE` | `/api/favoritedLists/{id}` | Deleta uma lista favoritada |
| `POST` | `/api/favoritedLists/{id}/sessions` | Adiciona uma sessão em uma lista |
| `PUT` | `/api/favoritedLists/sessions/{id}` | Renomeia uma sessão favoritada |
| `DELETE` | `/api/favoritedLists/sessions/{id}` | Remove uma sessão favoritada de uma lista |

Todas as rotas (exceto `register`/`login`) exigem o cookie de autenticação e aplicam verificação de posse (*ownership check*) / um usuário nunca acessa recurso de outro usuário, mesmo sabendo o Id.

## Segurança

- Senha com hash via BCrypt, nunca armazenada em texto puro.
- JWT entregue exclusivamente via cookie `HttpOnly` (nunca no corpo da resposta), o que evita roubo de token via XSS.
- Todo SQL é parametrizado via Dapper / sem concatenação de string, sem risco de SQL Injection.
- Mensagens de erro de login/registro são neutras de propósito (não revelam se um e-mail já existe ou se foi a senha que errou), para evitar enumeração de contas.
- Toda entidade que pertence a um usuário é validada por *ownership* em todos os Handlers, cobrindo contra ataques de IDOR (Insecure Direct Object Reference).