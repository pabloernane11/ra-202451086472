using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OficinaApi.Models;

public class OrdemServico
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [Required(ErrorMessage = "O ID do veículo é obrigatório.")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string VeiculoId { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição do problema é obrigatória.")]
    public string DescricaoProblema { get; set; } = string.Empty;

    public string Status { get; set; } = "Aberta";

    public decimal ValorPecas { get; set; }

    public decimal ValorMaoDeObra { get; set; }

    public decimal ValorTotal { get; set; }

    public DateTime DataAbertura { get; set; } = DateTime.UtcNow;
}