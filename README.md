# Sophia & Nitro: Sweetland Adventures

An engineering case study for Protosoul Studios: a Unity Android obstacle-course game with character selection, three courses, checkpoints, progression rewards, and mobile application services.

The engineering focus is ownership across scene changes: persistent services coordinate transitions and settings, while scene-local systems manage players and checkpoint recovery. Progression and settings deliberately use separate persistence paths.

| Engineering area | Implemented approach |
| --- | --- |
| Scene transitions | Persistent coroutine host, validated requests and explicit transition states |
| Checkpoint recovery | Scene-local checkpoint authority and a typed player respawn API |
| Progression | Bounded level unlocks and separate reward flags |
| Persistence | PlayerPrefs progression plus JSON settings with backup fallback |
| Mobile policy | Supported frame-rate targets and focused logic tests |

Read the [architecture](docs/architecture.md), [gameplay and persistence](docs/gameplay-and-persistence.md), [mobile and validation](docs/mobile-and-validation.md), [engineering decisions](docs/decisions.md), and [coverage limits](docs/coverage.md). See the [draft history](CHANGELOG.md).

The reviewed project pins Unity 6000.2.14f1. This showcase contains documentation only. It includes no game source, assets, executable build, or license grant. Source inspection supports the implementation descriptions; runtime and Android verification are not claimed. This reconstructed draft is prepared for owner review, not authorized for publication.
