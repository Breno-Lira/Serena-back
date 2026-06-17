# Serena — Backend

Backend enxuto com **duas** APIs independentes:

| API | Stack | Porta (host) | Função |
|-----|-------|--------------|--------|
| `api-user` | .NET 8 (C#) | `5000` | CRUD de usuários, login e redefinição de senha |
| `api-dashboard` | Java 17 / Spring Boot | `8080` | Indicadores a partir de um CSV de dados de violência |
| `api-ia` | Python 3.12 / FastAPI | `8000` | Predição do tipo de violência (Random Forest e XGBoost) |

As APIs de **denúncia, login** e o **gateway** foram removidas do projeto. A API de **IA** foi reintroduzida (`api-ia`), servindo os modelos treinados.

---

## Arquitetura do `api-user` (camadas)

O projeto segue arquitetura em camadas, com dependências sempre apontando "para dentro":

```
api-user (API / apresentação)
   │  Controllers, Program.cs (injeção de dependência), Swagger
   ▼
ServiceUser (Serviço / regras de negócio)
   │  IUserService, UserService, DTOs, Profiles (AutoMapper)
   ▼
InfrastructureUser (Infraestrutura)
   │  AppDbContext (EF Core), Migrations, Repositories (repositório genérico)
   ▼
DominioUser (Domínio)
      User, Endereco, Apoios
```

Principais ajustes feitos em relação à versão original:

- O repositório genérico (`IGenericRepository` / `GenericRepositoryEntity`) **estava em outra solução** (a de denúncia, via caminho `..\..\backend\...`). Foi trazido para dentro de `InfrastructureUser/Repositories`, eliminando o acoplamento entre soluções.
- A injeção de dependência no `Program.cs` foi corrigida: o projeto da API agora referencia explicitamente `InfrastructureUser`, e os tipos usados (`AppDbContext`, repositório genérico) resolvem corretamente.
- Removido todo o cliente da API de denúncia (`IDenunciaApiClient`/`DenunciaApiClient`, `AddHttpClient` e a configuração `DenunciaApi`), que dependia de um serviço que não existe mais.
- `VerifyUniqueFieldsAsync` deixou de "engolir" a exceção de negócio — agora o `409 Conflict` volta a funcionar ao cadastrar e‑mail/CPF/RG duplicado.
- Troca de `EnsureCreated()` por `Database.Migrate()` (as migrations passam a ser a fonte da verdade do schema).
- Connection string passou de `localdb` (Windows) para SQL Server, configurável por variável de ambiente — funciona em Linux/Docker.
- Removidos arquivos de template/lixo (`WeatherForecast.cs`, `.http`, `.csproj.user`), namespaces de DTO unificados em `ServiceUser.DTOs`.

## Arquitetura do `api-dashboard`

```
Controller  ->  Service  ->  Repository  ->  Domínio (RegistroViolencia)
```

- O código-fonte foi movido para o layout padrão do Maven (`src/main/java`, `src/main/resources`) — antes estava em `main/java` e o Maven não o encontrava.
- O `CsvRepository` agora lê o CSV do classpath (antes usava um caminho fixo do Windows) e devolve objetos de domínio; o `DashboardService` consome o repositório, sem mais índices "mágicos" de coluna.
- `pom.xml` tornou‑se independente (sem o agregador que amarrava os módulos removidos) e com dependências enxutas.

---

## Como rodar

### Opção 1 — Docker (recomendada)

Sobe o SQL Server (dependência do `api-user`) e as duas APIs com um comando:

```bash
docker compose up --build
```

Depois de subir:

- `api-user`  → http://localhost:5000/swagger
- `api-dashboard` → http://localhost:8080/dashboard/total
- `api-ia` → http://localhost:8000/docs

O banco é criado automaticamente (as migrations rodam no startup). Os dados do SQL Server ficam num volume (`sqlserver-data`), então persistem entre reinícios.

Para parar e limpar tudo (inclusive o volume do banco):

```bash
docker compose down -v
```

### Opção 2 — Execução local (sem Docker)

Requisitos:

- **.NET SDK 8.0** (para o `api-user`)
- **JDK 17+** e **Maven 3.9+** (para o `api-dashboard`)
- Uma instância de **SQL Server** acessível (pode ser só o container do banco: `docker compose up sqlserver`)

`api-user`:

```bash
cd api-user
# Ajuste a connection string se necessário (appsettings.json ou variável de ambiente):
export ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=User-API;User Id=sa;Password=Your_strong_Pass123;TrustServerCertificate=True;MultipleActiveResultSets=true"
dotnet run --project api-user/api-user.csproj
```

`api-dashboard`:

```bash
cd api-dashboard
mvn spring-boot:run
```

---

## Endpoints

### api-user (`/api/user`)

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/user` | Cria usuário |
| GET | `/api/user/{id}` | Busca usuário (CPF mascarado) |
| GET | `/api/user/internal/{id}` | Busca usuário (uso interno, sem máscara) |
| PUT | `/api/user/{id}` | Atualiza usuário |
| DELETE | `/api/user/{id}` | Remove usuário |
| POST | `/api/user/login` | Autentica (e‑mail + senha) |
| POST | `/api/user/reset-password` | Redefine a senha |

### api-dashboard (`/dashboard`)

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/dashboard/total` | Total de registros |
| GET | `/dashboard/municipios` | Casos por município |
| GET | `/dashboard/violencia-fisica` | Violência física por município |
| GET | `/dashboard/violencia-por-ano` | Tipos de violência por ano |
| GET | `/dashboard/casos-por-idade` | Casos por faixa etária |
| GET | `/dashboard/casos-por-hora` | Casos por hora do dia |

### api-ia (`/ia`)

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/ia/health` | Status e modelos carregados |
| GET | `/ia/features` | Lista das 51 features e das classes |
| POST | `/ia/prever` | Predição a partir das respostas do questionário |
| POST | `/ia/prever-vetor` | Predição a partir de um vetor cru `{feature: 0/1}` |

Modelos disponíveis: `rf` (Random Forest) e `xgb` (XGBoost, padrão). Detalhes e exemplos em `api-ia/README.md`.

---

## Observação de segurança

O hash de senha do `api-user` é apenas ilustrativo (Base64). Antes de produção, troque por **BCrypt**, **PBKDF2** ou **Argon2**, e considere autenticação via JWT.
