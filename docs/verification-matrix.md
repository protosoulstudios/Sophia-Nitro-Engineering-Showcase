# Verification matrix

[Portfolio](../README.md) · [Coverage](coverage.md) · [Demonstration plan](demonstration-plan.md)

The integration review reran all three independent models: **41 checks passed** after direct Roslyn compilation with nullable analysis and warnings as errors. These are executable contract checks, not Unity tests. Ordinary SDK project runs remain blocked by NuGet-Migrations initialization. No production runtime batch below has been executed **during the showcase preparation**. Separately, the developer reports that the delivered v1.0.0 game passed Unity/Android release QA, including a full physical-tablet playthrough. Those prior observations are not new independent showcase test results.

## Evidence already available

| Layer | Status | What it supports |
| --- | --- | --- |
| Transition model | 12 checks passed | Request ordering, guards, validation, diagnostic lifetime and managed ownership cleanup under the declared backend contract |
| Checkpoint model | 12 checks passed | Fallback, value snapshots, replacement, supported state reset, stale-session rejection and reentrant recovery handling |
| Persistence model | 17 checks passed | Defaults, schema decisions, normalization, backup policy, injected partial writes and reset scope under a synthetic storage contract |
| Production source inspection | Inspected at the reviewed baseline | Responsibilities and code paths described in each case study; exact revision/evidence retained privately |
| Developer's historical game QA | Owner-reported and recorded in the private Master Development Brain | Completed Android tablet playthrough, progression/rewards, navigation, checkpoint/respawn, pause/settings and signed APK smoke test before v1.0.0 delivery; original session artifacts were not rerun in this showcase task |
| Production EditMode tests | Source inspected; execution/discovery unverified during showcase work | Declared general logic cases; not newly verified production transition, checkpoint or fault-recovery coverage |
| New Unity Editor / Play Mode checks | Not run in showcase environment | No fresh, agent-collected scene-binding, physics, actual loading or JSON behavior result; does not invalidate past owner testing |
| New Android device checks | Not run in showcase environment | No new agent-observed touch, lifecycle, PlayerPrefs durability, sustained performance or store-readiness evidence; historical owner-reported release QA is separate |

## Optional focused re-verification — not rerun during showcase work

These batches are a reference **only if a new behavior, changed build, reproduced defect or stronger technical claim requires fresh evidence**. The completed game's existing release QA should not be repeated merely because Codex did not have access to Unity. If a targeted check is justified, run it in an authorized development checkout with isolated test saves. This plan does not authorize production edits or creation of new test harnesses.

| Batch | Optional procedure | Editor evidence if specifically rerun | Android evidence if specifically rerun |
| --- | --- | --- | --- |
| V1 — Navigation and lifetime | Bootstrap → menu → selection → Loading → course; repeat input during both load phases; restart and return to menu | One accepted transition; persistent runner survives outgoing scene; final state and player/UI/service counts; record rejected requests | Repeat using touch/Back and pause/resume; no stuck input, time scale or duplicate service/player behavior |
| V2 — Checkpoint recovery | On each character, activate checkpoint, fall twice, compare fall-zone and delayed-hazard recovery; restart | Recorded position/yaw, motion/input recovery, checkpoint feedback and cooldown behavior; restart resets scene-local destination | Repeat representative course/hazards with touch and verify character/camera presentation |
| V3 — Persistence and reset | Set preferences, complete controlled course, relaunch; cancel then confirm adventure reset | Values and actual applied settings restore; earned rewards/unlocks persist; character/settings survive scoped reset | Relaunch and background/foreground observations on named device/OS; treat uninstall/clear-data as separate unsupported retention scenarios |
| V4 — Controlled fault cases | Use synthetic scenes/saves for invalid scene names, interruption, early checkpoint activation and malformed saves | Capture actual outcomes around state reset versus cancellation, manager Start ordering, fallback and write/copy/delete/move failures | After Editor characterization, test approved pause/termination/update scenarios in a disposable profile; do not corrupt real saves |

For each batch record source commit, Unity version, build configuration, device/OS where applicable, fixture/setup, expected outcome, actual observation, pass/fail/unrun status and retained logs/media. Record unexpected behavior as a finding before changing claims. Timing or performance claims require separate measurement methodology; a successful clip is not a benchmark.

## Claims still unverified

The production cancellation/reset overlap risk, early checkpoint activation ordering and corrupt-primary backup replacement risk are source-inspected concerns, not confirmed gameplay failures. The independent safeguards are not evidence that production implements them. Unity synchronization-context survival, physics/collider restoration, JsonUtility legacy-field defaults, PlayerPrefs flush behavior and actual filesystem interruption recovery all require direct verification.

A source/model-only portfolio can accurately retain these limitations and defer footage. Gameplay clips can illustrate the previously completed game without repeating full release QA; any stronger internal or failure-recovery claim needs its own targeted evidence. Publication requires separately approved repository content, reuse terms and media rights where applicable.
