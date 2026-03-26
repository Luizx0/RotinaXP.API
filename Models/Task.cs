using System.Text.Json.Serialization;
namespace RotinaXP.API.Models;
public class Tarefa
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public bool Concluida { get; set; }
    public int UsuarioId { get; set; }
    [JsonIgnore]
    public Usuario? Usuario { get; set; }
}
