namespace Application.Extensions;

public static class EnumerableExtensions
{
    /// <summary>
    ///     Get a paginated subset of the source.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T> Paginate<T>(this IEnumerable<T> source, int page, int pageSize)
    {
        return source.Skip((page - 1) * pageSize).Take(pageSize);
    }

    /// <summary>
    ///     Get a paginated subset of the source.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IQueryable<T> Paginate<T>(this IQueryable<T> source, int page, int pageSize)
    {
        return source.Skip(page * pageSize).Take(pageSize);
    }
}