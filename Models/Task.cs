using System.Text.Json.Serialization;

namespace RotinaXP.API.Models;

/// <summary>
/// Representa uma tarefa do sistema RotinaXP.
/// </summary>
public class Tarefa
{
    /// <summary>
    /// Identificador único da tarefa.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Título da tarefa.
    /// </summary>
    public string Titulo { get; set; } = string.Empty;

    /// <summary>
    /// Indica se a tarefa foi concluída.
    /// </summary>
    public bool Concluida { get; set; }

    /// <summary>
    /// Identificador do usuário proprietário da tarefa.
    /// </summary>
    public int UsuarioId { get; set; }

    /// <summary>
    /// Referência ao usuário proprietário da tarefa.
    /// </summary>
    [JsonIgnore]
    public Usuario? Usuario { get; set; }
}
