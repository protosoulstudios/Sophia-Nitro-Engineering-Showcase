# Checkpoint and respawn demonstration

Independently authored demonstration, not copied production code or a Unity-ready component. No production assets or dependencies are included. Published for professional code inspection with owner approval; no general public reuse or redistribution license is granted.

- `CheckpointAuthority.cs`: immutable snapshots, scene-session ownership, fallback, activation and recovery boundary.
- `PlayerRecoveryState.cs`: data-only implementation of explicitly supported pose/motion/input recovery.
- `Program.cs`: twelve executable contract checks using real sample objects and fault-injecting targets.

With .NET 8 SDK installed, the ordinary reproduction command from the repository root is:

```sh
dotnet run --project samples/checkpoint-respawn/CheckpointRespawn.csproj --configuration Release
```

Expected final output: `12 checkpoint contract checks passed.` Failed assertions exit nonzero. This ordinary SDK command was attempted here but failed during NuGet-Migrations mutex initialization. Actual successful compilation used SDK 8.0.426 Roslyn directly against .NET 8 reference assemblies with nullable checks and warnings as errors, then executed the assembly through the .NET runtime. All twelve checks passed. This does not establish an SDK project build, Unity compilation or physical gameplay results.

The authority is synchronous and assumes serialized owner access. A scene adapter must supply/reset its session, decide whether an actor is available and implement actual teleport/physics recovery. Snapshot validation is numeric, not a geometry or registration lookup. No cooldown, cross-thread safety, health system, rollback, moving-platform following, save persistence, event bus or engine adapter is included.

See the [case study](../../docs/showcases/checkpoint-and-respawn.md) for inspected production behavior, sample differences and optional targeted verification of deeper claims. The developer's previously completed Android release QA remains a separate evidence category.
