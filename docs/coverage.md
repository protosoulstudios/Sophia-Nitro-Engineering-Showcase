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

These claims describe inspected implementation; they do **not** claim new independently executed Unity, rendering, performance, or store tests. Separately, the developer reports completing Unity/Android release QA, including a full physical-tablet playthrough and delivery of the v1.0.0 APK/AAB. Those prior observations are historical owner-reported results, not new agent-executed tests. Private review records retain source revisions and claim mappings; they remain outside this showcase.

This independently authored showcase contains no production assets, production source excerpts, game configuration, or private audit records. Asset redistribution and project publication rights are separate questions. Publication and any reuse terms require owner review. Future updates should follow verified engineering milestones rather than mirror the private project automatically.

## Integrated verification

| Independent example | Executed checks | Principal scope |
| --- | --- | --- |
| [Scene transitions](showcases/scene-transition-management.md) | 12 passed | Request/state ownership, validation, failures and reentrant calls |
| [Checkpoint and respawn](showcases/checkpoint-and-respawn.md) | 12 passed | Pose snapshots, fallback, supported state recovery, reset and stale sessions |
| [Save and persistence](showcases/save-and-persistence.md) | 17 passed | Version/default/validation policy, backup fallback, partial writes and reset scope |

All 41 checks were rerun for the integration candidate with direct Roslyn compilation and .NET runtime execution. Sample C# behavior is unchanged. Nullable analysis and warnings as errors were enabled. Ordinary SDK commands remain blocked by NuGet-Migrations initialization; an ordinary project build is not claimed.

The [verification matrix](verification-matrix.md) separates these results from production source inspection, planned Editor/Play Mode work and Android tests. The [demonstration plan](demonstration-plan.md) identifies proposed footage, prerequisites and limits. No **new** V1–V4 runtime batches were run during showcase preparation, and no portfolio media has been captured. This is separate from the developer's already completed release QA. Future focused tests are optional when a changed build or specific new claim justifies them. The samples' safeguards must not be attributed to production without source/runtime evidence.
