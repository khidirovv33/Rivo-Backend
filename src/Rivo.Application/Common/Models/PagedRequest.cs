namespace Rivo.Application.Common.Models;

/// <summary>Единый контракт пагинации/сортировки/фильтрации по тексту для всей команды.</summary>
public class PagedRequest
{
    private const int MaxPageSize = 100;

    private int _pageSize = 20;

    public int Page { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value <= 0 ? 20 : Math.Min(value, MaxPageSize);
    }

    public string? Search { get; set; }

    public string? SortBy { get; set; }

    public bool SortDescending { get; set; }
}
