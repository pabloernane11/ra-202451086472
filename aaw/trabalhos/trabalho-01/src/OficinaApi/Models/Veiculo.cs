using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OficinaApi.Models;

public class Veiculo
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [Required(ErrorMessage = "A placa é obrigatória.")]
    public string Placa { get; set; } = string.Empty;

    [Required(ErrorMessage = "A marca é obrigatória.")]
    public string Marca { get; set; } = string.Empty;

    [Required(ErrorMessage = "O modelo é obrigatório.")]
    public string Modelo { get; set; } = string.Empty;

    [Range(1950, 2030, ErrorMessage = "Informe um ano válido entre 1950 e 2030.")]
    public int Ano { get; set; }

    [Required(ErrorMessage = "O nome do proprietário é obrigatório.")]
    public string NomeProprietario { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone do proprietário é obrigatório.")]
    public string TelefoneProprietario { get; set; } = string.Empty;
}