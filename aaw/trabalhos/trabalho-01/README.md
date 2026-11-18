# OficinaAPI — Sistema de Gestão de Oficina Mecânica

Projeto prático semestral da disciplina **Arquitetura de Aplicações Web (2026.2)**.

- **Aluno:** Pablo Ernane Oliveira Vieira
- **RA:** 202451086472
- **Curso / Turma:** Ciência da Computação — 5º Período · Turma 3001 (GNP0547)
- **Professor:** Thalles Noce

---

## 1. Descrição do projeto e domínio

A **OficinaAPI** resolve o controle de atendimentos em pequenas oficinas mecânicas, substituindo fichas em papel e planilhas soltas por uma aplicação web completa. O sistema gerencia **Veículos** (`veiculos`) e **Ordens de Serviço** (`ordens_servico`) em relacionamento `1-N` por referência (`veiculoId`), garantindo regras de negócio na camada de serviço (normalização de placa, bloqueio de placa duplicada, cálculo de valor total no servidor, integridade referencial ao excluir e bloqueio de alteração após conclusão/cancelamento).

### Tecnologias utilizadas
- **Backend:** .NET 10 (`net10.0`) · ASP.NET Core Web API
- **Banco de dados NoSQL:** MongoDB (`MongoDB.Driver`)
- **Autenticação / Autorização:** JWT Bearer + RBAC (`admin` e `mecanico`) com hash de senha via `PasswordHasher`
- **Documentação:** OpenAPI / Swagger (`Swashbuckle.AspNetCore`)
- **Frontend:** HTML5, CSS3 e JavaScript puro com `fetch` assíncrono (sem recarregamento de página)
- **Testes unitários:** xUnit (`OficinaApi.Tests`)

---

## 2. Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (`10.0` ou superior)
- Instância **MongoDB** acessível (MongoDB Atlas ou MongoDB local na porta `27017`)
- Navegador web moderno (Chrome, Edge ou Firefox)

---

## 3. Variáveis de ambiente

A API não armazena senhas ou chaves no repositório. Configure as variáveis abaixo no terminal antes de iniciar:

| Variável | Obrigatória | Exemplo *(não é um valor real)* | Descrição |
|---|---|---|---|
| `MONGODB_URI` | Sim *(em produção/Atlas)* | `mongodb+srv://usuario:senha@cluster.exemplo.mongodb.net/?appName=Cluster0` | String de conexão do MongoDB (se omitida, tenta `mongodb://localhost:27017`) |
| `MONGODB_DATABASE` | Não | `OficinaDb` | Nome do banco no MongoDB (padrão: `OficinaDb`) |
| `JWT_KEY` | **Sim** | `ChaveSecretaDeExemploComPeloMenos32Caracteres!` | Chave HMAC-SHA256 de assinatura dos tokens JWT (mínimo de 32 caracteres) |

---

## 4. Como executar localmente

### 4.1. Clonar o repositório
```bash
git clone https://github.com/pabloernane11/ra-202451086472.git
cd ra-202451086472/aaw/trabalhos/trabalho-01
```

### 4.2. Subir a API Backend

**No Windows (PowerShell):**
```powershell
cd src\OficinaApi
$env:MONGODB_URI = "mongodb://localhost:27017"
$env:MONGODB_DATABASE = "OficinaDb"
$env:JWT_KEY = "ChaveSecretaDeExemploComPeloMenos32Caracteres!"
dotnet run
```

**No Linux / macOS (Bash):**
```bash
cd src/OficinaApi
export MONGODB_URI="mongodb://localhost:27017"
export MONGODB_DATABASE="OficinaDb"
export JWT_KEY="ChaveSecretaDeExemploComPeloMenos32Caracteres!"
dotnet run
```

*(Caso utilize o MongoDB Atlas, substitua `"mongodb://localhost:27017"` pela sua connection string do cluster.)*

A API estará disponível em: **`http://localhost:5290`**

---

## 5. Como acessar o Swagger (OpenAPI)

Com a API rodando, acesse no navegador:

- **`http://localhost:5290/swagger`**

### Fluxo rápido para testar rotas protegidas no Swagger:
1. Crie um usuário (`admin` ou `mecanico`) em `POST /api/v1/auth/registrar`.
2. Faça login em `POST /api/v1/auth/login` e copie apenas o valor do campo `token` (sem as aspas).
3. Clique no botão **Authorize** (topo da página), cole o token e confirme.
4. Teste os endpoints de `/api/v1/veiculos` e `/api/v1/ordens-servico` (`DELETE` exige perfil `admin`).

---

## 6. Como executar o Frontend web assíncrono

Com a API rodando em `http://localhost:5290`:

- Abra o arquivo `frontend/index.html` diretamente no navegador (ou sirva a pasta `frontend/` via Live Server / servidor HTTP estático).
- A interface realiza chamadas assíncronas via `fetch` e navega entre **Login/Cadastro → Lista de Veículos → Detalhe + Ordens de Serviço → Formulários** sem recarregar a página.

---

## 7. Como executar os testes unitários

Na pasta raiz do trabalho (`aaw/trabalhos/trabalho-01/`), execute um único comando:

```bash
dotnet test
```

Serão executados **7 testes unitários (xUnit)** cobrindo cenários de sucesso e de erro das camadas `VeiculoService` e `OrdemServicoService` usando repositórios em memória (`Fakes`).

---

## Uso de IA
- Ferramentas usadas: Antigravity (assistente de IA no editor) como apoio de estudo e arquitetura.
- Onde ajudou: estruturou os templates iniciais da configuração JWT/CORS no `Program.cs`, do painel web assíncrono em HTML/CSS/JS e dos testes unitários com repositórios Fake em xUnit.
- O que eu revisei/reescrevi: configurei o banco e variáveis de ambiente no MongoDB Atlas, validei manualmente todos os fluxos de status HTTP (`200`, `201`, `204`, `400`, `401`, `403`, `404`), ajustei a exibição amigável de status e erros sem expor códigos técnicos ao usuário final, e substituí `localStorage` por `sessionStorage` para isolar sessões entre abas.