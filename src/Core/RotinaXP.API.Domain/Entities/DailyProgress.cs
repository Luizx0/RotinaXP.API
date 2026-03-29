
namespace RotinaXP.API.Models;
public class DailyProgress
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int CompletedTasksCount { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
}