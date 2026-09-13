# Atividade — AULA 06

## Síncrono ou Assíncrono?

_Análise de fluxos de comunicação entre serviços — Arquitetura de Aplicações Web_

## 🎯 MISSÃO

Vocês são os arquitetos dos 4 fluxos abaixo. Para CADA cenário:

- Decidam o estilo de comunicação: síncrono (request/response), assíncrono (fila/evento) ou API Gateway/BFF
- Desenhem o fluxo com caixas (serviços) e setas (chamadas/mensagens) no espaço indicado
- Justifiquem com pelo menos 2 fatores (urgência da resposta, tolerância a atraso, picos, falhas...)
- Apontem o principal risco da escolha de vocês

_⏱️ Tempo: 25 minutos | 👥 Formato: em duplas | Não existe resposta única — o que vale é a justificativa._

> **Nomes:** _Pablo Ernane Oliveira Vieira_ **Turma:** _GNP0547 - 3001_ **Data:** _12_ / _09_ / _2026_

## CENÁRIO 01 — PagFácil — aprovar ou negar AGORA

No checkout do PagFácil, ao clicar em “Pagar”, o serviço de Pagamentos precisa consultar o saldo/limite do cliente no serviço de Contas — e a resposta define se a venda acontece neste exato momento.

- O cliente está na tela, esperando o resultado da compra
- Sem a resposta de Contas, não há decisão possível: aprovar às cegas é proibido
- Tempo de resposta do serviço de Contas: ~80 ms em condições normais

**Sua análise:**

1. Estilo recomendado: [x] Síncrono ☐ Assíncrono (fila/evento) ☐ API Gateway/BFF

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

| Cliente chama POST /pagar no Serviço de Pagamentos |
| Pagamentos chama GET /saldo no Serviço de Contas |
| Contas responde: saldo OK ou recusado |
| Pagamentos responde ao Cliente: 200 OK ou 400 Bad Request |

3. Justificativa (mínimo 2 fatores):
   - Urgência da resposta: o cliente está esperando a decisão na tela, então a comunicação precisa ser imediata, não há como avançar sem saber se foi aprovado.
   - Tolerância a falhas: se o serviço de Contas estiver fora do ar, o Pagamentos não consegue decidir e a venda não pode acontecer. A comunicação síncrona deixa claro que a operação depende da disponibilidade do outro serviço.

4. Principal risco da escolha:
   - Se o serviço de Contas estiver fora do ar ou aumentar a latência, o serviço de Pagamentos trava junto e o checkout para de vender na hora.

## CENÁRIO 02 — CadastraJá — o e-mail de boas-vindas

Após criar a conta no CadastraJá, o sistema envia um e-mail de boas-vindas. O provedor de e-mail às vezes demora 8 segundos para responder e falha em 2% das tentativas.

- O usuário quer começar a usar o app imediatamente após o cadastro
- O e-mail chegar 1 minuto depois não incomoda ninguém
- Se o provedor falhar, o envio deve ser tentado de novo — sem o usuário perceber

**Sua análise:**

1. Estilo recomendado: ☐ Síncrono [x] Assíncrono (fila/evento) ☐ API Gateway/BFF

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

| Cliente chama POST /cadastro no Serviço de Contas |
| Contas cria o registro e responde 201 Created de imediato ao Cliente |
| Contas publica o evento "usuario-criado" na fila |
| Serviço de E-mail consome o evento e chama o provedor de e-mail |

3. Justificativa (mínimo 2 fatores):
   - Tolerância a atraso: alta. O enunciado deixa claro que um atraso de 1 minuto não afeta o usuário, então não faz sentido prender a tela de cadastro.
   - Tolerância a falhas: se o provedor de e-mail falhar, podemos reprocessar a fila sem impactar o usuário.

4. Principal risco da escolha:
   - Entrega duplicada: filas garantem entrega pelo menos uma vez, então o serviço de e-mail precisa ser idempotente para não enviar o mesmo e-mail duas vezes.

## CENÁRIO 03 — MegaMarket — baixa de estoque nos picos

