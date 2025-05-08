using Quantum.MonadicTasks;

public class MaybeTaskMonadExtensionsTests
{
    [Fact]
    public async Task ToTaskMonad_ShouldReturnTaskMonad_WhenMaybeHasValue()
    {
        // Arrange
        var maybe = MaybeMonad<int>.Some(42);

        // Act
        var taskMonad = maybe.ToTaskMonad();

        // Assert
        var result = await taskMonad.GetResultAsync();
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task ToTaskMonad_ShouldFail_WhenMaybeHasNoValue()
    {
        // Arrange
        var maybe = MaybeMonad<int>.None();

        // Act
        var taskMonad = maybe.ToTaskMonad();

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await taskMonad.GetResultAsync());
        Assert.Equal("Maybe was None", ex.Message);
    }

    // Test for PerformSideEffect<T> using Action (sync side effects)
    [Fact]
    public async Task PerformSideEffect_ShouldExecuteActionWithoutChangingValue()
    {
        // Arrange
        var sideEffectCalled = false;
        var monad = TaskMonad<int>.Unit(5);

        // Act
        var resultMonad = monad.PerformSideEffect(value =>
        {
            sideEffectCalled = true;
            Assert.Equal(5, value); // Make sure the value inside monad is 5
        });

        var result = await resultMonad.GetResultAsync();

        // Assert
        Assert.Equal(5, result); // Ensure the original value isn't modified
        Assert.True(sideEffectCalled); // Ensure the side effect was executed
    }

    // Test for PerformSideEffect<T> using Func<Task> (async side effects)
    [Fact]
    public async Task PerformSideEffect_ShouldExecuteAsyncActionWithoutChangingValue()
    {
        // Arrange
        var sideEffectCalled = false;
        var monad = TaskMonad<int>.Unit(10);

        // Act
        var resultMonad = monad.PerformSideEffect(async value =>
        {
            await Task.Delay(100); // Simulate async work
            sideEffectCalled = true;
            Assert.Equal(10, value); // Ensure the value inside monad is 10
        });

        var result = await resultMonad.GetResultAsync();

        // Assert
        Assert.Equal(10, result); // Ensure the original value isn't modified
        Assert.True(sideEffectCalled); // Ensure the async side effect was executed
    }
}
