public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string SenhaHash { get; set; }

    public int Pontos { get; set; } = 0;

    public List<Tarefa> Tarefas { get; set; }
}