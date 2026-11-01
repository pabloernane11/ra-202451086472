### Tabela de casos do handout

| subtotal | desconto | frete | total | observação |
|---|---|---|---|---|
| 100,00 | 0,00 | 25,00 | 125,00 | base |
| 199,99 | 0,00 | 25,00 | 224,99 | borda < 200 |
| 200,00 | 10,00 | 25,00 | 215,00 | borda 5% (inclusive) |
| 299,99 | 15,00 | 25,00 | 309,99 | borda < 300 (com frete) |
| 300,00 | 15,00 | 0,00 | 285,00 | borda frete grátis (inclusive) |
| 499,99 | 25,00 | 0,00 | 474,99 | borda < 500 (5%) |
| 500,00 | 50,00 | 0,00 | 450,00 | borda 10% (revelou o bug) |
| 600,00 | 60,00 | 0,00 | 540,00 | acima de 500 |

### Exit ticket (Bug revelado)
- **Caso:** `[InlineData(500, 450)]`
- **Causa:** `PedidoService.cs` linha 31 estava `subtotal > 500m` (deveria ser `>= 500m`).
- **Erro:** cobrava 475,00 (5%) em vez de 450,00 (10%).