No marketplace MegaMarket, cada venda gera uma baixa no serviço de Estoque. Nas grandes promoções o tráfego sobe 10x e o Estoque não dá conta de responder na velocidade das vendas.

- Atraso de alguns segundos na baixa é aceitável
- PERDER uma baixa de estoque não é aceitável (gera venda sem produto)
- O checkout não pode ficar lento nem cair porque o Estoque está sobrecarregado

**Sua análise:**

1. Estilo recomendado: ☐ Síncrono [x] Assíncrono (fila/evento) ☐ API Gateway/BFF

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

| Cliente chama POST /finalizar-compra no Serviço de Checkout |
| Checkout valida o pedido, publica o evento "venda-realizada" no broker persistente e responde 202 Accepted ao Cliente |
| A fila persistente absorve o pico de tráfego |
| Serviço de Estoque consome o evento no seu ritmo e atualiza o saldo do produto |

3. Justificativa (mínimo 2 fatores):
   - Picos de tráfego: durante promoções, o serviço de Estoque não consegue processar todas as requisições em tempo real. A fila permite absorver o pico e processar no ritmo do Estoque.
   - Tolerância a atraso: o enunciado diz que um atraso de alguns segundos na baixa é aceitável, então podemos desacoplar a venda da atualização do estoque sem prejudicar a experiência do usuário.

4. Principal risco da escolha:
   - Janela de inconsistência: se o serviço de Estoque falhar e não processar a fila a tempo, podemos ter vendas registradas sem a baixa correspondente no estoque, gerando inconsistência nos dados.

## CENÁRIO 04 — AppBanco — uma tela, cinco serviços

A tela inicial do AppBanco mostra saldo, fatura do cartão, investimentos, empréstimos e cashback — dados de 5 serviços diferentes. O time mobile reclama: são 5 chamadas, 5 formatos de resposta e 5 pontos de falha em cada abertura do app.

- A tela precisa abrir rápido, inclusive em redes móveis ruins
- Cada serviço tem equipe, formato e autenticação próprios
- Amanhã nasce a versão web, que precisa de MAIS dados que a mobile

**Sua análise:**

1. Estilo recomendado: ☐ Síncrono ☐ Assíncrono (fila/evento) [x] API Gateway/BFF

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

| App Mobile chama GET /home-mobile no BFF (uma única chamada consolidada) |
| BFF faz chamadas aos 5 serviços e agrega os dados |
| Cada serviço responde ao BFF no seu formato específico |
| BFF agrega os dados, filtra o que é relevante para celular e devolve um payload único otimizado |

3. Justificativa (mínimo 2 fatores):
   - Redes móveis ruins: fazer 5 chamadas separadas direto do celular multiplica latência de rede e chances de falha parcial;
   - Diferentes formatos de resposta: o BFF pode padronizar e otimizar o payload para cada tipo de cliente (mobile, web, etc.), evitando que cada app precise lidar com múltiplos formatos.

4. Principal risco da escolha:
   - Ponto único de falha: se o BFF cair, todos os clientes ficam sem acesso aos dados, mesmo que os serviços individuais estejam funcionando.

## DESAFIO

1. Escolha um cenário em que vocês indicaram ASSÍNCRONO. Os brokers de mensagens costumam garantir entrega “pelo menos uma vez” — ou seja, a MESMA mensagem pode chegar duas vezes. O que aconteceria no seu fluxo? Como o consumidor deveria se proteger?

   03 - MegaMarket (baixa de estoque)
   - Se a mensagem de evento "venda-criada" for processada duas vezes pelo serviço de Estoque, podemos acabar com uma baixa de estoque duplicada, resultando em inconsistência e possível venda de produto que não existe.
   - Para se proteger, o serviço de Estoque deve implementar idempotência, garantindo que a mesma operação (baixa de estoque para uma venda específica) não seja aplicada mais de uma vez. Isso pode ser feito, por exemplo, registrando o ID da venda processada e ignorando mensagens duplicadas com o mesmo ID.
