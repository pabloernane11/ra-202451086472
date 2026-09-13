using System.Text;
using System.Text.Json;
using LojaDev.Compartilhado.Fila;
using LojaDev.Compartilhado.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace LojaDev.LojaApi.Controllers;

/// <summary>
/// Controller principal da LojaApi.
///
/// Dois endpoints para o MESMO fluxo de pedido, mas com estratégias
/// de comunicação diferentes:
///   - POST /api/pedidos/sincrono   → tudo síncrono (HTTP request/response)
///   - POST /api/pedidos/assincrono → pagamento síncrono + notificação assíncrona (fila)
///
/// ══════════════════════════════════════════════════════════════
///  ATENÇÃO: Complete os TODOs 1 a 5 neste arquivo.
///  Dica: use os comentários como guia — cada TODO tem uma instrução.
/// ══════════════════════════════════════════════════════════════
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly FilaDePedidos _fila;

    public PedidosController(
        IHttpClientFactory httpFactory,
        IConfiguration config,
        FilaDePedidos fila)
    {
        _httpFactory = httpFactory;
        _config = config;
        _fila = fila;
    }

    // ╔══════════════════════════════════════════════════════════╗
    // ║  FLUXO 1: TOTALMENTE SÍNCRONO                          ║
    // ║  LojaApi → PagamentoApi (HTTP) → NotificacaoApi (HTTP)  ║
    // ║  O cliente (Postman) espera TUDO terminar.              ║
    // ╚══════════════════════════════════════════════════════════╝

    /// <summary>
    /// POST /api/pedidos/sincrono
    /// Fluxo síncrono: chama PagamentoApi, depois NotificacaoApi,
    /// e só então retorna a resposta ao cliente.
    /// </summary>
    [HttpPost("sincrono")]
    public async Task<IActionResult> CriarPedidoSincrono([FromBody] Pedido pedido)
    {
        var inicioTotal = DateTime.Now;
        Console.WriteLine($"\n🛒 ═══ FLUXO SÍNCRONO — Pedido {pedido.Id} ═══");

        // ── Passo 1: Chamar PagamentoApi via HTTP (síncrono) ──────────
        //
        // Precisamos da resposta do pagamento ANTES de continuar.
        // Sem saber se foi aprovado, não podemos prosseguir.
        //
        // Leia a URL do serviço de pagamento em _config["ServicoPagamento"].
        // O endpoint é: POST {url}/api/pagamentos
        // O corpo (body) deve ser o objeto 'pedido' serializado como JSON.

        // ╔═══════════════════════════════════════════════════════╗
        // ║  TODO 1: Fazer chamada HTTP POST para PagamentoApi   ║
        // ║                                                      ║
        // ║  Passos:                                             ║
        // ║  1. Crie um HttpClient com _httpFactory               ║
        // ║  2. Serialize o 'pedido' para JSON (StringContent)   ║
        // ║  3. Faça POST para "{urlPagamento}/api/pagamentos"   ║
        // ║  4. Leia a resposta como string                      ║
        // ║  5. Desserialize para pegar o campo "aprovado"       ║
        // ╚═══════════════════════════════════════════════════════╝

        var urlPagamento = _config["ServicoPagamento"];
        var clientPagamento = _httpFactory.CreateClient();
        var jsonPagamento = _httpFactory.Serialize(pedido);
        var conteudoPagamento = new StringContent(json, Encoding.UTF8, "application/json");
        var respostaPagamento = await client.PostAsync($"{urlPagamento}/api/pagamentos", conteudoPagamento);
        var corpoPagamento = await resposta.Content.ReadAsStringAsync();
        var resultadoPagamento = JsonSerializer.Deserialize<JsonElement>(corpoPagamento);
        bool aprovado = resultadoPagamento.GetProperty("aprovado").GetBoolean();

        if (!aprovado)
        {
            var tempoRejeicao = (DateTime.Now - inicioTotal).TotalMilliseconds;
            Console.WriteLine($"🛒 Pedido REJEITADO em {tempoRejeicao:F0}ms");
            return BadRequest(new
            {
                pedidoId = pedido.Id,
                status = "rejeitado",
                mensagem = "Pagamento não aprovado",
                tempoTotalMs = tempoRejeicao
            });
        }

        // ── Passo 2: Chamar NotificacaoApi via HTTP (síncrono) ────────
        //
        // AQUI está o problema do fluxo síncrono: precisamos ESPERAR
        // o serviço de notificação (que é LENTO ~3s e INSTÁVEL ~20% falha)
        // antes de devolver a resposta ao cliente.

        // ╔═══════════════════════════════════════════════════════╗
        // ║  TODO 2: Fazer chamada HTTP POST para NotificacaoApi  ║
        // ║                                                      ║
        // ║  Passos:                                             ║
        // ║  1. Leia a URL em _config["ServicoNotificacao"]      ║
        // ║  2. Monte o body com: Destinatario, Assunto, Corpo   ║
        // ║  3. Faça POST para "{url}/api/notificacoes"          ║
        // ║  4. Guarde o status (sucesso/falha) para retornar    ║
        // ╚═══════════════════════════════════════════════════════╝

        var urlNotificacao = _config["ServicoNotificacao"];
        var clienteNotifcacao = _httpFactory.CreateClient();
        var bodyNotificacao = new 
        {
            destinatario = $"{pedido.Cliente.ToLower().Replace(" ", ".")}@email.com",
            assunto = $"Pedido {pedido.Id} confirmado!",
            corpo = $"Olá {pedido.Cliente}, seu pedido de {pedido.Produto} no valor de R$ {pedido.Valor:N2} foi aprovado."
        };
        var jsonNotif = JsonSerializer.Serialize(bodyNotificacao);
        var conteudoNotif = new StringContent(jsonNotif, Encoding.UTF8, "application/json");
        
        string statusNotificacao;
        try
        {
            var respostaNotif = await clienteNotifcacao.PostAsync($"{urlNotificacao}/api/notificacoes", conteudoNotif);
            statusNotificacao = respostaNotif.IsSucessStatusCode ? "enviado com sucesso" : $"falha HTTP {(int)respostaNotif.StatusCode}";
        }
        catch (Exception ex)
        {
            statusNotificacao = $"erro de conexao: {ex.Message}";
        }

        var tempoTotal = (DateTime.Now - inicioTotal).TotalMilliseconds;
        Console.WriteLine($"🛒 ═══ SÍNCRONO concluído em {tempoTotal:F0}ms ═══\n");

        return Ok(new
        {
            pedidoId = pedido.Id,
            status = "aprovado",
            notificacao = statusNotificacao,
            fluxo = "sincrono",
            tempoTotalMs = tempoTotal,
            observacao = "⏱️ Note o tempo total — inclui a espera da notificação!"
        });
    }

    // ╔══════════════════════════════════════════════════════════╗
    // ║  FLUXO 2: PAGAMENTO SÍNCRONO + NOTIFICAÇÃO ASSÍNCRONA  ║
    // ║  LojaApi → PagamentoApi (HTTP) → Fila → [background]   ║
    // ║  O cliente (Postman) NÃO espera a notificação.          ║
    // ╚══════════════════════════════════════════════════════════╝

    /// <summary>
    /// POST /api/pedidos/assincrono
    /// Fluxo misto: pagamento síncrono (precisa da resposta),
    /// mas notificação é publicada na fila e processada em background.
    /// </summary>
    [HttpPost("assincrono")]
    public async Task<IActionResult> CriarPedidoAssincrono([FromBody] Pedido pedido)
    {
        var inicioTotal = DateTime.Now;
        Console.WriteLine($"\n🛒 ═══ FLUXO ASSÍNCRONO — Pedido {pedido.Id} ═══");

        // ── Passo 1: Chamar PagamentoApi (síncrono — mesma lógica) ───
        //
        // O pagamento CONTINUA síncrono porque precisamos da resposta
        // antes de confirmar o pedido. Sem saber se foi aprovado,
        // não podemos confirmar a compra.

        // ╔═══════════════════════════════════════════════════════╗
        // ║  TODO 3: Chamar PagamentoApi (igual ao TODO 1)       ║
        // ║                                                      ║
        // ║  Copie a mesma lógica do TODO 1 aqui.                ║
        // ║  Pergunta: por que o pagamento continua síncrono?    ║
        // ╚═══════════════════════════════════════════════════════╝

        var urlPagamento = _config["ServicoPagamento"];
        var clientPagamento = _httpFactory.CreateClient();
        
        var respostaPagamento = await client.PostAsync($"{urlPagamento}/api/pagamentos", conteudoPagamento);
        var corpoPagamento = await resposta.Content.ReadAsStringAsync();
        var resultadoPagamento = JsonSerializer.Deserialize<JsonElement>(corpoPagamento);
        bool aprovado = resultadoPagamento.GetProperty("aprovado").GetBoolean();

        if (!aprovado)
        {
            var tempoRejeicao = (DateTime.Now - inicioTotal).TotalMilliseconds;
            Console.WriteLine($"🛒 Pedido REJEITADO em {tempoRejeicao:F0}ms");
            return BadRequest(new
            {
                pedidoId = pedido.Id,
                status = "rejeitado",
                mensagem = "Pagamento não aprovado",
                tempoTotalMs = tempoRejeicao
            });
        }

        // ── Passo 2: Publicar evento na fila (ASSÍNCRONO) ────────────
        //
        // Em vez de chamar NotificacaoApi diretamente (e esperar 3s),
        // publicamos um EVENTO na fila. O NotificacaoBackground vai
        // consumir esse evento em background e chamar NotificacaoApi
        // quando puder — sem bloquear a resposta ao cliente.

        // ╔═══════════════════════════════════════════════════════╗
        // ║  TODO 4: Publicar evento na fila                     ║
        // ║                                                      ║
        // ║  Passos:                                             ║
        // ║  1. Crie um EventoPedidoAprovado com os dados        ║
        // ║     do pedido (PedidoId, Cliente, Produto, Valor)    ║
        // ║  2. Use _fila.PublicarAsync(evento) para publicar    ║
        // ╚═══════════════════════════════════════════════════════╝

        var evento = new EventoPedidoAprovado
        {
            PedidoId = pedido.Id,
            Cliente = pedido.Cliente,
            Produto = pedido.Produto,
            Valor = valor.Valor
        };
        await _fila.PublicarAsync(evento);


        // ── Passo 3: Retornar resposta IMEDIATAMENTE ─────────────────

        // ╔═══════════════════════════════════════════════════════╗
        // ║  TODO 5: Retornar resposta sem esperar notificação   ║
        // ║                                                      ║
        // ║  Use Accepted() (HTTP 202) em vez de Ok() (HTTP 200) ║
        // ║  para indicar que o pedido foi aceito mas a           ║
        // ║  notificação ainda será processada.                  ║
        // ║                                                      ║
        // ║  Substitua o return Ok(...) abaixo por Accepted()    ║
        // ╚═══════════════════════════════════════════════════════╝

        var tempoTotal = (DateTime.Now - inicioTotal).TotalMilliseconds;
        Console.WriteLine($"🛒 ═══ ASSÍNCRONO concluído em {tempoTotal:F0}ms ═══");
        Console.WriteLine($"🛒 (notificação será processada em background)\n");

        // ⬇⬇⬇ TROQUE Ok() POR Accepted() APÓS COMPLETAR O TODO 5 ⬇⬇⬇
        return Accepted(new
        {
            pedidoId = pedido.Id,
            status = "aprovado",
            notificacao = "pendente — será processada em background",
            fluxo = "assincrono",
            tempoTotalMs = tempoTotal,
            observacao = "⚡ Compare este tempo com o fluxo síncrono!"
        });
        // ⬆⬆⬆ TROQUE Ok() POR Accepted() APÓS COMPLETAR O TODO 5 ⬆⬆⬆
    }
}
