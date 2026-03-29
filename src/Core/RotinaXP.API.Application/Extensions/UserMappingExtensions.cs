using RotinaXP.API.DTOs;
using RotinaXP.API.Models;

namespace RotinaXP.API.Application.Extensions;

public static class UserMappingExtensions
{
    public static UserDTO ToDto(this User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        Points = user.Points
    };
}
