# AAW - Aula 04: Auditoria de Apis REST

- Aluno: Pablo Ernane Oliveira Vieira
- RA: 202451086472
- Disciplina: Arquitetura de Aplicações Web
- Professor: Thalles Noce

---

## Tabela de Auditoria - PetHouse API

| #   | O que eu chamei                                                | O que a resposta mostrou                          | Regra REST violada                                                                          | Como eu redesenharia                                                             |
| --- | -------------------------------------------------------------- | ------------------------------------------------- | ------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------- |
| 01  | POST /api/v1/getPets                                           | 200 OK com lista de pets                          | Interface uniforme: leitura deve usar GET e rota deve ser substantivo no plural, sem verbos | GET /api/v1/pets                                                                 |
| 02  | GET /api/v1/deletarPet?id=7                                    | 200 OK e pet apagado no servidor                  | Metodos seguros: GET nao pode produzir efeito colateral; ID deve ir no path                 | DELETE /api/v1/pets/7 com retorno 204 No Content                                 |
| 03  | GET /api/v1/pet/1                                              | 200 OK com dados do pet                           | Nomenclatura consistente: colecoes devem usar substantivo no plural                         | GET /api/v1/pets/1                                                               |
| 04  | GET /api/v1/banhosTosa e /tutores_vip                          | 200 OK nas duas rotas                             | Padronizacao de nomes: usar kebab-case; estado/fidelidade vira parametro de busca           | GET /api/v1/banhos-e-tosas e GET /api/v1/tutores?vip=true                        |
| 05  | POST /api/v1/pets com body valido                              | 200 OK sem header Location                        | Status code semantico de criacao de recurso                                                 | POST /api/v1/pets respondendo 201 Created com header Location                    |
| 06  | GET /api/v1/pets/999999                                        | 200 OK informando que nao existe                  | Contrato semantico de erros: recurso ausente e erro 4xx do cliente                          | 404 Not Found com ProblemDetails (RFC 9457)                                      |
| 07  | GET /api/pets                                                  | 200 OK sem prefixo v1 e chave nomeDoPet           | Versionamento de API: breaking changes quebram clientes legados                             | Manter /api/v1/pets intacta e criar /api/v2/pets                                 |
| 08  | GET /api/v1/petshops/1/clientes/5/pets/9/consultas/12/exames/3 | 200 OK mesmo alterando os primeiros IDs           | Superaninhamento: limite maximo de 2 niveis; recurso proprio vira top-level                 | GET /api/v1/exames/3 ou GET /api/v1/consultas/12/exames                          |
| 09  | GET /api/v1/consultas                                          | 200 OK trazendo mais de 6.000 registros (~680 KB) | Colecao sem limites: ausencia de paginacao controlada no servidor                           | GET /api/v1/consultas?page=1&size=20 com envelope de metadados                   |
| 10  | PUT /api/v1/pets/12/vacinas                                    | 200 OK criando uma nova dose a cada envio         | Idempotencia do PUT violada: chamadas identicas acumulam registros                          | POST /api/v1/pets/12/vacinas (dose nova) ou PUT que substitui a carteira         |
| 11  | POST /api/v1/sessao e GET /api/v1/meus-pets                    | Login do usuario 2 altera o retorno do usuario 1  | Principio Stateless: o servidor guardou sessao na memoria                                   | Contexto na URI (GET /api/v1/tutores/1/pets) ou token no header Authorization    |
| 12  | GET /api/v1/tabela-de-precos                                   | 200 OK com cabecalho Cache-Control: no-store      | Cacheabilidade: dado estatico anual deve permitir cache                                     | Header Cache-Control: public, max-age=3600 com ETag e suporte a 304 Not Modified |
