# Biblioteca API

API RESTful em **C# .NET 10** com **Entity Framework Core** para gerenciar o acervo de uma biblioteca (autores e livros).

> Projeto da disciplina **C# Software Development** — CP5.

## Integrantes

| Nome | RM |
|------|----|
| NOME DO INTEGRANTE 1 | RM00000 |
| NOME DO INTEGRANTE 2 | RM00000 |
| NOME DO INTEGRANTE 3 | RM00000 |

## Contexto do projeto

Bibliotecas pequenas e escolares costumam controlar o acervo em planilhas ou cadernos, o que gera livros duplicados, dados inconsistentes e dificuldade para saber quais obras de cada autor estão disponíveis.

A **Biblioteca API** resolve isso oferecendo um serviço centralizado para cadastrar, consultar, atualizar e remover **autores** e **livros**, com validações (ISBN único, autor obrigatório para cada livro, campos obrigatórios) e respostas padronizadas.

**Público-alvo:** bibliotecários e desenvolvedores de sistemas (sites, apps ou painéis) que precisem consumir os dados do acervo.

## Tecnologias

- C# / .NET 10 (ASP.NET Core Web API com controllers)
- Entity Framework Core 10 + Migrations
- **Banco de dados: SQLite** (arquivo `biblioteca.db`, criado automaticamente)
- Asp.Versioning (versionamento por URL)
- Swagger (Swashbuckle) para documentação e testes

## Como rodar localmente

Pré-requisitos: [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) e Git. Não é necessário instalar o SQLite.

```bash
git clone URL_DO_REPOSITORIO
cd NOME_DA_PASTA/src/Biblioteca.Api
dotnet restore
dotnet run
```

Ao iniciar, a aplicação aplica as migrations automaticamente e cria o banco com dados de exemplo (2 autores e 3 livros).
Abra o endereço exibido no terminal (ex.: `http://localhost:5xxx`). A raiz (`/`) redireciona para o **Swagger** em `/swagger`.

## Banco de dados e Migrations

O mapeamento é feito pelo `AppDbContext` (`Data/AppDbContext.cs`) com Fluent API. Relacionamento **1:N**: um `Autor` possui vários `Livros`. O ISBN possui índice único.

Comandos usados (executar dentro de `src/Biblioteca.Api`):

```bash
# instalar a ferramenta (uma única vez)
dotnet tool install --global dotnet-ef

# criar a migration inicial
dotnet ef migrations add Inicial --output-dir Data/Migrations

# aplicar no banco (também é feito automaticamente ao iniciar a API)
dotnet ef database update
```

Migration documentada: **`Inicial`** — cria as tabelas `Autores` e `Livros`, a chave estrangeira, o índice único de ISBN e os dados iniciais (seed).

## Versionamento

A versão da API vai na URL: `/api/v1/...`. Para criar uma nova versão, basta adicionar controllers com `[ApiVersion("2.0")]` mantendo a v1 intacta.

## Endpoints (v1)

### Autores

| Método | Rota | Descrição | Status de sucesso |
|--------|------|-----------|-------------------|
| GET | `/api/v1/autores` | Lista autores (filtro opcional `?nome=`) | 200 |
| GET | `/api/v1/autores/{id}` | Busca autor por id | 200 / 404 |
| GET | `/api/v1/autores/{id}/livros` | Lista os livros de um autor | 200 / 404 |
| POST | `/api/v1/autores` | Cadastra autor | 201 / 400 |
| PUT | `/api/v1/autores/{id}` | Atualiza autor | 204 / 400 / 404 |
| DELETE | `/api/v1/autores/{id}` | Remove autor (sem livros vinculados) | 204 / 404 / 409 |

### Livros

| Método | Rota | Descrição | Status de sucesso |
|--------|------|-----------|-------------------|
| GET | `/api/v1/livros` | Lista livros (filtros `?titulo=` e `?autorId=`) | 200 |
| GET | `/api/v1/livros/{id}` | Busca livro por id | 200 / 404 |
| POST | `/api/v1/livros` | Cadastra livro | 201 / 400 / 409 |
| PUT | `/api/v1/livros/{id}` | Atualiza livro | 204 / 400 / 404 / 409 |
| DELETE | `/api/v1/livros/{id}` | Remove livro | 204 / 404 |

Erros seguem o padrão `ProblemDetails` (RFC 7807). Exemplos de corpo para `POST`:

```json
// POST /api/v1/autores
{ "nome": "Lima Barreto", "nacionalidade": "Brasileira", "dataNascimento": "1881-05-13" }

// POST /api/v1/livros
{ "titulo": "Triste Fim de Policarpo Quaresma", "isbn": "978-85-0000-004-2", "anoPublicacao": 1915, "genero": "Romance", "autorId": 1 }
```

## Estrutura do projeto

```
src/Biblioteca.Api/
├── Controllers/V1/   # AutoresController, LivrosController
├── Models/           # Entidades (Autor, Livro)
├── Dtos/             # Requests e responses
├── Data/             # AppDbContext e Migrations
├── Mappings/         # Conversão entidade -> DTO
├── Middleware/       # Tratamento global de erros
└── Program.cs
```

## Evidências de teste

Prints de cada endpoint funcionando (Swagger) na pasta [`docs/evidencias`](docs/evidencias).
