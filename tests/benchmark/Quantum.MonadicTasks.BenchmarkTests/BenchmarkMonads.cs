using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Quantum.MonadicTasks;
using System.Threading.Tasks;

[MemoryDiagnoser]
public class BenchmarkMonads
{
    private TaskMonad<int> monad;
    private MaybeMonad<int> maybeSome;
    private MaybeMonad<int> maybeNone;

    [GlobalSetup]
    public void Setup()
    {
        monad = TaskMonad<int>.Unit(10);
        maybeSome = MaybeMonad<int>.Some(42);
        maybeNone = MaybeMonad<int>.None();
    }

    [Benchmark]
    public async Task Chain_5_Binds()
    {
        var result = await monad
            .Bind(x => TaskMonad<int>.Unit(x + 1))
            .Bind(x => TaskMonad<int>.Unit(x * 2))
            .Bind(x => TaskMonad<int>.Unit(x - 3))
            .Bind(x => TaskMonad<int>.Unit(x / 2))
            .Bind(x => TaskMonad<int>.Unit(x + 10))
            .GetResultAsync();
    }

    [Benchmark]
    public async Task PerformAsyncSideEffect()
    {
        var result = await monad
            .PerformSideEffect(async x =>
            {
                await Task.Delay(1);
            })
            .GetResultAsync();
    }

    [Benchmark]
    public async Task MaybeSome_ToTaskMonad()
    {
        var result = await maybeSome
            .ToTaskMonad()
            .GetResultAsync();
    }

    [Benchmark]
    public async Task MaybeNone_ToTaskMonad()
    {
        try
        {
            await maybeNone
                .ToTaskMonad()
                .GetResultAsync();
        }
        catch
        {
            // expected
        }
    }

    [Benchmark]
    public async Task FlatMap_Using_Map_And_Bind()
    {
        var result = await monad
            .Map(x => x * 2)
            .Bind(x => TaskMonad<int>.Unit(x + 1))
            .GetResultAsync();
    }
}
