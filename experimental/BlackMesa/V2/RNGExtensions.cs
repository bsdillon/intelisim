namespace BlackMesa.V2;

public static class RNGExtensions
{
    public static T? TakeFirstRandom<T>(
        this IReadOnlyList<T> collection,
        Random random)
    {
        if (collection.Count == 0)
            return default;

        return collection[random.Next(collection.Count)];
    }
}