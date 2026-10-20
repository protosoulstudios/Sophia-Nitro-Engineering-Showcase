// Executable contract checks for the independently authored demonstration only.
using Showcase.SceneTransitions;

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

var passed = 0;
async Task Test(string name, Func<Task> body)
{
    await body();
    Console.WriteLine("PASS " + name);
    passed++;
}

await Test("intermediary then destination, reset on completion", async () =>
{
    var backend = new FakeBackend();
    var coordinator = new TransitionCoordinator(backend, "Loading");
    Check(await coordinator.RequestAsync("Course") == RequestResult.Completed, "completion");
    Check(backend.Calls.SequenceEqual(new[] { "Loading", "Course" }), "load order");
    Check(coordinator.State == TransitionState.Idle && coordinator.Destination == null, "reset");
});
await Test("direct request skips intermediary", async () =>
{
    var backend = new FakeBackend();
    Check(await new TransitionCoordinator(backend, "Loading").RequestAsync("Course", false)
        == RequestResult.Completed, "direct completion");
    Check(backend.Calls.SequenceEqual(new[] { "Course" }), "direct order");
});
await Test("invalid targets never load", async () =>
{
    var backend = new FakeBackend();
    var coordinator = new TransitionCoordinator(backend, "Loading");
    foreach (var target in new string?[] { null, " ", "Missing", "Loading" })
        Check(await coordinator.RequestAsync(target) == RequestResult.InvalidDestination, "invalid result");
    Check(backend.Calls.Count == 0 && coordinator.State == TransitionState.Idle
        && coordinator.Destination == null && coordinator.LastFailure == null, "no side effects");
});
await Test("missing intermediary rejects before loading", async () =>
{
    var backend = new FakeBackend { MissingIntermediary = true };
    var coordinator = new TransitionCoordinator(backend, "Loading");
    Check(await coordinator.RequestAsync("Course") == RequestResult.InvalidDestination, "unavailable");
    Check(backend.Calls.Count == 0 && coordinator.State == TransitionState.Idle
        && coordinator.Destination == null && coordinator.LastFailure == null, "no loads");
    backend.MissingIntermediary = false;
    Check(await coordinator.RequestAsync("Course") == RequestResult.Completed, "ownership reusable");
});
await Test("overlap rejected during both asynchronous phases", async () =>
{
    var first = new TaskCompletionSource();
    var second = new TaskCompletionSource();
    var destinationStarted = new TaskCompletionSource();
    var backend = new FakeBackend { Load = scene =>
    {
        if (scene == "Loading") return first.Task;
        destinationStarted.SetResult();
        return second.Task;
    } };
    var coordinator = new TransitionCoordinator(backend, "Loading");
    var accepted = coordinator.RequestAsync("Course");
    Check(coordinator.State == TransitionState.LoadingIntermediary, "first phase");
    Check(await coordinator.RequestAsync("Course") == RequestResult.Busy, "first overlap");
    first.SetResult();
    await destinationStarted.Task;
    Check(coordinator.State == TransitionState.LoadingDestination, "second phase");
    Check(await coordinator.RequestAsync("Course", false) == RequestResult.Busy, "second overlap");
    second.SetResult();
    Check(await accepted == RequestResult.Completed && backend.Calls.Count == 2, "one owner");
});
await Test("intermediary failure prevents destination and allows retry", async () =>
{
    var backend = new FakeBackend { Load = _ => Task.FromException(new IOException("synthetic")) };
    var coordinator = new TransitionCoordinator(backend, "Loading");
    Check(await coordinator.RequestAsync("Course") == RequestResult.Failed, "failure");
    Check(backend.Calls.Count == 1 && coordinator.LastFailure is IOException, "observable failure");
    backend.Load = _ => Task.CompletedTask;
    Check(await coordinator.RequestAsync("Course") == RequestResult.Completed, "retry");
    Check(coordinator.LastFailure == null, "old error cleared");
});
await Test("destination asynchronous failure releases ownership", async () =>
{
    var backend = new FakeBackend { Load = scene => scene == "Course"
        ? Task.FromException(new IOException("synthetic")) : Task.CompletedTask };
    var coordinator = new TransitionCoordinator(backend, "Loading");
    Check(await coordinator.RequestAsync("Course") == RequestResult.Failed, "failed result");
    Check(coordinator.State == TransitionState.Idle && coordinator.Destination == null, "release");
});
await Test("validation exception releases ownership", async () =>
{
    var backend = new FakeBackend { FailValidation = true };
    var coordinator = new TransitionCoordinator(backend, "Loading");
    Check(await coordinator.RequestAsync("Course") == RequestResult.Failed, "validation failure");
    Check(coordinator.State == TransitionState.Idle && coordinator.Destination == null
        && coordinator.LastFailure is IOException && backend.Calls.Count == 0, "release");
    backend.FailValidation = false;
    Check(await coordinator.RequestAsync("Course") == RequestResult.Completed, "validation retry");
    Check(coordinator.LastFailure == null, "failure cleared on admission");
});
await Test("unavailable destination releases ownership before a valid retry", async () =>
{
    var backend = new FakeBackend();
    var coordinator = new TransitionCoordinator(backend, "Loading");
    Check(await coordinator.RequestAsync("Missing") == RequestResult.InvalidDestination, "invalid");
    Check(coordinator.State == TransitionState.Idle && coordinator.Destination == null
        && coordinator.LastFailure == null && backend.Calls.Count == 0, "clean rejection");
    Check(await coordinator.RequestAsync("Course") == RequestResult.Completed, "valid retry");
});
await Test("synchronous load exception releases ownership and retains diagnostic", async () =>
{
    var failure = new IOException("synthetic synchronous failure");
    var backend = new FakeBackend { Load = _ => throw failure };
    var coordinator = new TransitionCoordinator(backend, "Loading");
    Check(await coordinator.RequestAsync("Course", false) == RequestResult.Failed, "sync failure");
    Check(coordinator.State == TransitionState.Idle && coordinator.Destination == null
        && ReferenceEquals(coordinator.LastFailure, failure), "release and identity");
    Check(await coordinator.RequestAsync(" ") == RequestResult.InvalidDestination, "malformed retry");
    Check(ReferenceEquals(coordinator.LastFailure, failure), "rejection preserves diagnostic");
    backend.Load = _ => Task.CompletedTask;
    Check(await coordinator.RequestAsync("Course", false) == RequestResult.Completed
        && coordinator.LastFailure == null, "next admission clears diagnostic");
});
await Test("deferred failure retains ownership until settlement", async () =>
{
    var pending = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    var failure = new IOException("synthetic deferred failure");
    var backend = new FakeBackend { Load = _ => pending.Task };
    var coordinator = new TransitionCoordinator(backend, "Loading");
    var accepted = coordinator.RequestAsync("Course", false);
    Check(!accepted.IsCompleted && coordinator.Destination == "Course", "pending owner");
    Check(await coordinator.RequestAsync("Missing") == RequestResult.Busy, "busy takes precedence");
    Check(coordinator.State == TransitionState.LoadingDestination
        && coordinator.Destination == "Course" && backend.Calls.Count == 1, "owner undisturbed");
    pending.SetException(failure);
    Check(await accepted == RequestResult.Failed, "deferred failure");
    Check(coordinator.State == TransitionState.Idle && coordinator.Destination == null
        && ReferenceEquals(coordinator.LastFailure, failure), "settled release");
});
await Test("reentrant availability callback cannot acquire transition ownership", async () =>
{
    var backend = new FakeBackend();
    var coordinator = new TransitionCoordinator(backend, "Loading");
    backend.OnValidation = _ =>
    {
        var nested = coordinator.RequestAsync("Course", false);
        Check(nested.IsCompletedSuccessfully && nested.Result == RequestResult.Busy, "reentrant guard");
        Check(coordinator.Destination == "Course", "original destination preserved");
    };
    Check(await coordinator.RequestAsync("Course") == RequestResult.Completed, "outer completes");
    Check(backend.Calls.SequenceEqual(new[] { "Loading", "Course" })
        && coordinator.State == TransitionState.Idle && coordinator.Destination == null, "one owner");
});
Console.WriteLine($"{passed} contract checks passed.");

sealed class FakeBackend : ISceneBackend
{
    public List<string> Calls { get; } = new();
    public Func<string, Task> Load { get; set; } = _ => Task.CompletedTask;
    public bool MissingIntermediary { get; set; }
    public bool FailValidation { get; set; }
    public Action<string>? OnValidation { get; set; }
    public bool IsAvailable(string scene)
    {
        OnValidation?.Invoke(scene);
        if (FailValidation) throw new IOException("synthetic validation failure");
        return scene == "Course" || (scene == "Loading" && !MissingIntermediary);
    }
    public Task LoadSingleAsync(string scene)
    {
        Calls.Add(scene);
        return Load(scene);
    }
}
