namespace SamplVSSkill.Domain.Common;

/// <summary>
/// Generic paginated result wrapper used by all paged query responses.
/// Placed in Domain/Common because it represents a cross-cutting query concern,
/// not infrastructure — extracted here because it will be used in 2+ domains.
/// </summary>
public record PaginatedResult<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
