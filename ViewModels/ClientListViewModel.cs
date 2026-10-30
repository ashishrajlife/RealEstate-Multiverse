using Multiverse.Web.Models;

namespace Multiverse.Web.ViewModels;

public class ClientListViewModel
{
    public IReadOnlyList<Client> Clients { get; set; } = Array.Empty<Client>();

    public int Total { get; set; }
    public int Active { get; set; }
    public int NewThisWeek { get; set; }

    // filters
    public string? Query { get; set; }
    public ClientStatus? Status { get; set; }
    public IndianState? State { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    // paging
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int FilteredTotal { get; set; }

    public int TotalPages => FilteredTotal == 0 ? 1
        : (int)Math.Ceiling(FilteredTotal / (double)PageSize);
    public int FirstOnPage => FilteredTotal == 0 ? 0 : (Page - 1) * PageSize + 1;
    public int LastOnPage  => Math.Min(Page * PageSize, FilteredTotal);
    public bool HasPrev => Page > 1;
    public bool HasNext => Page < TotalPages;

    public bool AnyFilter =>
        !string.IsNullOrWhiteSpace(Query) || Status.HasValue
        || State.HasValue || CreatedFrom.HasValue || CreatedTo.HasValue;
}