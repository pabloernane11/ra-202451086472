# HANDOUT — AULA 07

## Caça às Vulnerabilidades

_Revisão de segurança de uma API .NET — Arquitetura de Aplicações Web_

## 🎯 MISSÃO

Vocês são a dupla de revisores de segurança da empresa. Os 4 trechos abaixo são da MESMA API, prestes a ir para produção. Para CADA card:

- Descrevam a falha com as próprias palavras (não precisa do nome técnico ainda)
- Estimem o dano possível se isso chegar à produção
- Proponham a correção

_⏱️ Tempo: 30 minutos | 👥 Formato: em duplas | Todo o código é fictício e roda apenas no laboratório._

> **Nomes:** _Pablo Ernane Oliveira Vieira_ **Turma:** _GNP0547 - 3001_ **Data:** _17\_\_ / \_09_ / _2026_

## VULNERABILIDADE 01 — A busca de clientes

> `GET /api/clientes/buscar?nome=...`

Endpoint de busca usado pela tela de atendimento. O parâmetro nome vem direto da caixa de busca do site.

```text
 1  [HttpGet("buscar")]
 2  public IActionResult Buscar(string nome)
 3  {
 4      var sql = "SELECT * FROM Clientes WHERE Nome = '"
 5                + nome + "'";
 6      var clientes = _db.Clientes.FromSqlRaw(sql).ToList();
 7      return Ok(clientes);
 8  }
```

**Sua análise:**

1. Qual é a falha?

- Injeção de SQL. O parâmetro _nome_ é concatenado diretamente na instrução SQL com o operador `+`, então o banco não sabe separar o que é comando do que é dado. Tudo que o usuário digita acaba sendo executado como se fosse SQL.

2. Qual o dano possível em produção?

- Dá pra um atacante mandar algo como %' OR '1'='1 e o filtro deixa de funcionar, retornando a tabela inteira. Isso é vazamento de dados pessoais, ou seja, problema claro com a LGPD. Dependendo do banco e das permissões, ainda dá pra deletar ou alterar dados, o que é bem pior.

3. Como corrigir?

- Usar consulta parametrizada, que separa o comando do dado. Com LINQ fica assim:

```text
 1  [HttpGet("buscar")]
 2  public IActionResult Buscar(string nome)
 3  {
 4      var resultado = _db.Clientes
 5          .Where(c => c.Nome.Contains(nome))
 6          .ToList();
 7      return Ok(resultado);
 8  }
```

## VULNERABILIDADE 02 — A consulta de faturas

> `GET /api/faturas/{id}`

Endpoint usado pelo app para exibir a fatura do cartão. O usuário está autenticado quando chama esta rota.

```text
 1  [HttpGet("{id}")]
 2  public IActionResult GetFatura(int id)
 3  {
 4      var fatura = _db.Faturas.Find(id);
 5      if (fatura == null) return NotFound();
 6      return Ok(fatura);
 7  }
```

**Sua análise:**

1. Qual é a falha?

- Broken Access Control. O endpoint exige login, mas não confere se o recurso do {id} pertence ao usuário logado. A autenticação existe, a autorização não.

2. Qual o dano possível em produção?

- Vazamento horizontal de dados entre os usuários. Qualquer usuário logado consegue ver a fatura de qualquer outro usuário, bastando adivinhar o id da fatura. Como envolve dados pessoais, é problema com a LGPD.

3. Como corrigir?

- Comparar o UsuarioId da fatura com o usuário extraído do token JWT:

```text
1 [Authorize]
2 [HttpGet("{id}")]
3 public IActionResult GetFatura(int id)
4 {
5     var fatura = _db.Faturas.Find(id);
6     if (fatura is null) return NotFound();
7     if (fatura.UsuarioId != UsuarioLogadoId)
8        return Forbid();
9     return Ok(fatura);
10 }
```

## VULNERABILIDADE 03 — A configuração do servidor

> `Program.cs (roda igual em dev e em produção)`

Trecho de inicialização da API, idêntico em todos os ambientes. Este arquivo está versionado no Git da empresa.

