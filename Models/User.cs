namespace RotinaXP.API.Models;
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int Points { get; set; }

    public List<TaskItem> Tasks { get; set; } = new();
    public List<Reward> Rewards { get; set; } = new();
    public List<DailyProgress> DailyProgresses { get; set; } = new();
}