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

## Scene transition example

The [independent demonstration](showcases/scene-transition-management.md) now has twelve executed .NET contract checks, including cleanup/retry and reentrant validation scenarios. These results apply only to the newly authored example, not production Unity behavior. Public code rights and Unity integration remain pending review.
