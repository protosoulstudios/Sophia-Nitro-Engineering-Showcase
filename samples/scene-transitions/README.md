# Scene transition contract demonstration

Independently authored for this portfolio task. Not copied production code and not a runnable Unity scene. Private review candidate; public redistribution and reuse terms remain pending. No license grant is supplied.

`TransitionCoordinator.cs` contains the coordinator and backend contract. `Program.cs` supplies a synthetic backend and eight executable checks. No external packages, game assets, configuration or production scripts are required.

With an installed .NET 8 SDK, the normal reproduction command from the repository root is:

```sh
dotnet run --project samples/scene-transitions/SceneTransitions.csproj --configuration Release --artifacts-path /tmp/scene-transition-build
```

Expected output ends with `8 contract checks passed.` An assertion failure terminates with a nonzero exit. The normal SDK project command above was not verified in this environment: CLI initialization hit filesystem/mutex restrictions. Actual verification used the SDK's Roslyn compiler directly with .NET 8 reference assemblies, nullable analysis and warnings as errors, then executed the compiled program with the .NET runtime. All eight checks passed; this is not a claimed project build or Unity test result.

The fake backend intentionally controls when each phase completes so overlap checks do not depend on delays. The coordinator assumes calls and continuations on a single owning context. Production Unity needs a separately reviewed adapter and persistent owner; the example does not create either. It has no cancellation, progress, timeout or rollback mechanism. It cannot guarantee recovery from a backend operation that never settles.

Read the [case study](../../docs/showcases/scene-transition-management.md) for production differences and the pending Unity/device verification plan.
