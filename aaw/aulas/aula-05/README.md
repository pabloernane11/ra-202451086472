# HANDOUT — AULA 05

## Escolha o Banco

*Persistência em arquiteturas distribuídas — Arquitetura de Aplicações Web*

## 🎯 MISSÃO

Vocês são o time de arquitetura de dados contratado pelas 4 empresas abaixo. Para CADA cenário:

- Escolham o modelo de banco: relacional, documento, chave-valor ou grafo
- Justifiquem com pelo menos 2 fatores do contexto (estrutura dos dados, padrão de acesso, escala, consistência...)
- Apontem o principal risco da escolha de vocês

*⏱️ Tempo: 25 minutos  |  👥 Formato: em duplas  |  Não existe resposta única — o que vale é a justificativa.*

> **Nomes:** _Cauã Ribeiro e Pablo Ernane_  **Turma:** _GNP0547 - 3001_   **Data:** _03_ / _09_ / _2026_

## CENÁRIO 01 — TechStore — o catálogo camaleão

E-commerce com 80 mil produtos. Cada categoria tem atributos completamente diferentes: livro tem autor e número de páginas; notebook tem RAM e CPU; camiseta tem tamanho e cor.

- A cada categoria nova, o time faz ALTER TABLE e a tabela produtos já tem 92 colunas (a maioria NULL)
- O produto é quase sempre lido INTEIRO, de uma vez, para montar a página
- Novos atributos surgem toda semana — o marketing não espera o DBA
- Relatórios cruzando categorias são raros

**Sua análise:**

1. Modelo recomendado:   ☐ Relacional     [X] Documento     ☐ Chave-valor     ☐ Grafo

2. Justificativa (mínimo 2 fatores do contexto):

    2.1 - Pois cada produto vai ter seu doc. Json com as suas características necessárias;

    2.2 - Flexibilização ma estrutura dos dados.

3. Principal risco da escolha:

    A não convergencia de dados imediatamente, pois o banco de dados não é relacional e sim distribuído.

## CENÁRIO 02 — MegaCart — o carrinho da Black Friday

Serviço de carrinho de compras de um varejista gigante. Na Black Friday são milhões de leituras e escritas por minuto.

- O acesso é SEMPRE pela chave: “carrinho do cliente 12345” — nunca por busca ou filtro
- Todo carrinho expira automaticamente em 48h (TTL)
- Latência precisa ser de poucos milissegundos
- Perder um carrinho é chato, mas NÃO é tragédia — o cliente remonta

**Sua análise:**

1. Modelo recomendado:   ☐ Relacional     ☐ Documento     [X] Chave-valor     ☐ Grafo

2. Justificativa (mínimo 2 fatores do contexto):

    2.1 - Pois o acesso é sempre pela chave, e não por busca ou filtro;

    2.2 - Pois é mais rápido do que as buscas por relacionamento.

3. Principal risco da escolha:

    Trabalhar com dados desatualizados.

## CENÁRIO 03 — PayBank — dinheiro não pode evaporar

Módulo de transferências de um banco. Uma transferência debita uma conta e credita outra — as duas operações têm que acontecer JUNTAS ou nenhuma acontece.

- Consistência forte exigida por lei — saldo errado é multa do Banco Central
- Auditoria cruza contas, clientes, agências e transações em relatórios complexos (joins)
- O esquema dos dados é estável há 10 anos
- Volume alto, mas previsível

**Sua análise:**

1. Modelo recomendado:   [X] Relacional     ☐ Documento     ☐ Chave-valor     ☐ Grafo

2. Justificativa (mínimo 2 fatores do contexto):

    2.1 - Pois os pilares ACID, protegem a acuracidade dos dados;

    2-2 - E também, pois o esquema dos dados é estável há 10 anos, e não há necessidade de flexibilidade.

3. Principal risco da escolha:

    Escala vertical, com o tempo traz limitações, e hardware e software proprietários, ficando dependentes de serviçoes de terceiros.

## CENÁRIO 04 — FriendLink — amigos dos seus amigos

Rede social profissional em que o produto principal é a indicação: “pessoas que você talvez conheça” e “quem pode te apresentar à empresa X”.

- As consultas dominantes percorrem RELACIONAMENTOS: amigos dos amigos, caminhos de indicação com até 6 níveis
- Em banco relacional, cada nível vira um self-join — com 6 níveis a consulta já não responde
- Os dados de perfil são simples; o valor está nas CONEXÕES
- O grafo cresce milhões de arestas por dia

**Sua análise:**

1. Modelo recomendado:   ☐ Relacional     ☐ Documento     ☐ Chave-valor     [X] Grafo

2. Justificativa (mínimo 2 fatores do contexto):

    2.1 - Pois o valor está nas conexões, e não nos dados de perfil;

    2.2 - Pois o grafo cresce milhões de arestas por dia, e o banco de dados relacional não consegue lidar com isso.

3. Principal risco da escolha:

    Pois esse tipo de banco possui fraudes de segurança, e não é tão seguro quanto um banco relacional.
## DESAFIO

1. Escolha um dos cenários e responda: se a rede particionar (metade dos servidores não enxerga a outra metade), o que o sistema deve fazer — parar de responder para não errar, ou continuar respondendo mesmo arriscando dados desatualizados? Qual letra do CAP vocês sacrificariam e por quê?

    Depende de qual tipo de banco de dados você está utilizando, se for um banco relacional, o sistema deve parar de responder para não errar, sacrificando a disponibilidade (A) do CAP. Se for um banco de dados NoSQL, o sistema deve continuar respondendo mesmo arriscando dados desatualizados, sacrificando a consistência (C) do CAP.