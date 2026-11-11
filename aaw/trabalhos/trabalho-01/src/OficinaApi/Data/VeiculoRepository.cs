using MongoDB.Bson;
using MongoDB.Driver;
using OficinaApi.Models;

namespace OficinaApi.Data;

public interface IVeiculoRepository
{
    Task<List<Veiculo>> ListarTodosAsync();
    Task<Veiculo?> ObterPorIdAsync(string id);
    Task<Veiculo?> ObterPorPlacaAsync(string placa);
    Task<Veiculo> CriarAsync(Veiculo veiculo);
    Task<bool> AtualizarAsync(string id, Veiculo veiculo);
    Task<bool> RemoverAsync(string id);
}

public class VeiculoRepository : IVeiculoRepository
{
    private readonly IMongoCollection<Veiculo> _colecao;

    public VeiculoRepository(IMongoDatabase database)
    {
        _colecao = database.GetCollection<Veiculo>("veiculos");
    }
     public async Task<List<Veiculo>> ListarTodosAsync() =>
        await _colecao.Find(_ => true).ToListAsync();

    public async Task<Veiculo?> ObterPorIdAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;
            
        return await _colecao.Find(v => v.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Veiculo?> ObterPorPlacaAsync(string placa) =>
        await _colecao.Find(v => v.Placa == placa).FirstOrDefaultAsync();

    public async Task<Veiculo> CriarAsync(Veiculo veiculo)
    {
        veiculo.Id = null;
        await _colecao.InsertOneAsync(veiculo);
        return veiculo;
    }

    public async Task<bool> AtualizarAsync(string id, Veiculo veiculo)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        veiculo.Id = id;
        var resultado = await _colecao.ReplaceOneAsync(v => v.Id == id, veiculo);
        return resultado.MatchedCount > 0;
    }

    public async Task<bool> RemoverAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        var resultado = await _colecao.DeleteOneAsync(v => v.Id == id);
        return resultado.DeletedCount > 0;
    }
}