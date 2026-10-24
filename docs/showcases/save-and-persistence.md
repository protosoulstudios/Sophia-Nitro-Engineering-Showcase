# Save and persistence

[Portfolio](../../README.md) · [Verification matrix](../verification-matrix.md) · [Demonstration plan](../demonstration-plan.md)

## Engineering challenge

Players expect settings and earned progress to survive a restart, while Reset Adventure should not erase their character choice or preferences. The engineering problem is to give each data set an explicit owner, validate stored input, and make recovery behavior understandable when storage is missing, malformed or fails mid-write.

The [independent settings store](../../samples/save-persistence/SettingsStore.cs), [scoped progression example](../../samples/save-persistence/AdventurePreferences.cs) and [executable checks](../../samples/save-persistence/Program.cs) demonstrate data contracts, recovery decisions and partial-write testing. They are newly authored illustrations, not copied production code or a Unity persistence replacement. Code provenance, reuse terms and public publication remain subject to owner review.

## Production implementation inspected

On October 20, 2026, remote production main still matched the previously reviewed Unity source baseline. Private records retain the exact commit and inspected-file hashes. Source inspection establishes these responsibilities; it does not establish Android storage durability.

| Data | Owner and storage | Validation and reset boundary |
| --- | --- | --- |
| Character choice and pending level | Progression helper using PlayerPrefs strings | Unknown names normalize to supported defaults |
| Highest unlocked level | PlayerPrefs integer | Bounded to levels one through three; unlocking does not lower progress |
| Three rewards | Separate PlayerPrefs flags | Granted flags read as enabled only when equal to one |
| Audio volumes, quality, target FPS, haptics | Settings service and JSON save service | Volume/quality bounds and frame-rate policy applied after load and before save |
| Current checkpoint and completion guard | Scene-local gameplay state | Not saved as checkpoint coordinates or a persistent completion journal |

This separation permits a narrow adventure reset while keeping settings intact. It also creates multiple consistency boundaries: changing progression flags, preferences and JSON settings is not one transaction. The completion component guards repeated completion only within its scene instance, then writes reward and optional level unlock through separate calls. No cross-store migration, cloud synchronization, encryption or tamper resistance was established by the inspected implementation.

### Load, save and lifecycle flow

The bootstrap constructs the save service, loads settings, and applies audio, frame-rate and quality policy. The settings UI saves changes and saves after applying reset defaults. SettingsService.ResetToDefaults itself changes the in-memory object; persistence is a separate call. Application pause and quitting request settings saves, while focus/resume reapply relevant runtime policy. Those callbacks are opportunities to save, not guarantees of execution on abrupt Android termination.

The generic save service stores JSON under Unity's persistent data directory. It tries primary deserialization first and then backup on failure; the settings owner supplies defaults if neither loads. It clamps volumes and quality and normalizes exactly 30 FPS to 30, everything else to 60. It does not implement an explicit schema version/migration or separate finite-number check. A deserializable primary is accepted before settings normalization; backup fallback is not a semantic-validation election between two records.

The write sequence is temporary JSON → copy existing primary to backup → delete primary → move temporary to primary. Errors inside save are logged; save returns no success result. The delete/move interval is a failure window, so this is not guaranteed atomic replacement or power-loss durability. Construction creates the directory outside that save catch boundary. The generic filename helper appends a JSON suffix rather than enforcing a complete filename policy; the inspected settings caller uses a fixed name.

An additional source-inspected risk follows recovery: after a corrupt primary loads from a valid backup, the next save copies the existing corrupt primary over the backup before replacement. If replacement subsequently fails, the useful backup may be lost. This scenario was not reproduced in production; the independent sample tests an alternative policy without changing the game.

### Reset and schema boundaries

Reset Adventure removes progression, pending level and reward keys while preserving character selection and separately stored settings. The main-menu reset action requests confirmation first. Reward-only reset is narrower still. No global PlayerPrefs deletion is used in these inspected reset paths.

Production JSON has field defaults and settings clamping, but no explicit version negotiation or migration path. Adding fields requires Unity deserialization and old-save verification rather than assuming missing fields behave as desired. The existing test whose name mentions missing FPS data actually loads an absent settings file; it does not supply legacy JSON lacking an FPS field.

## C# demonstration and design decisions

The sample deliberately uses a small version-one settings record: volume, frame-rate target and haptics. A separate two-key progression model illustrates scoped level/reward writes. It is not the production key layout, full settings schema or a production migration.

