using System.ComponentModel.DataAnnotations;

namespace RotinaXP.API.DTOs;

public class TaskDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int UserId { get; set; }
}

public class RewardDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int PointsCost { get; set; }
    public int UserId { get; set; }
}

public class DailyProgressDTO
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int CompletedTasksCount { get; set; }
    public int UserId { get; set; }
}

public class CreateTaskDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    [Range(1, int.MaxValue)]
    public int UserId { get; set; }
}

public class UpdateTaskDto
{
    [StringLength(200, MinimumLength = 1)]
    public string? Title { get; set; }

    public bool? IsCompleted { get; set; }
}

public class CreateRewardDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int PointsCost { get; set; }

    [Range(1, int.MaxValue)]
    public int UserId { get; set; }
}

public class UpdateRewardDto
{
    [StringLength(200, MinimumLength = 1)]
    public string? Title { get; set; }

    [Range(1, int.MaxValue)]
    public int? PointsCost { get; set; }
}
