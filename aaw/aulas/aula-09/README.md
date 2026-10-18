# HANDOUT — AULA 09

## Projete a Matriz de Acesso

_SisAcad — sistema acadêmico fictício — Arquitetura de Aplicações Web_

## 🎯 MISSÃO

Vocês são o time de segurança contratado para definir QUEM pode fazer O QUÊ no SisAcad, o novo sistema acadêmico da faculdade. Os papéis já existem: Aluno, Professor, Coordenador e Secretaria.

- Preencham a matriz abaixo com ✓ (pode) ou ✗ (não pode) para cada papel × ação
- Justifiquem por escrito os 3 casos que geraram mais discussão na dupla
- Respondam à pergunta-armadilha da Parte 03 — ela esconde o limite do modelo

_⏱️ Tempo: 25 minutos | 👥 Formato: em duplas | Regra de ouro: na dúvida, NEGUE o acesso._

> **Nomes:** <u>Pablo Ernane e Cauã Rheris</u> **Turma:** _GNP0547 - 3001_ **Data:** _15_ / _10_ / _2026_

## PARTE 01 — A matriz papel × permissão

Marquem ✓ (pode) ou ✗ (não pode) em cada célula:

| **Ação**                          | **Aluno** | **Professor** | **Coordenador** | **Secretaria** |
| --------------------------------- | --------- | ------------- | --------------- | -------------- |
| 1. Ver as próprias notas          | ✓         | -             | -               | -              |
| 2. Ver notas de toda a turma      | ✗         | ✓             | ✓               | ✗              |
| 3. Lançar nota                    | ✗         | ✓             | ✗               | ✗              |
| 4. Alterar nota já lançada        | ✗         | ✓             | ✓               | ✗              |
| 5. Matricular aluno em disciplina | ✗         | ✗             | ✓               | ✓              |
| 6. Trancar matrícula              | ✗         | ✗             | ✗               | ✓              |
| 7. Criar nova disciplina          | ✗         | ✗             | ✓               | ✗              |
| 8. Ver dados pessoais de aluno    | ✓         | ✗             | ✓               | ✓              |
| 9. Emitir histórico escolar       | ✗         | ✗             | ✗               | ✓              |
| 10. Aprovar colação de grau       | ✗         | ✗             | ✓               | ✓              |

_Dica: pensem em consequências — quem responde perante o MEC se uma nota for alterada indevidamente? E perante a LGPD se dados pessoais vazarem?_

## PARTE 02 — Os casos polêmicos

Quais 3 células geraram mais discussão na dupla? Anotem a célula (ação + papel) e a justificativa da decisão final:

1. Caso 1: Alterar nota já lançada
   - R: No papel do coordenador, discutimos se deveria ter essa função. Concluímos que deveria, pois ele pode realizar essa ação em casos como, em período de fechamento de semestre, onde o professor já realizou o lançamento das notas e não possui permissão para modificar, o coordenador poderá com o alinhamento feito com o professor, alterar a nota do aluno em questão.

2. Caso 2: Matricular aluno em disciplina
   - R: No papel da Secretaria, discutimos se deveria ter essa função. Concluímos que deveria, pois ela deve fazer a gestão da grade de calouros, transferências.

3. Caso 3: Aprovar colação de grau
   - R: No papel do Coordenador, discutimos se deveria ter essa função. Concluímos que deveria, pois ele é o responsável por aprovar a colação de grau dos alunos, garantindo que todos os requisitos acadêmicos foram cumpridos antes da aprovação final.

## PARTE 03 — A pergunta-armadilha

Vocês marcaram que Professor pode “lançar nota”. Mas… de QUALQUER turma? O professor de Cálculo pode lançar nota na turma de Arquitetura de Aplicações Web?

1. O papel “Professor” sozinho é suficiente para decidir esse acesso? Por quê?
   - R: Não, o papel “Professor” sozinho não é suficiente para decidir esse acesso, pois ele não leva em consideração a relação do professor com a turma específica. Um professor pode ter permissão para lançar notas apenas nas turmas que ele ministra, e não em qualquer turma da instituição. Portanto, é necessário verificar se o professor está associado à disciplina e à turma em questão antes de permitir o lançamento de notas.

2. Que informação ALÉM do papel o sistema precisa conhecer para decidir corretamente?
   - R: O sistema precisa conhecer a relação entre o professor e a turma específica, ou seja, se o professor está realmente ministrando aquela disciplina para aquela turma. Isso pode ser feito através de um mapeamento de turmas e disciplinas associadas aos professores.

3. Onde vocês já viram esse mesmo problema nesta disciplina? (dica: Aula 07…)
   - R: Já vimos esse mesmo problema na Aula 07, quando discutimos sobre a autorização baseada em papéis e como ela pode ser insuficiente para determinar o acesso a recursos específicos. Naquela aula, aprendemos que além do papel do usuário, é necessário considerar o contexto e as relações específicas para garantir que o acesso seja concedido de forma adequada e segura.

## DESAFIO

1. Chegou um pedido da coordenação: criar o papel “Monitor” — um aluno que ajuda o professor em uma disciplina. Que permissões vocês dariam ao Monitor? Que risco novo esse papel cria?
   - R: O papel “Monitor” poderia ter permissões limitadas, como visualizar as notas da turma, a grade da disciplina e situações acadêmicas dos alunos, mas não poderia lançar ou alterar notas. O risco novo que esse papel cria é a possibilidade de vazamento de informações acadêmicas, já que o monitor teria acesso a dados sensíveis dos alunos.
