# HANDOUT — AULA 02

## Dissecando o HTTP

_6 requisições sob o microscópio — Arquitetura de Aplicações Web_

## 🎯 MISSÃO

Vocês interceptaram 6 conversas entre um app e a API de uma biblioteca. Para CADA card:

- Descrevam o que o cliente pediu (verbo + recurso na URI)
- Expliquem o que o status code da resposta informa
- Respondam: repetindo a MESMA requisição 3 vezes seguidas, o estado do servidor muda?

Ao final, preencham juntos a TABELA-SÍNTESE dos verbos na última página.

_⏱️ Tempo: 30 minutos | 👥 Formato: em duplas | Dica: o card 6 esconde uma pegadinha de quem é a culpa._

> **Nomes:** _Pablo Ernane Oliveira Vieira_ **Turma:** _GNP0547 - 3001_ **Data:** _07_ / _09_ / _2026_

## REQUISIÇÃO 01 — A prateleira inteira

```text
→ REQUISIÇÃO
GET /api/livros HTTP/1.1
Host: biblioteca.newton.br
Accept: application/json
```

```text
← RESPOSTA
HTTP/1.1 200 OK
Content-Type: application/json

[ { "id": 1, "titulo": "Clean Code", "autor": "Robert C. Martin" },
  { "id": 7, "titulo": "O Programador Pragmático", "autor": "Hunt & Thomas" } ]
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?
   Verbo: GET (ler/consultar dados).
   Recurso: /api/livros

2. O que o status code informa? Deu certo? Culpa de quem se não deu?
   O status 200 OK informa que a requisição HTTP foi recebida, compreendida e processada com sucesso pelo servidor. Sim, deu certo e não houve erro.

3. Repetindo esta requisição 3 vezes seguidas, o estado do servidor muda? E a resposta?
   Não. O método GET é estritamente seguro, atuando apenas em modo de leitura, não altera dados, não adiciona e não remove nada da memória ou do banco. A resposta permanece a mesma.

## REQUISIÇÃO 02 — O livro fantasma

```text
→ REQUISIÇÃO
GET /api/livros/99 HTTP/1.1
Host: biblioteca.newton.br
Accept: application/json
```

```text
← RESPOSTA
HTTP/1.1 404 Not Found
Content-Type: application/problem+json

{ "title": "Not Found", "status": 404 }
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?
   Verbo: GET (ler/consultar dados).
   Recurso: /api/livros/99

2. O que o status code informa? Deu certo? Culpa de quem se não deu?
   O status 404 Not Found informa que a requisição foi recebida e processada pelo servidor, mas o recurso solicitado não existe na base de dados/memória.
   Não deu certo, houve uma falha na localização do recurso.
   A culpa é do cliente, códigos da família 4xx indicam erro por parte do cliente, pois ele solicitou um identificador inexistente ou inválido.

3. Repetindo esta requisição 3 vezes seguidas, o estado do servidor muda? E a resposta?
   Não. O método GET é estritamente seguro.
   A resposta permanece exatamente a mesma, desde que ninguém insira o livro 99 nesse intervalo.

## REQUISIÇÃO 03 — Livro novo na estante

```text
→ REQUISIÇÃO
POST /api/livros HTTP/1.1
Host: biblioteca.newton.br
Content-Type: application/json

{ "titulo": "Domain-Driven Design", "autor": "Eric Evans" }
```

```text
← RESPOSTA
HTTP/1.1 201 Created
Location: /api/livros/8
Content-Type: application/json

{ "id": 8, "titulo": "Domain-Driven Design", "autor": "Eric Evans" }
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?
   Verbo: POST (criar novo recurso).
   Recurso: /api/livros

2. O que o status code informa? Deu certo? Culpa de quem se não deu?
   O status 201 Created informa que a requisição foi processada com sucesso e um novo recurso foi criado no servidor. Sim, deu certo.

3. Enviando este POST 3 vezes seguidas, o que acontece na estante? Para que serve o header Location?
   Cada envio cria um novo livro na estante, com um ID único. O cabeçalho Location serve para indicar a URL exata onde o novo recurso recém-criado pode ser acessado diretamente pelo cliente (/api/livros/8).

## REQUISIÇÃO 04 — Corrigindo a ficha completa

```text
→ REQUISIÇÃO
PUT /api/livros/7 HTTP/1.1
Host: biblioteca.newton.br
Content-Type: application/json

