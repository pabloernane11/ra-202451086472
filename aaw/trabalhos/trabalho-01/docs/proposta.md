# Proposta — Trabalho Semestral · Arquitetura de Aplicações Web 2026.2

- **Aluno:** Pablo Ernane Oliveira Vieira · **RA:** 202451086472 · **Curso/Turma:** Ciência da Computação / 5º Período
- **Repositório:** https://github.com/pabloernane11/ra-202451086472

## 1. Domínio e problema

O sistema **OficinaAPI** é voltado para o gerenciamento de pequenas oficinas mecânicas. O objetivo é resolver a desorganização de orçamentos e fichas feitas em papel ou planilhas soltas. A aplicação permite cadastrar os veículos dos clientes e controlar todo o andamento das ordens de serviço (peças, mão de obra, valores e status do conserto), centralizando as informações e evitando erros de cobrança.

## 2. Entidades

- **Entidade A — `Veiculo`:**
  - `id` (string / ObjectId)
  - `placa` (string — único)
  - `marca` (string)
  - `modelo` (string)
  - `ano` (int)
  - `nomeProprietario` (string)
  - `telefoneProprietario` (string)

- **Entidade B — `OrdemServico`:**
  - `id` (string / ObjectId)
  - `veiculoId` (string — referência ao `Veiculo`)
  - `descricaoProblema` (string)
  - `status` (string: `Aberta`, `EmAndamento`, `Concluida`, `Cancelada`)
  - `valorPecas` (decimal)
  - `valorMaoDeObra` (decimal)
  - `valorTotal` (decimal — calculado pela API)
  - `dataAbertura` (DateTime)

- **Relacionamento e Modelagem NoSQL:**
  Relacionamento **1-N** (um veículo pode ter várias ordens de serviço ao longo do tempo), modelado por **referência (`veiculoId`)** em duas coleções separadas: `veiculos` e `ordens_servico`.
-  **Justificativa:** Como o histórico de reparos de um carro tende a crescer com o passar dos anos e a oficina precisa listar e filtrar ordens de serviço ativas diretamente na tela inicial, usar referência evita documentos gigantes dentro de veículo (anti-pattern de *unbounded array*) e mantém as consultas rápidas e diretas.

## 3. Endpoints previstos

### Veículos (`/api/v1/veiculos`)
- `GET /api/v1/veiculos` — Lista todos os veículos.
- `GET /api/v1/veiculos/{id}` — Busca um veículo por ID.
- `POST /api/v1/veiculos` — Cadastra um novo veículo (valida placa duplicada).
- `PUT /api/v1/veiculos/{id}` — Atualiza os dados de um veículo.
- `DELETE /api/v1/veiculos/{id}` — Remove um veículo (restrito a `admin`).

### Ordens de Serviço (`/api/v1/ordens-servico`)
- `GET /api/v1/ordens-servico` — Lista todas as ordens de serviço (com filtro opcional por status ou veículo).
- `GET /api/v1/ordens-servico/{id}` — Busca os detalhes de uma ordem de serviço por ID.
- `POST /api/v1/ordens-servico` — Abre uma nova OS com status `Aberta`, calculando o `valorTotal` no servidor.
- `PUT /api/v1/ordens-servico/{id}` — Atualiza valores ou status da ordem de serviço.
- `DELETE /api/v1/ordens-servico/{id}` — Exclui uma ordem de serviço (restrito a `admin`).

### Autenticação (`/api/v1/auth`)
- `POST /api/v1/auth/registrar` — Cria novo usuário com perfil (`admin` ou `mecanico`).
- `POST /api/v1/auth/login` — Faz login e retorna o token JWT.

## 4. Regra de negócio

A camada de serviço (`OrdemServicoService` e `VeiculoService`) vai aplicar as seguintes regras:
1. **Cálculo de orçamento no servidor:** O `valorTotal` é sempre calculado somando `valorPecas + valorMaoDeObra`, rejeitando valores negativos.
2. **Bloqueio de alteração em OS finalizada:** Ordens de serviço com status `Concluida` ou `Cancelada` não podem mais ser editadas.
3. **Validação de duplicidade e integridade:** A API não permite cadastrar placas duplicadas, só permite abrir OS para veículos existentes e impede a exclusão de veículos que possuam ordens de serviço vinculadas.

## 5. Casos de erro

1. **ID não encontrado (`404 Not Found`):** Ao tentar buscar, alterar ou deletar um veículo ou OS que não existe, ou ao tentar abrir uma OS com `veiculoId` inexistente.
2. **Dados inválidos ou violação de regra (`400 Bad Request`):** Ao tentar cadastrar placa repetida, enviar valores negativos em peças/mão de obra, tentar alterar uma OS já concluída ou tentar remover veículo com OS ativa.

## 6. Stack escolhida

- **Backend:** .NET 10 / ASP.NET Core Web API (C#)
- **Banco NoSQL:** MongoDB Atlas
- **Documentação:** OpenAPI / Swagger
- **Frontend:** HTML5, CSS3 e JavaScript assíncrono (fetch API)

## 7. Segurança e testes

- **Autenticação JWT (5.1):** Rotas de login e registro gerando token JWT com tempo de expiração; endpoints protegidos com `[Authorize]`.
- **Autorização RBAC (5.2):** Dois perfis (`admin` e `mecanico`). O mecânico pode consultar, criar e atualizar ordens de serviço; os endpoints de exclusão (`DELETE`) exigem perfil `admin`.
- **Testes Unitários (5.3):** Projeto com **xUnit** testando o `OrdemServicoService`:
  - 2 cenários de sucesso: cálculo correto de `valorTotal` na criação da OS e transição válida de status.
  - 2 cenários de erro: tentativa de criar OS com valores negativos e tentativa de editar OS já concluída.