```text
 1  public const string Conn =
 2      "Server=prod-db;Database=Banco;User=sa;" +
 3      "Password=Newton@2026!";
 4
 5  var app = WebApplication.CreateBuilder(args).Build();
 6  app.UseDeveloperExceptionPage();
 7  app.Run();
```

**Sua análise:**

1. Qual é a falha?

- Security Misconfiguration e Mishandling of Exceptional Conditions. A senha do banco está fixa no código, o usuário é o 'sa' (privilégio máximo) e o arquivo está versionado no Git. Além disso, o UseDeveloperExceptionPage() está ligado o tempo todo, inclusive em produção.

2. Qual o dano possível em produção?

- Quem tiver leitura no repositório (ou no histórico dele) já tem acesso total ao banco de produção. E qualquer erro em produção devolve stack trace, caminhos de arquivo, versões de pacote e rotas internas pra qualquer pessoa, o que ajuda muito no reconhecimento de um ataque.

3. Como corrigir?

- Movendo segredos para arquivos de configuração e variáveis de ambiente, e deixar a página detalhada só em desenvolvimento:

```text
 1  var connectionString = builder.Configuration.GetConnectionString("Default")!;
 2  var jwtKey = builder.Configuration["Jwt:Key"]
 3      ?? throw new InvalidOperationException("Jwt:Key não configurada.");
 4  if (app.Environment.IsDevelopment())
 5  {
 6     app.UseDeveloperExceptionPage();
 7  }
 8  else
 9  {
 10    app.UseExceptionHandler("/erro");
 11 }
 12
 13 app.Map("/erro", () => Results.Problem("Ocorreu um erro interno."));
```

## VULNERABILIDADE 04 — A atualização de perfil

> `PUT /api/usuarios/{id}`

Endpoint que o app chama quando o usuário edita o próprio perfil. O corpo da requisição é o JSON enviado pelo cliente.

```text
 1  public class UsuarioUpdate
 2  {
 3      public string Nome  { get; set; }
 4      public string Email { get; set; }
 5      public string Role  { get; set; }   // "user" | "admin"
 6  }
 7
 8  [HttpPut("{id}")]
 9  public IActionResult Atualizar(int id, UsuarioUpdate dto)
10  {
11      _repo.AtualizarTudo(id, dto);
12      return NoContent();
13  }
```

**Sua análise:**

1. Qual é a falha?

- Mass Assignment. O modelo de entrada expõe a propriedade de autorização "Role", permitindo que o model binder receba campos restritos enviados pelo cliente e o repositório sobrescreva essas permissões diretamente no banco.

2. Qual o dano possível em produção?

- Escalação horizontal e vertical de privilégios. Um usuário comum pode injetar "role": "Admin" no payload do Postman e autopromover sua conta a administrador com acesso irrestrito ao sistema.

3. Como corrigir?

- Utilizar um DTO só com os campos que o usuário pode alterar, ignorando ou rejeitando qualquer campo sensível.

```text
 1  public record AtualizarUsuarioRequest(string Nome, string Email);
 2
 3  [Authorize]
 4  [HttpPut("{id}")]
 5  public IActionResult Atualizar(int id, [FromBody] AtualizarUsuarioRequest dados)
 6  {
 7      var usuario = _db.Usuarios.Find(id);
 8      if (usuario is null) return NotFound();
 9
 10     if (usuario.Id != UsuarioLogadoId)
 11         return Forbid();
 12
 13     usuario.Nome = dados.Nome;
 14     usuario.Email = dados.Email;
 15     // Role não é alterada pelo cliente
 16
 17     _db.SaveChanges();
 18     return Ok(new { usuario.Id, usuario.Nome, usuario.Email, usuario.Role });
 19 }
```

## DESAFIO

1. Qual das 4 falhas um scanner automático de código teria MAIS dificuldade de encontrar? Por quê?

- Resposta: A Vulnerabilidade 02 (Broken Access Control / IDOR).
- Porque scanners estáticos pegam concatenação de texto em SQL, senhas soltas no código e middlewares ligados por pura análise de texto e sintaxe. Já o IDOR é um erro de regra de negócio. A sintaxe está certa, a linha com Find(id) compila sem aviso e a requisição está autenticada. Pra saber que o pedido é do João e não da Maria, a ferramenta precisaria conhecer a regra de domínio, e ela não conhece.
