static class NotEmptyEnumerable
{
    public static IEnumerable<T> NotEmpty<T>(this IEnumerable<T>? items)
    {
        return items ?? Array.Empty<T>();
    }
}