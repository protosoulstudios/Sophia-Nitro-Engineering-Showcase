# Sophia & Nitro: Sweetland Adventures

**Unity gameplay and application engineering — Protosoul Studios**

Sophia & Nitro is a Unity Android obstacle-course adventure with two playable characters, three courses, checkpoints and unlockable movement rewards. This portfolio examines the engineering behind scene navigation, player recovery and persistent data.

The inspected Protosoul Studios engineering scope includes persistent application services, scene-local gameplay components, character spawning/binding, progression and settings. Three independently authored C# demonstrations make the design principles inspectable without transferring the proprietary Unity project.

## Explore the engineering

| Showcase | What to evaluate | Inspect the evidence |
| --- | --- | --- |
| **[Scene Transition Management](docs/showcases/scene-transition-management.md)** | One transition owner, competing-request guards, asynchronous state and observable failures | [C# coordinator](samples/scene-transitions/TransitionCoordinator.cs) · [12 executable checks](samples/scene-transitions/Program.cs) · [Run instructions](samples/scene-transitions/README.md) |
| **[Checkpoint and Respawn](docs/showcases/checkpoint-and-respawn.md)** | Checkpoint authority, component boundaries, player-state recovery and stale scene sessions | [C# authority](samples/checkpoint-respawn/CheckpointAuthority.cs) · [Recovery model](samples/checkpoint-respawn/PlayerRecoveryState.cs) · [12 executable checks](samples/checkpoint-respawn/Program.cs) · [Run instructions](samples/checkpoint-respawn/README.md) |
| **[Save and Persistence](docs/showcases/save-and-persistence.md)** | Data ownership, validation, version decisions, backup recovery and narrowly scoped reset | [C# settings store](samples/save-persistence/SettingsStore.cs) · [Progression model](samples/save-persistence/AdventurePreferences.cs) · [17 executable checks](samples/save-persistence/Program.cs) · [Run instructions](samples/save-persistence/README.md) |

Each case study connects the inspected game implementation to an independent example, explains the tradeoffs, and identifies the remaining runtime checks. The samples illustrate engineering decisions; they are not extracted production scripts or drop-in Unity replacements.

## Technical context

The reviewed game pins **Unity 6000.2.14f1**, **URP 17.2.0** and **Input System 1.16.0**, targeting **Android**. The standalone examples target **.NET 8** and use synthetic backends or state rather than game assets. No game build or production source is distributed here.

## Verification and next evidence

All **41 independent checks** passed in the integration review: 12 transition, 12 checkpoint and 17 persistence checks. Compilation used .NET SDK 8.0.426's Roslyn compiler with nullable analysis and warnings as errors, followed by direct .NET runtime execution. The ordinary SDK run commands remain blocked in this environment by a NuGet-Migrations initialization error; ordinary project builds are not claimed.

These results verify the independent models. Production descriptions are grounded in source inspection; Unity Editor, Play Mode, Android device behavior and gameplay footage remain unverified. The examples do not establish measured performance, crash-safe storage or production failure recovery.

See the **[verification matrix](docs/verification-matrix.md)** for executed evidence and efficient runtime test batches, and the **[demonstration plan](docs/demonstration-plan.md)** for proposed footage and its limits. No footage has been recorded for this portfolio.

## Further reading

[Architecture](docs/architecture.md) · [Gameplay and persistence](docs/gameplay-and-persistence.md) · [Engineering decisions](docs/decisions.md) · [Mobile policy](docs/mobile-and-validation.md) · [Evidence and coverage](docs/coverage.md) · [Draft history](CHANGELOG.md)

This is a private portfolio candidate for owner review. Public release and code reuse terms remain pending approval. No public license is granted, and no production assets, private review records or real save data are included.
