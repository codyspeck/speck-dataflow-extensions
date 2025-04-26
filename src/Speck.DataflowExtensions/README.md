# Speck.DataflowExtensions

A collection of extensions for the [TPL Dataflow Library](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library).

---

## Dataflow Pipelines

Dataflow pipelines can be constructed via a fluent builder interface. This builder abstracts creating distinct dataflow
blocks and wiring them up to each other with completion propagation enabled. Disposing of the dataflow pipeline will
trigger a completion of the initial dataflow block and then block on completion of the final dataflow block in the
chain.

```c#
await using var dataflowPipeline = new DataflowPipelineBuilder<string>
    .Select(value => $"Hello, {value}")
    .Select(value => value + "!")
    .Build(Console.WriteLine);

await dataflowPipeline.SendAsync("World");
```

**ExecutionDataflowBlockOptions** can be injected into any dataflow block in the pipeline:

```c#
var executionOptions = new DataflowExecutionBlockOptions
{
    BoundedCapacity = 1000,
    EnsureOrdered = false,
    MaxDegreeOfParalellism = 4
};

await using var dataflowPipeline = new DataflowPipelineBuilder<string>
    .Select(async value => await database.InsertAsync(value), executionOptions)
    .Build(value => Console.WriteLine($"{value} inserted into database."));
```
