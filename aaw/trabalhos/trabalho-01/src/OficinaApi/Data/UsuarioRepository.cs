using MongoDB.Driver;
using OficinaApi.Models;

namespace OficinaApi.Data;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<Usuario> CriarAsync(Usuario usuario);
}

public class UsuarioRepository : IUsuarioRepository
{
    private readonly IMongoCollection<Usuario> _usuarios;

    public UsuarioRepository(IMongoDatabase database)
    {
        _usuarios = database.GetCollection<Usuario>("usuarios");
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email)
    {
        return await _usuarios.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<Usuario> CriarAsync(Usuario usuario)
    {
        await _usuarios.InsertOneAsync(usuario);
        return usuario;
    }
}