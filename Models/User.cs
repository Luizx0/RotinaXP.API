using System.Text.Json.Serialization;
namespace RotinaXP.API.Models;
public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public int Pontos { get; set; } = 0;
    [JsonIgnore]
    public List<Tarefa> Tarefas { get; set; } = new();
}
