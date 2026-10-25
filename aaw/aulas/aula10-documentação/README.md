# HANDOUT — AULA 10

## Decifrando o Contrato

_Lendo uma especificação OpenAPI — Arquitetura de Aplicações Web_

## 🎯 MISSÃO

Vocês receberam o contrato OpenAPI de uma API que NUNCA viram — e nada mais: sem código, sem wiki, sem ninguém para perguntar. Provem que o contrato basta:

- Parte 1 — Interroguem a especificação YAML do Card 1 e respondam às perguntas do Card 2
- Parte 2 — Comparem com a “documentação em prosa” do Card 3 e registrem o que só a spec responde

_⏱️ Tempo: 30 minutos | 👥 Formato: em duplas | Só vale usar o que está impresso neste handout._

> **Nomes:** **Pablo Ernane******\_\_\_\_******** **Turma:** ********\_\_\_\_******** **Data:** \_24** / \_10** / **2026\_\_**

## CARD 01 — O contrato — especificação OpenAPI (YAML)

Este é TODO o material que o time parceiro enviou. Leiam com calma — cada linha responde a uma pergunta do Card 2.

```text
openapi: 3.1.0
info:
  title: API Biblioteca
  version: 1.0.0
servers:
  - url: https://api.biblioteca.newton.br
paths:
  /api/v1/livros:
    get:
      summary: Lista os livros (paginado)
      parameters:
        - name: page
          in: query
          schema: { type: integer, default: 1 }
      responses:
        "200": { description: Lista de livros }
    post:
      summary: Cadastra um livro
      security:
        - bearerAuth: []
      requestBody:
        required: true
        content:
          application/json:
            schema: { $ref: "#/components/schemas/NovoLivro" }
      responses:
        "201": { description: Livro criado }
        "400": { description: Campos inválidos (ProblemDetails) }
        "401": { description: Token ausente ou inválido }
components:
  securitySchemes:
    bearerAuth:
      type: http
      scheme: bearer
      bearerFormat: JWT
  schemas:
    NovoLivro:
      type: object
      required: [titulo, autor]
      properties:
        titulo: { type: string, maxLength: 120 }
        autor:  { type: string }
        ano:    { type: integer, minimum: 1450 }
```

## CARD 02 — Interrogatório do contrato

Respondam usando SOMENTE o Card 1:

1. Que endpoints/operações existem e o que cada um faz?

- GET /api/v1/livros: Lista os livros de forma paginada.
- POST /api/v1/livros: Cadastra um novo livro no acervo, e exige autenticação.

2. Quais campos são OBRIGATÓRIOS para cadastrar um livro? Que restrições os campos têm?

- Apenas titulo e autor são obrigatórios. O campo ano é opcional.
- As restrições são:
  - título: tipo texto (string), deve ter no máximo 120 caracteres.
  - autor: tipo texto (string)
  - ano: tipo inteiro (integer), com valor mínimo de 1450.

3. Em que situações a API devolve 400? E 401? Qual operação exige token — e de que tipo?

- Devolve 400 quando o cliente envia dados inválidos no corpo da requisição. Ex: um ano menor que 1450, um título maior que 120 caracteres, etc.
- Devolve 401 quando o token está ausente no cabeçalho ou é inválido.
- A operação que exige token é o POST /api/v1/livros.
- O tipo de token HTTP do tipo Bearer, com formatação JWT.

4. Montem a requisição POST completa no papel (método, URL, headers, body JSON válido):
   POST https://api.biblioteca.newton.br/api/v1/livros HTTP/1.1
   Host: api.biblioteca.newton.br
   Content-Type: application/json
   Authorization: Bearer <token_jwt>

{
"titulo": "Vivendo como um guerreiro",
"autor": "Whindersson Nunes",
"ano": 2021
}

## CARD 03 — A mesma API, documentada “em prosa”

Versão que o parceiro tinha antes de adotar OpenAPI:

_“Para cadastrar um livro, envie os dados do livro para o endpoint de livros. É preciso estar autenticado. Os campos incluem título, autor e ano. Em caso de erro, a API retorna uma mensagem apropriada. Também é possível listar os livros cadastrados.”_

1. Listem 3 informações que vocês usaram no Card 2 e que esta prosa NÃO responde com precisão:

- Campos obrigatórios e validações: A prosa diz que os campos incluem título, autor e ano, mas não especifica quais são obrigatórios, nem quais validações os campos possuem.

- Autenticação: A prosa diz que é preciso estar autenticado, mas não explica se é autenticação básica (usuário e senha) ou um token Bearer JWT.

- Tratamento de erros: A prosa diz que em caso de erro, a API retorna uma mensagem apropriada, mas não especifica quais são os códigos de erro.

## DESAFIO

1. Mesmo com a spec em mãos, que pergunta a dupla AINDA precisaria fazer ao time da API? (Dica: o contrato diz o que a API promete — não diz tudo sobre como ela se comporta.)

- Como obter o token JWT? A especificação exige autenticação Bearer JWT para cadastrar livros, mas não existe nenhum endpoint de login descrito. Onde e como o cliente deve obter o JWT?
