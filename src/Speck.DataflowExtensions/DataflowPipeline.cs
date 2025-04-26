using System.Threading.Tasks.Dataflow;

namespace Speck.DataflowExtensions;

public sealed class DataflowPipeline<TInput> : IAsyncDisposable
{
    private readonly ITargetBlock<TInput> _inputBlock;
    private readonly IDataflowBlock _terminalBlock;
    
    internal DataflowPipeline(ITargetBlock<TInput> inputBlock, IDataflowBlock terminalBlock)
    {
        _inputBlock = inputBlock;
        _terminalBlock = terminalBlock;
    }

    public Task SendAsync(TInput item)
    {
        return _inputBlock.SendAsync(item);
    }
    
    public async ValueTask DisposeAsync()
    {
        _inputBlock.Complete();

        try
        {
            await _terminalBlock.Completion;
        }
        catch (OperationCanceledException)
        {
            // Noop
        }
    }
}
