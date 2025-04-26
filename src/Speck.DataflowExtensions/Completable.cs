namespace Speck.DataflowExtensions;

public class Completable<TValue>(TValue value)
{
    private readonly TaskCompletionSource _taskCompletionSource = new();

    public TValue Value { get; } = value;

    public Task Completion => _taskCompletionSource.Task;
    
    public void Complete()
    {
        _taskCompletionSource.TrySetResult();
    }

    public void Fail(Exception exception)
    {
        _taskCompletionSource.TrySetException(exception);
    }
}

public class Completable<TValue, TResult>(TValue value)
{
    private readonly TaskCompletionSource<TResult> _taskCompletionSource = new();

    public TValue Value { get; } = value;

    public Task Completion => _taskCompletionSource.Task;
    
    public void Complete(TResult result)
    {
        _taskCompletionSource.TrySetResult(result);
    }

    public void Fail(Exception exception)
    {
        _taskCompletionSource.TrySetException(exception);
    }
}
