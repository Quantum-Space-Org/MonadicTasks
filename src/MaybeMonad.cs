public sealed class MaybeMonad<T>
{
    private readonly Internal? _content;

    private record Internal(T Value);

    private MaybeMonad(Internal? content) => _content = content;

    public static MaybeMonad<T> Some(T value) => new(new Internal(value));
    public static MaybeMonad<T> None() => new(null);

    public bool HasValue => _content is not null;

    public T ValueOrDefault() => _content!.Value;
}