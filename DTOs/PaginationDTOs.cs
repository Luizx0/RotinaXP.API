namespace RotinaXP.API.DTOs;

public static class PaginationDefaults
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
}

public class PagedResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public List<T> Items { get; set; } = new();
}
