using System.Threading.Tasks.Dataflow;
using AutoFixture;
using Shouldly;

namespace Speck.DataflowExtensions.UnitTests;

public class DataflowPipelineTests
{
    public class Observer<T>
    {
        private readonly List<T> _items = [];

        public IReadOnlyCollection<T> Items => _items;
        
        public void Add(T item)
        {
            _items.Add(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            _items.AddRange(items);
        }

        public Task AddAsync(T item)
        {
            _items.Add(item);
            return Task.CompletedTask;
        }
    }

    public class TestException : Exception;

    private readonly Fixture _fixture = new();
    private readonly Observer<string> _observer = new();

    [Test]
    public async Task Disposing_pipeline_throws_captured_exception()
    {
        var pipeline = DataflowPipelineBuilder.Create<string>()
            .Build(_ => throw new TestException());
        
        await pipeline.SendAsync(_fixture.Create<string>());
        
        await Should.ThrowAsync<TestException>(pipeline.DisposeAsync().AsTask());
    }
    
    [Test]
    public async Task Disposing_pipeline_ignores_operation_canceled_exceptions()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        
        var pipeline = DataflowPipelineBuilder.Create<string>()
            .Build(_ => { }, new ExecutionDataflowBlockOptions { CancellationToken = cancellationTokenSource.Token });

        await cancellationTokenSource.CancelAsync();
        
        await Should.NotThrowAsync(pipeline.DisposeAsync().AsTask());
    }
    
    [Test]
    public async Task Pipeline_batches_items()
    {
        var expected = _fixture.CreateMany<string>().ToList();

        var pipeline = DataflowPipelineBuilder.Create<string>()
            .Batch(expected.Count, Timeout.InfiniteTimeSpan)
            .Build(_observer.AddRange);

        foreach (var item in expected)
            await pipeline.SendAsync(item);
        
        await pipeline.DisposeAsync();
        
        _observer.Items.ShouldBe(expected);
    }

    [Test]
    public async Task Pipeline_filters_items()
    {
        var item1 = _fixture.Create<string>();
        var item2 = _fixture.Create<string>();
        var item3 = _fixture.Create<string>();

        var pipeline = DataflowPipelineBuilder.Create<string>()
            .Where(value => value == item1)
            .Build(_observer.Add);
        
        await pipeline.SendAsync(item1);
        await pipeline.SendAsync(item2);
        await pipeline.SendAsync(item3);
        await pipeline.DisposeAsync();
        
        _observer.Items.ShouldBe([item1]);
    }
    
    [Test]
    public async Task Pipeline_performs_transformation()
    {
        var expected = _fixture.Create<string>();
        
        var pipeline = DataflowPipelineBuilder.Create<string>()
            .Select(_ => expected)
            .Build(_observer.Add);
        
        await pipeline.SendAsync(expected);
        await pipeline.DisposeAsync();
        
        _observer.Items.ShouldContain(expected);
    }
    
    [Test]
    public async Task Pipeline_performs_async_transformation()
    {
        var expected = _fixture.Create<string>();
        
        var pipeline = DataflowPipelineBuilder.Create<string>()
            .Select(_ => Task.FromResult(expected))
            .Build(_observer.AddAsync);
        
        await pipeline.SendAsync(expected);
        await pipeline.DisposeAsync();
        
        _observer.Items.ShouldContain(expected);
    }
}
