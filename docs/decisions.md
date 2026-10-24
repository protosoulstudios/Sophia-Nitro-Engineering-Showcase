# Engineering decisions and tradeoffs

These interpretations follow the implemented responsibilities; they are not quotations from historical design discussions.

- A persistent transition runner makes the execution lifetime independent of the outgoing scene. Interruption semantics remain a separate concern: resetting transition state does not cancel Unity loading.
- Scene-local checkpoints simplify ownership and respawn routing. They do not provide cross-session checkpoint restoration.
- Separate progression preferences and JSON settings allow a narrow adventure reset that retains character choice and settings. They also require explicit future migration and recovery policies across stores.
- Backup fallback offers a recovery path, while the primary replacement sequence retains a failure window. Future work could test interruption cases and make save outcomes observable.
- Frame-rate caps express a mobile policy; device measurements are required to assess sustained performance.

These **specialized future checks** are optional investigation or evidence opportunities, not a statement that general release QA was never completed. The developer already reports completing final Android playthrough testing before client delivery. See [coverage](coverage.md).
