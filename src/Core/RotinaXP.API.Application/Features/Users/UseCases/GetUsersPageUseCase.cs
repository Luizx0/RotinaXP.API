using RotinaXP.API.Application.Interfaces.Services;
using RotinaXP.API.DTOs;

namespace RotinaXP.API.Features.Users.UseCases;

public class GetUsersPageUseCase
{
    private readonly IUserService _service;

    public GetUsersPageUseCase(IUserService service)
    {
        _service = service;
    }

    public async Task<PagedResult<UserDTO>> ExecuteAsync(int authenticatedUserId, int page, int pageSize)
    {
        var normalizedPage = page < PaginationDefaults.DefaultPage
            ? PaginationDefaults.DefaultPage
            : page;

        var normalizedPageSize = pageSize < 1
            ? PaginationDefaults.DefaultPageSize
            : Math.Min(pageSize, PaginationDefaults.MaxPageSize);

        var currentUser = await _service.GetUserByIdAsync(authenticatedUserId);
        var hasCurrentUserOnPage = normalizedPage == PaginationDefaults.DefaultPage;

        return new PagedResult<UserDTO>
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            TotalItems = currentUser != null ? 1 : 0,
            TotalPages = currentUser != null ? 1 : 0,
            HasNext = false,
            HasPrevious = false,
            Items = hasCurrentUserOnPage && currentUser != null ? [currentUser] : []
        };
    }
}
