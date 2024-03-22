namespace Web.Models;

/// <summary>
///     Paginated result of instances of <typeparamref name="T" />.
/// </summary>
/// <typeparam name="T"></typeparam>
public class PaginatedResult<T>
{
    public required T[] Data { get; set; }
    public required int TotalItems { get; set; }
}