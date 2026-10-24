# Demonstration evidence plan

[Portfolio](../README.md) · [Verification matrix](verification-matrix.md)

The owner approved and published the **code-first engineering showcase** on October 24, 2026, with gameplay footage to be added later if and when ready. The optional future package contains up to four short clips of the **already completed and owner-tested** game, plus a separate optional synthetic C# example. The exact clip selection can be finalized after capture; a new full QA campaign is not required. Durations below are planning estimates, not existing footage. No screenshots, video, customer saves or new recordings have been produced for this plan. The [video staging directory](../media/videos/README.md) is reserved for future approved files.

## Suggested shot list — optional, not recorded

| Clip | Approximate content | What the recording can show | What it cannot establish |
| --- | --- | --- | --- |
| M1 — Navigation, 20–35 seconds | Main Menu → Level Select → EASY → Sophia/Nitro → Loading → Level_Easy; one restart, with readable action labels | The recorded build follows the visible route and restart sequence | Busy-guard coverage, coroutine ownership, cancellation safety or all scene bindings; focused V1 evidence is optional for such deeper claims |
| M2 — Recovery, 30–45 seconds | Initial spawn → checkpoint → fall → recovery → second fall/recovery → scene restart | Observed checkpoint recovery and restart boundary on that character/build | All collider types, complete internal motion cleanup, other rigs or lifecycle ordering; optional focused V2 testing is needed only for those stronger claims |
| M3 — Settings, 20–35 seconds | Change one visible setting, leave/relaunch app, show restored value and relevant observable effect | That setting persisted in the recorded installation | Every field, forced-kill durability or disk integrity; additional V3 testing is optional and claim-specific |
| M4 — Progression, 20–40 seconds plus disclosed completion cut | Show locked state, controlled course completion/reward, relaunch, then retained unlock/reward | The recorded progression survives this relaunch | Atomic multi-key saves, cross-device progress or all completion paths; label any edit/time skip rather than implying one continuous take |
| M5 — Synthetic recovery, 15–30 seconds | In an isolated harness, show a synthetic malformed primary and valid backup, then explicit fallback result; optionally show a partial-write assertion | The named model or harness handles its controlled fixture | Production corruption recovery or real filesystem guarantees; never label the independent model as gameplay footage |

## Prerequisites and capture order

1. The developer has already reported **client permission to make the original Unity production repository public**. The developer chose to keep it private and publish this engineering showcase instead; do not ask again for that permission. Before publicly adding footage, check third-party character, purchased environment, audio and confidentiality conditions relevant to that clip. Permission to publish a repository is distinct from a general license to redistribute third-party assets.
2. Record from the previously working released game or an approved development build. Use a disposable profile only when a settings/progression demo would modify save state. Note actual version and device/OS when available. Never manipulate customer saves or the authoritative production save directory.
3. **Do not rerun V1–V3 simply to record footage.** The developer's v1.0.0 release QA was already completed. Capture the visible behavior directly; use a *targeted* check only if footage reveals a defect, a different build changes behavior, or a specific engineering claim requires new evidence. Unreproduced source risks remain source-inspection findings.
4. Capture M1 and M2 in one gameplay session if convenient; M3 and M4 can be separate relaunch demonstrations. M5 is optional and must remain explicitly labeled as the independent C# model, not production gameplay. Disable private notifications and omit personal paths, account details, signing/configuration views and confidential logs. Use captions where audio rights are unclear.
5. Retain original recordings privately; review each public clip for edits, accuracy, asset/audio terms and confidentiality. Add only approved final clips or approved external links to the reserved video folder and case studies. The code-first showcase is already public without videos; future clips still require independent clearance before addition.

## Evidence handoff

A private media record should identify clip ID, source/build revision, device/OS, fixture, observation, edits, file hash, clearance scope and owner approval. A future public caption should state what was observed and the build/platform context without exposing private records or paths. No placeholder image, download link or claim that a planned shot already exists should be added.

Media complements the [case studies](../README.md#explore-the-engineering) and executable checks; it does not replace them. Describe each future clip only to the extent supported by the evidence actually captured, including any known limitations.
