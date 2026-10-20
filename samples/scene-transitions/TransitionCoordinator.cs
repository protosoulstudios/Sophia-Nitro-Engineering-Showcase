// Independently authored portfolio demonstration; not extracted production code.
// Private review candidate. Redistribution approval and reuse terms are pending.
namespace Showcase.SceneTransitions;

public enum TransitionState { Idle, LoadingIntermediary, LoadingDestination }
public enum RequestResult { Completed, InvalidDestination, Busy, Failed }

// Implementations define scene availability and complete only after scene activation.
// This sample deliberately has no Unity dependency or claimed Unity adapter.
public interface ISceneBackend
{
    bool IsAvailable(string scene);
    Task LoadSingleAsync(string scene);
}

public sealed class TransitionCoordinator
{
    private readonly ISceneBackend backend;
    private readonly string intermediary;

    public TransitionState State { get; private set; }
    public string? Destination { get; private set; }
    public Exception? LastFailure { get; private set; }

    public TransitionCoordinator(ISceneBackend backend, string intermediary)
    {
        this.backend = backend ?? throw new ArgumentNullException(nameof(backend));
        if (string.IsNullOrWhiteSpace(intermediary))
            throw new ArgumentException("An intermediary scene is required.", nameof(intermediary));
        this.intermediary = intermediary;
    }

    // Call on one owning thread/context, as Unity UI normally would. Not thread-safe.
    // No cancellation/reset API: a request stays busy until the backend settles.
    // A backend that never completes will keep it busy; no timeout is claimed.
    public async Task<RequestResult> RequestAsync(string? destination, bool viaIntermediary = true)
    {
        if (State != TransitionState.Idle)
            return RequestResult.Busy;
        if (string.IsNullOrWhiteSpace(destination) || destination == intermediary)
            return RequestResult.InvalidDestination;

        // Reserve ownership before invoking backend code, including validation.
        State = viaIntermediary ? TransitionState.LoadingIntermediary : TransitionState.LoadingDestination;
        Destination = destination;
        LastFailure = null;
        try
        {
            if (!backend.IsAvailable(destination) ||
                (viaIntermediary && !backend.IsAvailable(intermediary)))
                return RequestResult.InvalidDestination;

            if (viaIntermediary)
                await backend.LoadSingleAsync(intermediary);

            State = TransitionState.LoadingDestination;
            await backend.LoadSingleAsync(destination);
            return RequestResult.Completed;
        }
        catch (Exception failure)
        {
            // Backend errors become an observable result. A partially changed
            // scene is not rolled back; callers must choose a recovery screen.
            LastFailure = failure;
            return RequestResult.Failed;
        }
        finally
        {
            Destination = null;
            State = TransitionState.Idle;
        }
    }
}
