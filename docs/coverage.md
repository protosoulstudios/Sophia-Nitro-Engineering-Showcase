# Evidence and coverage

| Claim | Source-inspected subject |
| --- | --- |
| ARCH-01 | Service composition and coroutine runner ownership |
| ARCH-02 | Startup, transition states and request guards |
| GAME-01 | Checkpoint lifetime and typed respawn delegation |
| GAME-02 | Completion guard, unlocks and replay request |
| DATA-01 | Progression keys, bounds and reset scope |
| DATA-02 | Temporary write, backup and load fallback |
| MOBILE-01 | Frame-rate target normalization |
| VERIFY-01 | Declared test methods and cases |

These claims describe inspected implementation. No Unity runtime, device, rendering, performance, or store verification is asserted. Private review records retain the source revision and evidence mapping; they are deliberately outside this showcase.

This independently authored showcase contains no production assets, production source excerpts, game configuration, or private audit records. Asset redistribution and project publication rights are separate questions. Publication and any reuse terms require owner review. Future updates should follow verified engineering milestones rather than mirror the private project automatically.

## Integrated verification

| Independent example | Executed checks | Principal scope |
| --- | --- | --- |
| [Scene transitions](showcases/scene-transition-management.md) | 12 passed | Request/state ownership, validation, failures and reentrant calls |
| [Checkpoint and respawn](showcases/checkpoint-and-respawn.md) | 12 passed | Pose snapshots, fallback, supported state recovery, reset and stale sessions |
| [Save and persistence](showcases/save-and-persistence.md) | 17 passed | Version/default/validation policy, backup fallback, partial writes and reset scope |

All 41 checks were rerun for the integration candidate with direct Roslyn compilation and .NET runtime execution. Sample C# behavior is unchanged. Nullable analysis and warnings as errors were enabled. Ordinary SDK commands remain blocked by NuGet-Migrations initialization; an ordinary project build is not claimed.

The [verification matrix](verification-matrix.md) separates these results from production source inspection, planned Editor/Play Mode work and Android tests. The [demonstration plan](demonstration-plan.md) identifies proposed footage, prerequisites and limits. Neither runtime batches nor media capture have been completed. The samples' safeguards must not be attributed to production without source/runtime evidence.
