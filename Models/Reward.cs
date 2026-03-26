
namespace RotinaXP.API.Models;
public class Reward
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int PointsCost { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
}