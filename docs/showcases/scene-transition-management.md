# Scene transition management

Navigation input can arrive repeatedly while a scene is still loading, and the screen that requested a transition may be destroyed before it finishes. A single transition owner keeps that work alive and prevents competing requests from starting another load. Explicit failure results and retained exception details give the caller a useful debugging boundary instead of an unexplained stuck screen.

This example lets a client inspect how the developer separates coordination from engine calls, defines state and failure contracts, and verifies edge cases. Its checks demonstrate those skills in an independent C# model; they do not establish production Unity reliability or device performance.

Start with the [C# coordinator](../../samples/scene-transitions/TransitionCoordinator.cs), then inspect the [executable contract checks](../../samples/scene-transitions/Program.cs). This is an independently authored, runnable .NET demonstration inspired by inspected responsibilities in the game. It is not extracted production code, a Unity adapter, or evidence that the game passed these tests. Code and narrative remain private review candidates pending owner approval and reuse-rights review; no public license is granted.

## What the game implements

The reviewed production branch still points to the previously reviewed Unity baseline when checked on October 20, 2026. The bootstrap composes a scene-loading service and supplies its persistent coroutine host. The startup controller waits for initialization, then loads Main Menu directly. Menu and level selection set a pending course; character selection requests a transition through Loading. Pause can reload the active scene directly or return to Main Menu through Loading.

The service exposes requests for a direct load, a load through Loading, an active-scene reload, and beginning the pending target. It validates nonempty names and build availability, disallows Loading as its own final target, and rejects a new request while busy. Single-mode direct loads reject the already-active scene; explicit reload supports that case. A legacy template scene-name constant remains in the service; its presence does not make that scene a production destination.

| State | Responsibility and normal successor |
| --- | --- |
| Idle | Validate and accept one request; enter intermediary or target loading |
| LoadingIntermediaryScene | Load Loading in Single mode; its controller asks the service to start the pending target |
| LoadingTargetScene | Run target load, normalize progress, hold activation until ready, activate and await completion; reset to Idle |

Both asynchronous coroutines run on the bootstrap host. The loading screen is scene-local: it initiates the pending target and displays service progress. It does not own the target coroutine. Single-mode loads request replacement of the active scene set through Unity; the service has no custom streaming/unload queue. Direct loading also accepts a mode parameter, so additive lifetime behavior should not be inferred from the normal Single-mode route.

## Failure boundaries found in source

- Missing Loading, missing pending target, or a null load operation reaches a logged state-reset path. The loading screen attempts a Main Menu fallback when no pending target can begin.
- The method named CancelTransition resets bookkeeping and time scale. It does not stop coroutines or cancel Unity's asynchronous operation. Clearing state during an active load can therefore permit a new request while earlier work is still running. Old completion could reset newer state; this is a review risk, not a reproduced runtime failure.
- The production coroutines do not wrap engine calls in an exception recovery boundary and have no timeout. Availability validation is not proof that an operation completes. A missing persistent host or bootstrap teardown is not resolved by the busy guard.
- Progress reaching its activation threshold is not a frame-time measurement or proof that scene-local dependencies are ready. Startup and normal navigation must still be exercised in Unity.

## Why an independent example

A production excerpt would offer direct implementation fidelity, but authorship, contractual permission and redistribution terms for the source have not been independently cleared. It also relies on Unity coroutines, scene configuration and application logging. Copying it would transfer those dependencies without producing a self-contained verification target.

The new demonstration isolates coordination behind `ISceneBackend`. It reserves ownership before backend validation or loading, returns explicit request outcomes, and releases ownership in `finally`. Backend exceptions are observable through `LastFailure`. That diagnostic belongs to the last admitted backend request, not every caller: busy or malformed-name rejections preserve it; the next admitted request clears it. Callers should inspect the result and diagnostic before admitting another request. This is a proposed example design, not a claim that production already has those improvements.

| Demonstration choice | Boundary |
| --- | --- |
| Awaitable backend methods | Synthetic backend only; a Unity adapter must complete after activation on the correct context |
| One owning thread/context | Supports overlapping asynchronous calls on that context; not cross-thread synchronization |
| Direct or intermediary route | No scene reload policy, progress UI, gameplay binding, additive management or time-scale changes |
| No reset/cancel API | Busy remains until backend completion; hung operations remain busy |
| Explicit failure result | No rollback after a scene partially changes; caller chooses recovery |

Calls, property reads and continuations must be serialized by one owning context. In Unity, normal `await` captures a present synchronization context, but the example neither installs that context nor verifies thread affinity. A console app can resume on a pool thread: serial access is still required, and concurrent callers remain unsupported. A destroyed owner or lost Unity context is not handled here. Adding `ConfigureAwait(false)` would require redesigning the ownership contract, not merely a style change.

The backend must return a non-null task. Successful completion means activation has finished; a faulted or canceled task must mean no underlying operation is still changing scene state. A backend must not report cancellation merely because its caller stopped waiting. A canceled task becomes a Failed result in this demonstration; the coordinator issues no cancellation command.

The `finally` block releases managed bookkeeping only. It cannot stop an engine load. A timeout wrapper that returns while engine work continues would violate the backend contract and allow competing work. The example deliberately has no timeout or manual reset. If the backend never settles, it stays busy. If activation partially succeeds before failure, there is no rollback; recovery and scene-lifetime decisions belong to a future Unity integration. Catching backend exceptions is a bounded reporting mechanism, not a promise to recover from every process failure.

## Verification evidence

The refinement was compiled on October 20, 2026 using .NET SDK 8.0.426's Roslyn compiler against .NET 8 reference assemblies, with nullable analysis and warnings as errors. **All 12 executable contract checks passed.** The original eight scenarios remain, with stronger cleanup/retry assertions; four focused scenarios were added:

- Unavailable destination releases ownership and permits a valid retry.
- A synchronous backend exception releases ownership, preserves its diagnostic across a malformed-name rejection, and clears it on the next admission.
- A deferred failure holds ownership until settlement; a rejected request cannot disturb the accepted destination.
- A reentrant availability callback cannot acquire a second transition owner.

The ordinary documented `dotnet run` command was attempted again and failed during SDK initialization with a NuGet-Migrations mutex error. The compiler and runtime were invoked directly instead. This validates the committed C# inputs and executed checks, not an SDK project build or Unity compilation. Private revision records tie exact input hashes and output to the resulting showcase commit, retaining the earlier eight-check results separately. See [sample instructions](../../samples/scene-transitions/README.md).

The existing production EditMode file checks empty/whitespace scene names using the static name helper. It does not demonstrate availability, coroutine lifecycle, overlap, or interruption coverage. Those production tests were inspected, not executed. No Play Mode, Unity build, Android device or gameplay-media verification was performed here.

## Unity verification still required

| Scenario | Required observation in a controlled Editor test |
| --- | --- |
| Successful route | Bootstrap reaches menu; selected course loads through Loading once; state returns Idle |
| Invalid request | Empty, unavailable and Loading-as-target requests do not start unwanted loads |
| Overlap | Repeated input during both phases yields one accepted transition and one active player/UI set |
| Restart | Active scene reloads through the explicit reload API rather than duplicate direct request |
| Scene lifetime | Outgoing scene destruction leaves the bootstrap runner alive and completion observable |
| Failure/interruption | Missing Loading, fallback, reset during load and bootstrap destruction produce recorded outcomes; do not assume cancellation |
| Android | Repeat navigation and background/foreground on device; inspect time scale, input and service duplication |

Use synthetic scenes before interrupting real game transitions. Record source revision, Unity version, setup, expected and actual observations, and logs. Do not label these planned checks passed.

## Proposed demonstration clip

After permission clearance, capture a short menu → character selection → Loading → course sequence, followed by one restart. Pair it with a development-only state trace and a clearly labeled synthetic overlap test. Avoid credentials, editor paths and private logs. Clear rights to depict characters, purchased environments, audio and client work first. No clip has been produced.

The next portfolio step is owner review of this independent code and its relationship to production, followed by Unity integration verification. Checkpoint/respawn and persistence remain separate future examples.
