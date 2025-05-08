using Quantum.MonadicTasks;

public static class MaybeTaskMonadExtensions
{
    public static TaskMonad<T> ToTaskMonad<T>(this MaybeMonad<T> maybe)
    {
        var maybeHasValue = maybe.HasValue;

        return maybeHasValue
            ? TaskMonad<T>.Unit(maybe.ValueOrDefault()!)
            : TaskMonad<T>.Fail(new InvalidOperationException("Maybe was None"));
    }


    public static TaskMonad<T> PerformSideEffect<T>(this TaskMonad<T> taskMonad, Action<T> sideEffect)
    {
        return taskMonad.Map(value =>
        {
            sideEffect(value);
            return value;
        });
    }

    public static  TaskMonad<T> PerformSideEffect<T>(this TaskMonad<T> taskMonad, Func<T, Task> sideEffect)
    {
        return new TaskMonad<T>(new TapExpression<T>(taskMonad, sideEffect));
    }

    internal class TapExpression<T>(TaskMonad<T> monad, Func<T, Task> sideEffect) : MonadExpression<T>
    {
        public override async Task<T> Run()
        {
            var value = await monad.GetResultAsync();
            await sideEffect(value);
            return value;
        }
    }
}