using System.Threading.Tasks.Dataflow;

namespace Speck.DataflowExtensions;

public static class BatchedTimeoutBlock
{
    public static IPropagatorBlock<T, T[]> Create<T>(int batchSize, TimeSpan timeout)
    {
        var batchBlock = new BatchBlock<T>(batchSize);

        var timer = new Timer(_ => batchBlock.TriggerBatch());

        var actionBlock = new ActionBlock<T>(item =>
        {
            batchBlock.Post(item);
            
            timer.Change(timeout, Timeout.InfiniteTimeSpan);
        });

        actionBlock.Completion.ContinueWith(_ =>
        {
            timer.Dispose();
            
            batchBlock.Complete();
        });
        
        return DataflowBlock.Encapsulate(actionBlock, batchBlock);
    }
}
