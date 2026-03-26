using System.Text.Json.Serialization;

namespace RotinaXP.API.Models;

/// <summary>
/// Representa um usuário do sistema RotinaXP.
/// </summary>
public class Usuario
{
    /// <summary>
    /// Identificador único do usuário.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nome completo do usuário.
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Endereço de email do usuário.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hash da senha do usuário.
    /// </summary>
    public string SenhaHash { get; set; } = string.Empty;

    /// <summary>
    /// Pontos acumulados do usuário.
    /// </summary>
    public int Pontos { get; set; } = 0;

    /// <summary>
    /// Lista de tarefas do usuário.
    /// </summary>
    [JsonIgnore]
    public List<Tarefa> Tarefas { get; set; } = new();
}