namespace Quantum.MonadicTasks;

public abstract class MonadExpression<T>
{
    public abstract Task<T> Run();
}


internal class MapMonadExpression<T, TU>
    (TaskMonad<T> monad, Func<T, TaskMonad<TU>> func) : MonadExpression<TU>
{
    public override async Task<TU> Run()
    {
        var taskMonad = func.Invoke(await monad.GetResultAsync());
        return await taskMonad.GetResultAsync();
    }

}

internal class Map<T, TU> : MonadExpression<TU>
{
    private readonly TaskMonad<T> _taskMonad;
    private readonly Func<T, TU> _func;

    public Map(TaskMonad<T> taskMonad, Func<T, TU> func)
    {
        _taskMonad = taskMonad;
        _func = func;
    }

    public override async Task<TU> Run()
    {
        var taskMonad = _func.Invoke(await _taskMonad.GetResultAsync());
        return taskMonad;
    }
}
internal class Pure<T>(Task<T> fromResult) : MonadExpression<T>
{
    public override async Task<T> Run()
    {
        return await fromResult;
    }
}
internal class PureExpression<T, TU>(TaskMonad<T> monad, Func<T, TU> func) : MonadExpression<TU>
{
    public override async Task<TU> Run()
    {
        var taskMonad = func.Invoke(await monad.GetResultAsync());
        return taskMonad;
    }

}

internal class Bind<T>(TaskMonad<T> monad) : MonadExpression<T>
{
    public override async Task<T> Run()
    {
        return await monad.GetResultAsync();
    }
}


public class TaskMonad<T>
{
    private readonly MonadExpression<T> _taskMonadExpression;

    internal TaskMonad(MonadExpression<T> taskMonadExpression) => _taskMonadExpression = taskMonadExpression;

    // Constructor to initialize with a Task<T>

    // The 'Unit' function (also called 'unit' in some places)
    // It takes a value and wraps it into a Task
    public static TaskMonad<T> Unit(T value)
        => new(new Pure<T>(Task.FromResult(value)));

    public static TaskMonad<T> Unit(Func<Task<T>> func)
        => new(new Pure<T>(func.Invoke()));

    // The 'Bind' function (asynchronous version)
    // It takes a function that maps T to TaskMonad<U> and applies it to the result of the task
    public TaskMonad<TU> Bind<TU>(Func<T, TaskMonad<TU>> func)
        => new(new MapMonadExpression<T, TU>(this, func));

    public TaskMonad<T> Bind(Func<T, T> func)
        => new(new PureExpression<T,T>(this, func));

    // The 'Map' function (or 'Select')
    // It transforms the value inside the TaskMonad without changing the task itself
    public TaskMonad<TU> Map<TU>(Func<T, TU> func)
        => new(new Map<T, TU>(this, func));

    // 'Flatten' function: Unwrap a Task<Task<T>> to Task<T>
    public static async Task<TaskMonad<T>> Flatten(TaskMonad<TaskMonad<T>> monad)
        => await monad._taskMonadExpression.Run();


    // 'Fail' function: Creates a TaskMonad that represents a failure (an exception)
    public static TaskMonad<T> Fail(Exception exception) =>
        new(new Pure<T>(Task.FromException<T>(exception)));

    // Get the result of the task asynchronously
    public async Task<T> GetResultAsync()
        => await _taskMonadExpression.Run();

    // Sequence method: used for handling parallel tasks (returns a Task of an array of results)
    public static async Task<TaskMonad<T[]>> Sequence(TaskMonad<T>[] monads)
    {
        var results = await Task.WhenAll(monads.Select(m => m._taskMonadExpression.Run()));
        return TaskMonad<T[]>.Unit(results);
    }
}