{ "id": 7, "titulo": "O Programador Pragmático", "autor": "D. Hunt; D. Thomas" }
```

```text
← RESPOSTA
HTTP/1.1 200 OK
Content-Type: application/json

{ "id": 7, "titulo": "O Programador Pragmático", "autor": "D. Hunt; D. Thomas" }
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?
   Verbo: PUT (atualizar recurso existente).
   Recurso: /api/livros/7

2. O que o status code informa? Deu certo? Culpa de quem se não deu?
   O status 200 OK informa que a requisição foi processada com sucesso e o recurso foi atualizado no servidor. Sim, deu certo.

3. Repetindo esta requisição 3 vezes seguidas, o estado do servidor muda? E a resposta?
   Não. O método PUT é idempotente.
   A resposta permanece exatamente a mesma, desde que ninguém altere o livro 7 nesse intervalo.

## REQUISIÇÃO 05 — Fora do catálogo

```text
→ REQUISIÇÃO
DELETE /api/livros/7 HTTP/1.1
Host: biblioteca.newton.br
```

```text
← RESPOSTA
HTTP/1.1 204 No Content
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?
   Verbo: DELETE (remover recurso existente).
   Recurso: /api/livros/7

2. O que o status code informa? Deu certo? Culpa de quem se não deu?
   O status 204 No Content informa que a requisição foi processada com sucesso e o recurso foi removido do servidor. Sim, deu certo.

3. Repetindo o DELETE, o estado do servidor muda? Que resposta você ESPERA na segunda vez?
   O estado do servidor NÃO muda, pois o livro 7 continua não existindo. No entanto, a resposta esperada na segunda chamada é 404 Not Found, porque o recurso já foi excluído na primeira chamada e não é mais encontrado no catálogo.

## REQUISIÇÃO 06 — O cadastro capenga

```text
→ REQUISIÇÃO
POST /api/livros HTTP/1.1
Host: biblioteca.newton.br
Content-Type: application/json

{ "autor": "Anônimo" }
```

```text
← RESPOSTA
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json

{ "title": "Bad Request", "status": 400,
  "errors": { "Titulo": [ "O campo Titulo é obrigatório" ] } }
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?
   Verbo: POST (criar novo recurso).
   Recurso: /api/livros

2. O que o status code informa? Deu certo? Culpa de quem se não deu?
   O status 400 Bad Request informa que o servidor não pôde processar a requisição devido a uma violação de validação ou payload malformado. Não deu certo. A culpa é do cliente, pois o corpo da requisição foi enviado sem o campo obrigatório Titulo.

3. Repetindo esta requisição 3 vezes seguidas, o estado do servidor muda? E a resposta?
   O estado do servidor NÃO muda, pois a requisição não foi processada com sucesso. A resposta permanece a mesma, retornando 400 Bad Request, até que o cliente envie uma requisição válida.

## TABELA-SÍNTESE — Os verbos do HTTP

_Preencham com base nos 6 cards. “Seguro” = não altera nada no servidor. “Idempotente” = repetir N vezes deixa o servidor no mesmo estado que 1 vez._

| **Verbo**    | **Para que serve** | **Seguro?** | **Idempotente?** | **Status típicos** |
| ------------ | ------------------ | ----------- | ---------------- | ------------------ |
| **`GET`**    | Ler Recurso(s)     | Sim         | Sim              | 200 OK             |
| **`POST`**   | Criar Recurso      | Não         | Não              | 201 Created        |
| **`PUT`**    | Atualizar Recurso  | Não         | Sim              | 200 OK             |
| **`PATCH`**  | Atualizar Parcial  | Não         | Depende          | 200 OK             |
| **`DELETE`** | Remover Recurso    | Não         | Sim              | 204 No Content     |

## DESAFIO

1. O verbo PATCH não apareceu em nenhum card. Qual a diferença entre PATCH e PUT? Um app de banco quer alterar SÓ o apelido do usuário, entre dezenas de campos do perfil — qual dos dois você usaria e por quê?
   A diferença fundamental está no escopo da alteração: o PUT substitui o recurso inteiro, enquanto o PATCH aplica alterações parciais. Para alterar apenas o apelido do usuário, eu usaria PATCH, pois ele permite atualizar apenas o campo necessário sem afetar os demais dados do perfil.
