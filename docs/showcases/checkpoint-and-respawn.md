# Checkpoint and respawn

[Portfolio](../../README.md) · [Verification matrix](../verification-matrix.md) · [Demonstration plan](../demonstration-plan.md)

## Engineering challenge

A fall should cost the player progress from the last safe point, not leave the character carrying the motion that caused the failure. The engineering challenge is to give checkpoint ownership a clear lifetime and recover the correct player without making every hazard understand movement internals.

This case study examines the game's scene-local checkpoint authority and typed respawn boundary. The [independent C# authority](../../samples/checkpoint-respawn/CheckpointAuthority.cs), [player recovery model](../../samples/checkpoint-respawn/PlayerRecoveryState.cs) and [executable checks](../../samples/checkpoint-respawn/Program.cs) let a client inspect responsibility separation and recovery edge cases. They are newly authored illustrations, not extracted production scripts or a Unity-ready replacement. Rights and public publication remain pending review.

## Production implementation inspected

On October 20, 2026, remote production main still matched the previously reviewed baseline. Private evidence records retain the exact source commit and hashes. No newer production main revision was observed.

| Component | Source-inspected responsibility |
| --- | --- |
| Checkpoint trigger | Resolve a player controller from the entering collider's parents; record its own position/rotation through the manager; request checkpoint feedback |
| Scene-local manager | Store checkpoint position/rotation values, bind an active player, delegate recovery to the supplied player controller |
| Player spawner | Select Sophia or Nitro prefab, instantiate at an initial spawn, resolve controller root, bind camera and manager |
| Player controller | Reposition upright at the recovery pose and clear its owned input/motion state |
| Fall zone | Resolve the detected controller and request manager recovery, with an instance-local unscaled cooldown |
| Hazard and physics utility | Resolve player root, clear additional motion, try typed manager recovery, then use configured/tagged fallback when necessary |

The manager is a scene-local singleton, not a persistent checkpoint service. Its Start path uses a configured default spawn if present; otherwise it captures the player pose when no destination exists. Registering a player later does not overwrite an already established destination. A recovery request without a destination captures the target's current pose as fallback. That emergency fallback is not proof the position is safe.

Checkpoint activation stores values rather than retaining the checkpoint Transform. The trigger supports one-time activation, but sets its activated flag before checking manager availability. If triggered before a manager exists, a one-shot checkpoint may be consumed without registration. Manager Start also unconditionally applies a configured default spawn, so early checkpoint activation versus Start order needs runtime verification. These are source-inspected timing risks, not reproduced gameplay failures.

## Recovering the player, not just its position

The manager passes the already resolved controller to its recovery API. That avoids selecting an unrelated player through another broad lookup at the failure boundary. It checks that the target belongs to a valid scene, but does not enforce equality with the manager's scene or a scene-generation token.

The player controller temporarily disables its CharacterController, applies position and yaw-only rotation, and restores the controller's previous enabled state. It clears planar and vertical velocity, external horizontal velocity, queued displacement, move/turn inputs, pending yaw, jump and ability flags, temporary speed multiplier/timer, and its grounded flag. Grounding is recalculated by subsequent movement. This is not a general restore of health, camera state, animation state, rewards, or every attached component.

Both inspected Sophia and Nitro prefabs reference the shared controller script; the spawner selects the requested prefab with an alternative fallback when an assignment is missing. Different rigs and collider/camera bindings still need runtime checks. The animation bridge observes controller movement rather than owning locomotion.

A hazard path clears the separate external-motion receiver and applicable Rigidbody velocities through the utility before manager recovery. The fall-zone path goes directly to the manager/controller. Consequently, clearing all attached physics state cannot be assumed identical across those routes. No production correction was made here.

## Repeated failures and scene lifetime

The fall zone uses a 0.5-second unscaled cooldown per zone. Hazards use a per-root cooldown and a delayed coroutine based on scaled game time. Their cooldowns do not provide a central guard across every hazard. Hazard disable clears the cooldown dictionary; pending work and disable/re-enable timing need Unity verification. The manager itself performs synchronous recovery without a shared cooldown.

These interactions use Unity callbacks and direct component/service calls; the inspected checkpoint and manager do not register a checkpoint event bus or own subscription cleanup. The spawner's early execution order helps initial binding, while Awake/Start, destruction and delayed hazard callbacks remain Unity lifecycle dependencies. Manager destruction clears its singleton. Scene reload creates a new manager/checkpoint state; no checkpoint position is written to a save store. The spawner method named RespawnSelectedPlayer recreates the initial player and bindings; it is distinct from checkpoint recovery of the existing controller.

## C# demonstration and design decisions

The example is deliberately synchronous and gameplay-focused. It does not repeat the asynchronous scene-transition example.

