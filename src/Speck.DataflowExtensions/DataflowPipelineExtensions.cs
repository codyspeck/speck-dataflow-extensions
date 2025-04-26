namespace Speck.DataflowExtensions;

public static class DataflowPipelineExtensions
{
    public static async Task SendAndWaitForCompletionAsync<TInput>(
        this DataflowPipeline<Completable<TInput>> pipeline,
        TInput input)
    {
        var completable = new Completable<TInput>(input);

        await pipeline.SendAsync(completable);
        
        await completable.Completion;
    }
    
    public static async Task SendAndWaitForCompletionAsync<TInput, TOutput>(
        this DataflowPipeline<Completable<TInput, TOutput>> pipeline,
        TInput input)
    {
        var completable = new Completable<TInput, TOutput>(input);

        await pipeline.SendAsync(completable);
        
        await completable.Completion;
    }
}
