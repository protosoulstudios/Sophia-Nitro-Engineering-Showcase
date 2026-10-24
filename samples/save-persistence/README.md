# Save and persistence demonstration

Independently authored demonstration, not copied Unity production code. No real save data, assets or filesystem adapter is included. It is prepared for professional code inspection, subject to owner approval; no public reuse or redistribution license is granted.

- `SettingsStore.cs`: version-one model, validation, read provenance, guarded writes and known-valid backup policy.
- `AdventurePreferences.cs`: two-key synthetic progression model and narrowly scoped reset.
- `Program.cs`: seventeen executable checks with a storage fake that can truncate a key before throwing.

With .NET 8 SDK installed, the ordinary command from the repository root is:

```sh
dotnet run --project samples/save-persistence/SavePersistence.csproj --configuration Release
```

Expected final output: `17 persistence contract checks passed.` Assertions exit nonzero on failure. The ordinary command was attempted here but failed during NuGet-Migrations mutex initialization. Successful verification used SDK 8.0.426's Roslyn compiler directly against .NET 8 references, with nullable analysis and warnings as errors, followed by .NET runtime execution. This is not a verified ordinary project build or Unity test.

All access, including other users of the storage, must be serialized. A failed storage write may partially mutate only its own key. Successful writes must be visible to subsequent reads; power-loss durability is not promised. The sample has no physical filesystem implementation, atomic replacement, flush, encryption, concurrency control, migration or automatic repair. Temporary content is not used for recovery.

Loads return explicit provenance and per-slot statuses. Both unusable slots return defaults with diagnostics; unsupported versions block rather than silently downgrade. Save refuses unreadable/unsupported existing records, writes staging text, backs up only a validated primary, then writes primary. If a corrupt primary was recovered from backup, save preserves that valid backup. Unknown fields are ignored and not round-tripped; future incompatible schema changes require versioning and explicit migration work.

The [case study](../../docs/showcases/save-and-persistence.md) explains production differences, bounded guarantees and optional targeted checks for stronger claims. It does not reopen the developer's previously completed Android release QA.
