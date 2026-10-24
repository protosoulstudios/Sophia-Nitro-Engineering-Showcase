# Gameplay and persistence

GAME-01: The scene-local manager records checkpoint position and rotation. Its typed respawn method accepts a player controller and delegates repositioning to that controller. The checkpoint is transient scene state, not a saved continuation across application sessions.

GAME-02: The progression component guards repeated completion within its instance, unlocks the configured reward and optionally the next level, and requests transitions through the loading service. Replay requests another scene without clearing progression. Configuration determines each finisher's specific reward; this source inspection does not independently reverify serialized bindings or the developer's completed release QA.

DATA-01: Progression preferences store character choice, pending level, highest unlocked level bounded to one through three, and reward flags. Reset removes progression and reward keys while preserving character choice. Settings have a separate JSON persistence path.

DATA-02: The save service writes a temporary JSON file, copies an existing primary to backup, deletes the old primary, then moves the temporary file. Loading tries the primary and then backup. The delete/move interval is a failure window: this is not a guaranteed atomic replacement. Save catches and logs exceptions rather than returning a success result.

The developer's v1.0.0 release record already reports working checkpoint recovery, progression, and adventure reset in the shipped Android game. **Fault-injected save replacement, malformed-file recovery, and narrowly defined interruption cases remain source-inspected only**; new targeted tests would be needed before claiming those particular failure guarantees. The inspection here establishes source paths rather than new fault-injection outcomes.
