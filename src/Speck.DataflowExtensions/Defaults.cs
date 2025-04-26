using System.Threading.Tasks.Dataflow;

namespace Speck.DataflowExtensions;

internal static class Defaults
{
    internal static DataflowLinkOptions DataflowLinkOptions { get; } = new() { PropagateCompletion = true };
    
    internal static ExecutionDataflowBlockOptions ExecutionDataflowBlockOptions { get; } = new();
}