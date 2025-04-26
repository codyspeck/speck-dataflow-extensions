using System.Threading.Tasks.Dataflow;

namespace Speck.DataflowExtensions;

public static class DataflowPipelineBuilder
{
    public static DataflowPipelineBuilder<TInput, TInput> Create<TInput>()
    {
        var initialBlock = new TransformBlock<TInput, TInput>(input => input);
        
        return new DataflowPipelineBuilder<TInput, TInput>(initialBlock, initialBlock);
    }
}

public class DataflowPipelineBuilder<TInput, TOutput>
{
    private readonly ITargetBlock<TInput> _inputBlock;
    private readonly ISourceBlock<TOutput> _terminalBlock;
    
    internal DataflowPipelineBuilder(ITargetBlock<TInput> inputBlock, ISourceBlock<TOutput> terminalBlock)
    {
        _inputBlock = inputBlock;
        _terminalBlock = terminalBlock;
    }
    
    public DataflowPipelineBuilder<TInput, TOutput[]> Batch(int batchSize, TimeSpan timeout)
    {
        var terminalBlock = BatchedTimeoutBlock.Create<TOutput>(batchSize, timeout);

        _terminalBlock.LinkTo(terminalBlock, Defaults.DataflowLinkOptions);
        
        return new DataflowPipelineBuilder<TInput, TOutput[]>(_inputBlock, terminalBlock);
    }
    
    public DataflowPipeline<TInput> Build(Action<TOutput> action)
    {
        return Build(action, Defaults.ExecutionDataflowBlockOptions);
    }
    
    public DataflowPipeline<TInput> Build(Action<TOutput> action, ExecutionDataflowBlockOptions options)
    {
        var terminalBlock = new ActionBlock<TOutput>(action, options);
        
        _terminalBlock.LinkTo(terminalBlock, Defaults.DataflowLinkOptions);
        
        return new DataflowPipeline<TInput>(_inputBlock, terminalBlock);
    }
    
    public DataflowPipeline<TInput> Build(Func<TOutput, Task> func)
    {
        return Build(func, Defaults.ExecutionDataflowBlockOptions);
    }
    
    public DataflowPipeline<TInput> Build(Func<TOutput, Task> func, ExecutionDataflowBlockOptions options)
    {
        var terminalBlock = new ActionBlock<TOutput>(func, options);
        
        _terminalBlock.LinkTo(terminalBlock, Defaults.DataflowLinkOptions);
        
        return new DataflowPipeline<TInput>(_inputBlock, terminalBlock);
    }

    public DataflowPipelineBuilder<TInput, TNext> Select<TNext>(Func<TOutput, TNext> func)
    {
        return Select(func, Defaults.ExecutionDataflowBlockOptions);
    }
    
    public DataflowPipelineBuilder<TInput, TNext> Select<TNext>(
        Func<TOutput, TNext> func,
        ExecutionDataflowBlockOptions options)
    {
        var terminalBlock = new TransformBlock<TOutput, TNext>(func, options);
        
        _terminalBlock.LinkTo(terminalBlock, Defaults.DataflowLinkOptions);
        
        return new DataflowPipelineBuilder<TInput, TNext>(_inputBlock, terminalBlock);
    }
    
    public DataflowPipelineBuilder<TInput, TNext> Select<TNext>(Func<TOutput, Task<TNext>> func)
    {
        return Select(func, Defaults.ExecutionDataflowBlockOptions);
    }
    
    public DataflowPipelineBuilder<TInput, TNext> Select<TNext>(
        Func<TOutput, Task<TNext>> func,
        ExecutionDataflowBlockOptions options)
    {
        var terminalBlock = new TransformBlock<TOutput, TNext>(func, options);
        
        _terminalBlock.LinkTo(terminalBlock, Defaults.DataflowLinkOptions);
        
        return new DataflowPipelineBuilder<TInput, TNext>(_inputBlock, terminalBlock);
    }
    
    public DataflowPipelineBuilder<TInput, TOutput> Where(Predicate<TOutput> predicate)
    {
        return Where(predicate, Defaults.ExecutionDataflowBlockOptions);
    }
    
    public DataflowPipelineBuilder<TInput, TOutput> Where(
        Predicate<TOutput> predicate,
        ExecutionDataflowBlockOptions options)
    {
        var terminalBlock = new TransformBlock<TOutput, TOutput>(x => x, options);
        
        _terminalBlock.LinkTo(terminalBlock, Defaults.DataflowLinkOptions, predicate);
        _terminalBlock.LinkTo(DataflowBlock.NullTarget<TOutput>());
        
        return new DataflowPipelineBuilder<TInput, TOutput>(_inputBlock, terminalBlock);
    }
}
