namespace BibliotecaApi.Models;

/// <summary>
/// Estrutura de resposta padronizada para listagens paginadas.
/// </summary>
public class RespostaPaginada<T>
{
    /// <summary>
    /// Lista dos itens da página atual.
    /// </summary>
    public List<T> Itens { get; set; } = new();

    /// <summary>
    /// Número da página atual (iniciando em 1).
    /// </summary>
    /// <example>1</example>
    public int Pagina { get; set; }

    /// <summary>
    /// Quantidade de itens por página.
    /// </summary>
    /// <example>10</example>
    public int TamanhoPagina { get; set; }

    /// <summary>
    /// Total geral de itens no acervo.
    /// </summary>
    /// <example>45</example>
    public int TotalItens { get; set; }

    /// <summary>
    /// Total de páginas disponíveis calculadas.
    /// </summary>
    /// <example>5</example>
    public int TotalPaginas => (int)Math.Ceiling((double)TotalItens / TamanhoPagina);
}