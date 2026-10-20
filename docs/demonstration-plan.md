# Demonstration evidence plan

[Portfolio](../README.md) · [Verification matrix](verification-matrix.md)

The minimum useful package is four short gameplay clips plus one clearly labeled synthetic failure segment. Durations below are planning estimates, not existing footage. No screenshots, video, customer saves or demonstration results have been produced for this plan.

## Minimum shot list

| Clip | Approximate content | What the recording can show | What it cannot establish |
| --- | --- | --- | --- |
| M1 — Navigation, 20–35 seconds | Menu → character selection → Loading → course; one restart, with readable action labels | The recorded build follows the visible route and restart sequence | Busy-guard coverage, coroutine ownership, cancellation safety or all scene bindings; pair with V1 observations |
| M2 — Recovery, 30–45 seconds | Initial spawn → checkpoint → fall → recovery → second fall/recovery → scene restart | Observed checkpoint recovery and restart boundary on that character/build | All collider types, complete internal motion cleanup, other rigs or lifecycle ordering; pair with V2 and a second-character check |
| M3 — Settings, 20–35 seconds | Change one visible setting, leave/relaunch app, show restored value and relevant observable effect | That setting persisted in the recorded installation | Every field, forced-kill durability or disk integrity; record V3 separately |
| M4 — Progression, 20–40 seconds plus disclosed completion cut | Show locked state, controlled course completion/reward, relaunch, then retained unlock/reward | The recorded progression survives this relaunch | Atomic multi-key saves, cross-device progress or all completion paths; label any edit/time skip rather than implying one continuous take |
| M5 — Synthetic recovery, 15–30 seconds | In an isolated harness, show a synthetic malformed primary and valid backup, then explicit fallback result; optionally show a partial-write assertion | The named model or harness handles its controlled fixture | Production corruption recovery or real filesystem guarantees; never label the independent model as gameplay footage |

## Prerequisites and capture order

1. Obtain permission for client/project identification and depiction of character models, purchased environment content and any audible third-party material. Permission to discuss code does not establish asset depiction or redistribution rights. Review capture permission before recording; rights clearance must precede any publication.
2. Use a disposable development profile and synthetic save fixtures. Confirm exact source/build revision, Unity version, device/OS and intended behavior. Never manipulate customer saves or the authoritative production save directory.
3. Run V1–V3 before capture. Characterize approved V4 fault cases privately before deciding whether they are suitable evidence. Unreproduced source risks must remain labeled as inspection findings.
4. Capture M1 and M2 in one gameplay session where possible. Capture M3 and M4 as separate relaunch demonstrations; keep synthetic M5 visibly distinct. Disable private notifications and omit personal paths, account details, signing/configuration views and confidential logs. Use captions where audio permissions are unresolved.
5. Retain the unedited recording privately with a shot log and revision references. Review the final cut for captions, edits, rights and confidentiality. Only then seek explicit owner approval for the selected media and publication destination.

## Evidence handoff

A private media record should identify clip ID, source/build revision, device/OS, fixture, observation, edits, file hash, clearance scope and owner approval. A future public caption should state what was observed and the build/platform context without exposing private records or paths. No placeholder image, download link or claim that a planned shot already exists should be added.

Media complements the [case studies](../README.md#explore-the-engineering) and executable checks; it does not replace them. Select launch claims according to the evidence actually captured, including any known limitations.
