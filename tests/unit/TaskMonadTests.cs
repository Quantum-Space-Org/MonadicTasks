namespace Quantum.MonadicTasks.Tests;

public class TaskMonadTests
{
    // Test for return/unit functionality
    [Fact]
    public async Task Unit_ShouldWrapValueIntoTaskMonad()
    {
        var monad = TaskMonad<int>.Unit(5);

        var result = await monad.GetResultAsync();

        Assert.Equal(5, result);
    }

    [Fact]
    public async Task Unit_ShouldWrapTaskIntoTaskMonad()
    {
        var monad = TaskMonad<int>.Unit(() => Task.FromResult(5));

        var result = await monad.GetResultAsync();

        Assert.Equal(5, result);
    }

    // Test for Bind functionality (chaining)
    [Fact]
    public async Task Bind_ShouldChainTasks()
    {
        var monad = TaskMonad<int>.Unit(5);

        var result = await monad
            .Bind(x => TaskMonad<int>.Unit(x + 5))  // (10 * 2) = 20
            .Bind(x => TaskMonad<int>.Unit(x * 2))  // (10 * 2) = 20
            .GetResultAsync();

        Assert.Equal(20, result);  // (5 + 5) * 2 = 20
    }


    // Test for Map functionality (transforming inside the monad)
    [Fact]
    public async Task Map_ShouldTransformValueInsideMonad()
    {
        var monad = TaskMonad<int>.Unit(5);

        var mappedMonad = monad.Map(x => x * 2).Map(x=>x/2);

        var result = await mappedMonad.GetResultAsync();
        Assert.Equal(5, result);  // 5 * 2 / 2 = 5
    }

    // Test for Flatten functionality (unwrapping nested monads)
    [Fact]
    public async Task Flatten_ShouldUnwrapNestedMonads()
    {
        var monad = TaskMonad<TaskMonad<int>>
            .Unit(TaskMonad<int>.Unit(5));

        var flattened = TaskMonad<int>.Flatten(monad);

        var result = await flattened;
        Assert.Equal(5, await result.GetResultAsync());
    }

    // Test for Fail functionality (handling errors)
    [Fact]
    public async Task Fail_ShouldReturnFailedTaskMonad()
    {
        var exception = new InvalidOperationException("Test exception");
        var monad = TaskMonad<int>.Fail(exception);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await monad.GetResultAsync().ConfigureAwait(false));

        Assert.Equal("Test exception", ex.Message);
    }

    // Test for Sequence functionality (parallel tasks)
    [Fact]
    public async Task Sequence_ShouldHandleParallelTasks()
    {
        var monads = new[]
        {
            TaskMonad<int>.Unit(1),
            TaskMonad<int>.Unit(2),
            TaskMonad<int>.Unit(3)
        };

        var result = await TaskMonad<int>.Sequence(monads);

        var resultArray = await result.GetResultAsync();

        Assert.Equal(new[] { 1, 2, 3 }, resultArray);
    }


    [Fact]
    public async Task Bind_ShouldPropagateException()
    {

        TaskMonad<int>.Unit(5)
            .Bind(x => throw new InvalidOperationException("Failure during bind"));
        
        var monad = TaskMonad<int>.Unit(5)
            .Bind(x => TaskMonad<int>.Fail(new InvalidOperationException("Failure during bind")));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => monad.GetResultAsync());
        Assert.Equal("Failure during bind", ex.Message);
    }


    [Fact]
    public async Task Map_ShouldPropagateException()
    {
        var monad = TaskMonad<int>.Unit(5)
            .Map<Exception>(x => throw new InvalidOperationException("Failure in map"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => monad.GetResultAsync());
        Assert.Equal("Failure in map", ex.Message);
    }

    [Fact]
    public async Task Bind_ShouldBubbleUpInnerFailure()
    {
        var innerFailed = TaskMonad<int>.Fail(new ArgumentException("Inner fail"));

        var monad = TaskMonad<int>
            .Unit(5)
            .Bind(_ => innerFailed);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => monad.GetResultAsync());
        Assert.Equal("Inner fail", ex.Message);
    }

    [Fact]
    public async Task Sequence_ShouldFailIfAnyMonadFails()
    {
        var monads = new[]
        {
            TaskMonad<int>.Unit(1),
            TaskMonad<int>.Fail(new Exception("Oops")),
            TaskMonad<int>.Unit(3)
        };

        var result = TaskMonad<int>.Sequence(monads);

        var ex = await Assert.ThrowsAsync<Exception>(async () =>await result);
        Assert.Equal("Oops", ex.Message);
    }

}