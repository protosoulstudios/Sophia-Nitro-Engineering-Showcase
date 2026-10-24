# Scene transition contract demonstration

Independently authored for this portfolio task. Not copied production code and not a runnable Unity scene. Included in the owner-approved public engineering showcase for professional code inspection; no general reuse or redistribution license is granted.

`TransitionCoordinator.cs` contains the coordinator and backend contract. `Program.cs` supplies a synthetic backend and twelve executable checks. No external packages, game assets, configuration or production scripts are required.

With an installed .NET 8 SDK, the normal reproduction command from the repository root is:

```sh
dotnet run --project samples/scene-transitions/SceneTransitions.csproj --configuration Release
```

Expected output ends with `12 contract checks passed.` An assertion failure terminates with a nonzero exit. The normal SDK project command above was not verified in this environment: the refinement attempt failed during CLI initialization with a NuGet-Migrations mutex error. Actual verification used the SDK's Roslyn compiler directly with .NET 8 reference assemblies, nullable analysis and warnings as errors, then executed the compiled program with the .NET runtime. All twelve checks passed; this is not a claimed project build or Unity test result.

The fake backend intentionally controls when each phase completes so overlap checks do not depend on delays. The coordinator assumes serialized calls, property reads and continuations on one owning context; it does not enforce affinity. Ordinary await captures a present synchronization context, but console continuations may resume on a pool thread. The checks serialize access; they do not prove thread safety. Production Unity needs a separately reviewed adapter and persistent owner; the example does not create either. It has no cancellation, progress, timeout or rollback mechanism. It cannot guarantee recovery from a backend operation that never settles. Backend task settlement, including faults and cancellation, must mean underlying scene mutation has stopped; releasing managed ownership cannot cancel engine work. LastFailure describes the last admitted backend request and is not a per-call error history.

Read the [case study](../../docs/showcases/scene-transition-management.md) for production differences and optional targeted verification of stronger claims. Prior developer-reported game-release QA is distinct from these independent checks.
