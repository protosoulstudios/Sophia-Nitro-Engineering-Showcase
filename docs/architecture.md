# Architecture

ARCH-01: The application bootstrap composes settings, save, audio, frame-rate, lifecycle, and scene-loading services. The scene-loading service receives the bootstrap component as its coroutine runner, keeping transition execution with the persistent service host rather than an outgoing scene controller.

ARCH-02: Startup waits for initialization and requests Main Menu directly. The transition service also supports an intermediary Loading scene before a pending target. It validates scene availability and rejects a new transition while busy. Reloading the active scene uses a direct service path.

The transition states are Idle, LoadingIntermediaryScene, and LoadingTargetScene. The method named CancelTransition resets bookkeeping; it does not cancel a Unity asynchronous operation or stop an already running coroutine. This distinction matters for future interruption testing.

The scene-local game manager owns the active checkpoint and routes recovery to a resolved player controller. Persistence lifetimes are described in [gameplay and persistence](gameplay-and-persistence.md). These are source-inspected responsibilities, not a demonstrated runtime trace.