- `CheckpointSnapshot` is an immutable value containing an identifier, pose and scene-session token. Later caller changes cannot mutate the accepted snapshot. It does not track moving platforms or verify collision-safe geometry.
- `CheckpointAuthority` owns one scene session, an explicit initial fallback and the latest valid checkpoint. Invalid activation preserves the current checkpoint. A fresh token on reset rejects old checkpoint snapshots and delayed recovery requests.
- `IRespawnTarget` owns availability and synchronous recovery. The authority does not retain a player reference or manipulate player internals. `PlayerRecoveryState` illustrates position/yaw restoration and the explicitly supported movement/input reset only.
- A guard reserves recovery ownership before target getters/callbacks. Nested respawn, activation or reset cannot interfere mid-recovery. Sequential requests are intentionally permitted and clean state each time; there is no frame deduplication, timer or multiplayer policy.
- Target exceptions produce a Failed result and retained diagnostic, then release the guard. They may leave partial player changes. There is no rollback. The next admitted non-null current-scene request clears the diagnostic; early rejection preserves it.

All access is serialized by the caller; this is not thread-safe. The caller must reset the authority or replace it at a real scene boundary. Tokens distinguish sessions, not trusted users. Validity means finite coordinates/yaw and a nonblank identifier, not an available Unity object, a registered level checkpoint or a safe navigable location. Reset rejects invalid poses and cannot happen inside a recovery callback.

Production has no corresponding token model, central reentrant guard or structured recovery result. Conversely, the example does not reproduce production cooldowns, feedback, reward tuning, prefab selection, camera binding or CharacterController movement. Production excerpt rights were not cleared; no production code or assets were copied.

## Executable verification

The twelve independent contract checks passed after compilation using SDK 8.0.426's Roslyn compiler with .NET 8 references, nullable analysis and warnings as errors. Checks cover initial fallback, activation snapshot isolation, replacement, invalid activation, repeated motion/input recovery, stale work after reset, invalid reset preservation, unavailable targets, nested requests, partial failure/retry, availability exceptions and cross-authority rejection.

The ordinary SDK run command was attempted but failed during NuGet-Migrations mutex initialization. Direct compiler/runtime execution succeeded. The [sample instructions](../../samples/checkpoint-respawn/README.md) distinguish that result from a verified SDK project build. Private revision records tie the actual output and file hashes to the committed candidate.

The inspected production test file contains general logic checks, not checkpoint/respawn tests. A project C# filename scan found no additional test files, and no assembly definitions were found. Test discovery and execution in Unity were not verified. No Editor, Play Mode, Android, scene-reload or physics test was run here.

## Optional targeted verification for deeper claims

The developer previously verified normal checkpoint, FallZone and hazard recovery while testing the delivered Android game. The table below describes **additional edge-case observations**, not an unfinished general-release QA checklist. Perform these only for a new claim, reproduced issue or changed build; routine gameplay footage needs no blanket recheck.

| Scenario | Evidence to collect if specifically testing a stronger claim |
| --- | --- |
| Initial spawn | Each character spawns once; default pose, controller, camera and manager bindings are correct |
| Checkpoint activation | Trigger receives the correct collider/controller; destination and feedback update once as configured |
| Fall and repeated failure | Recovery uses the activated position/yaw; no residual impulse, jump or held ability causes immediate re-failure |
| Hazard differences | Compare direct fall-zone recovery with delayed hazard recovery, including optional external-motion receiver |
| Overlapping contacts | Multiple colliders/zones and disable/re-enable do not create unwanted repeated recovery; record actual cooldown behavior |
| Ordering and availability | Exercise late manager/player binding, checkpoint activation before manager Start, missing fallback and unavailable target |
| Scene restart | Fresh manager and checkpoint flags return to initial state; delayed old work does not affect a new scene |

Use controlled synthetic scenes for failure cases and record exact source revision, Unity version, setup, expected and actual outcomes. These are planned checks, not passed tests.

## Proposed demonstration footage

When the owner is ready to film, a short clip can show initial spawn → checkpoint activation → fall → checkpoint recovery → second fall/recovery → scene restart returning to initial state. The client's general public-display permission is already owner-reported; review distinct media/asset terms for the selected clip. Compare Sophia and Nitro if collider/rig behavior differs. Review character and purchased-environment depiction rights, audio and confidential editor/log content before recording. No footage was generated or published.

The integration review covered the code, provenance narrative and explanation. Public release requires the final content/rights decision and explicit owner approval. Gameplay footage can be added later; targeted Unity checks remain optional when new technical claims warrant them. The [persistence example](save-and-persistence.md) separately examines saved data and reset scope.