| Decision | Observable behavior |
| --- | --- |
| Explicit integer version | Absent/wrong-type version is malformed; unsupported versions block fallback rather than silently downgrading |
| Normalized settings | Finite volume clamps to range; nonfinite caller input defaults safely; target FPS normalizes to 30 or 60 |
| Explicit read result | Caller sees primary/backup/default provenance plus malformed, missing, unsupported or read-failure status |
| Read-only fallback | Loading never overwrites or repairs stored text automatically |
| Conservative write admission | Unsupported primary/backup versions or unreadable records prevent save; caller must resolve the condition |
| Validated backup policy | A valid primary can become backup; a corrupt primary never replaces an existing good backup |
| Narrow progression scope | Reset removes only its two owned keys; character choice, other preferences and settings remain intact |

Write order is temporary → backup of a validated primary, if any → primary. If the primary is invalid and a valid backup exists, that backup is left unchanged. The injected storage contract permits a failed write to partially change its own key. Checks exercise truncation at each stage: temporary failure leaves primary intact; backup failure stops before primary write; primary failure can fall back to the validated backup. A failed first save has no invented previous record to recover.

These guarantees are bounded by the storage contract and serialized access. A successful write means subsequent reads can observe its text, not that bytes have reached durable media. There is no filesystem adapter, atomic rename, flush/fsync protocol, multi-process lock or disk-full behavior verified here. The retained temporary text is staging only and is not automatically promoted during load. If no valid primary or backup remains, defaults carry failure diagnostics; default fallback does not silently repair storage.

Version-one optional fields use declared sample defaults. Unknown properties are ignored and would not round-trip through the known model; incompatible additions must change the version and require an explicit migration design. No migration is implemented. The sample progression dictionary is synthetic and has no PlayerPrefs flush or transactional guarantee: completing a level performs two independent writes. The sample does not claim cross-store consistency.

## Executable verification

Seventeen persistence checks passed using SDK 8.0.426's Roslyn compiler with .NET 8 references, nullable analysis and warnings as errors. They cover missing defaults, round trip, optional-field defaults, malformed/wrong-type JSON, normalization, backup recovery, both-invalid fallback, version refusals, read errors, write order, partial failures at all three stages, first-save failure, scoped reset and invalid progression values.

The two earlier examples were also compiled and executed unchanged: twelve scene-transition and twelve checkpoint checks passed. All results concern independent samples only. Private records tie input hashes and captured output to this candidate's exact commit.

The ordinary documented dotnet command was attempted but failed at SDK NuGet-Migrations mutex initialization. Direct compiler/runtime execution succeeded; an ordinary SDK project build is not claimed. See [sample instructions](../../samples/save-persistence/README.md).

Existing production EditMode tests cover default settings, frame-rate normalization, quality bounds, basic JSON round trip, absent-file defaults and filename conventions, alongside unrelated helpers. They were inspected, not executed. They do not establish production corruption recovery, backup preservation, interrupted-write behavior or progression reset coverage.

## Optional targeted verification for deeper claims

The developer reports that progression, rewards and settings worked in the delivered v1.0.0 Android game and were tested before client handoff. The cases below address **more specific persistence or failure-recovery claims**. Do not repeat full release QA just to record settings or reward footage. Run a focused test only when a stronger claim, changed build or reproduced defect warrants it.

| Check | Additional evidence if a targeted verification is justified |
| --- | --- |
| Settings round trip | Change each actual setting, relaunch, inspect value and applied audio/quality/FPS policy |
| Progression | Complete a controlled course, relaunch and verify level/reward/character data independently |
| Reset scope | Confirm cancellation and confirmation behavior; settings and character remain, intended progression clears |
| Legacy/malformed data | Use isolated synthetic saves to test missing fields, malformed primary, valid backup and both-invalid behavior |
| Storage faults | In a test harness, inject failures at actual write/copy/delete/move boundaries; inspect the resulting files and logs |
| Android lifecycle | Pause/resume, focus, termination/relaunch and app update tests; distinguish unsupported uninstall/clear-data retention |
| Migration policy | Define schema expectations and old-save fixtures before changing fields or stores |

Never corrupt real customer saves or a production save directory for this demonstration. Use a dedicated test profile with backups and synthetic fixtures. No Unity import, Editor/Play Mode, Android build/device, app-update or real filesystem failure verification was performed here.

## Proposed demonstration footage

When the owner is ready to film and clip-specific rights are reviewed, show a setting change surviving relaunch, then a controlled level completion and relaunch preserving the earned progression. A synthetic invalid-save fallback demonstration is optional, and if included must be labeled as an **independent model**, not production gameplay. The developer has already reported general client permission to show the project publicly; inspect actual clip-specific character/environment/audio restrictions and avoid private logs/paths. No footage was generated or published.

The integrated example has already undergone private code and narrative review. The next portfolio step is capturing selected gameplay clips and completing one final release-content review; stronger platform or fault-recovery claims would require only the specifically relevant evidence. This candidate remains private; no merge or publication is authorized.
