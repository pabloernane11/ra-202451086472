using System.Text.Json;
using ServicoProdutos.Models;

namespace ServicoProdutos.Repositorios;

public class ProdutoRepositorioDocumento : IProdutoRepositorio
{
    private readonly string _pasta;
    private static readonly JsonSerializerOptions _json =
        new() { WriteIndented = true };

    public ProdutoRepositorioDocumento(IConfiguration config)
    {
        _pasta = config["PastaDocumentos"] ?? "dados";
        Directory.CreateDirectory(_pasta);
    }

    private string CaminhoDoDocumento(int id) =>
        Path.Combine(_pasta, $"produto-{id}.json");

    public List<Produto> ObterTodos()
    {
        var produtos = new List<Produto>();
        foreach (var arquivo in Directory.EnumerateFiles(_pasta, "produto-*.json"))
        {
            var json = File.ReadAllText(arquivo);
            var produto = JsonSerializer.Deserialize<Produto>(json);
            if (produto is not null)
                produtos.Add(produto);
        }
        return produtos.OrderBy(p => p.Id).ToList();
    }

    public Produto? ObterPorId(int id)
    {
        var caminho = CaminhoDoDocumento(id);
        if (!File.Exists(caminho))
            return null;
        return JsonSerializer.Deserialize<Produto>(File.ReadAllText(caminho));
    }

    public List<Produto> ObterAbaixoDe(decimal precoMaximo)
    {
        return ObterTodos().Where(p => p.Preco < precoMaximo).ToList();
    }

    public Produto Criar(Produto produto)
    {
        var existentes = ObterTodos();
        produto.Id = existentes.Count == 0 ? 1 : existentes.Max(p => p.Id) + 1;
        File.WriteAllText(CaminhoDoDocumento(produto.Id),
            JsonSerializer.Serialize(produto, _json));
        return produto;
    }
}