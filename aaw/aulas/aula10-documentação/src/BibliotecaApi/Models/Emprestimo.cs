using System.ComponentModel.DataAnnotations;

namespace BibliotecaApi.Models;

/// <summary>
/// Representa um registro de empréstimo de um livro para um usuário.
/// </summary>
public class Emprestimo
{
    /// <summary>
    /// Identificador único do empréstimo.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// ID do livro emprestado.
    /// </summary>
    /// <example>1</example>
    [Required]
    public int LivroId { get; set; }

    /// <summary>
    /// Nome do leitor/usuário que retirou o livro.
    /// </summary>
    /// <example>João da Silva</example>
    [Required(ErrorMessage = "O nome do usuário é obrigatório.")]
    [StringLength(200)]
    public string NomeUsuario { get; set; } = string.Empty;

    /// <summary>
    /// Data e hora em que o empréstimo foi realizado.
    /// </summary>
    public DateTime DataEmprestimo { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data limite prevista para a devolução do livro (padrão: 14 dias).
    /// </summary>
    public DateTime DataDevolucaoPrevista { get; set; } = DateTime.UtcNow.AddDays(14);

    /// <summary>
    /// Data em que o livro foi efetivamente devolvido (nulo se ainda ativo).
    /// </summary>
    public DateTime? DataDevolucaoEfetiva { get; set; }

    /// <summary>
    /// Status atual do empréstimo.
    /// </summary>
    /// <example>0</example>
    public StatusEmprestimo Status { get; set; } = StatusEmprestimo.Ativo;
}

public enum StatusEmprestimo
{
    Ativo,
    Devolvido,
    Atrasado
}