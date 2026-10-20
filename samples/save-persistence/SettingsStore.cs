// Independently authored private portfolio candidate, not production Unity code.
// Public redistribution and reuse terms remain pending owner review.
using System.Text.Json;

namespace Showcase.SavePersistence;

public sealed record SettingsData
{
    public int Version { get; init; } = 1;
    public double MasterVolume { get; init; } = 1;
    public int TargetFrameRate { get; init; } = 60;
    public bool Haptics { get; init; } = true;

    public SettingsData Normalize() => this with
    {
        MasterVolume = double.IsFinite(MasterVolume) ? Math.Clamp(MasterVolume, 0, 1) : 1,
        TargetFrameRate = TargetFrameRate == 30 ? 30 : 60
    };
}

// Missing returns null; failed reads throw. Write may partially change only its key
// before throwing. Successful return means the text is available to subsequent reads,
// not that it survived a power loss. No filesystem adapter is supplied.
public interface ITextStorage
{
    string? Read(string key);
    void Write(string key, string text);
}

public enum RecordStatus { Missing, Valid, Malformed, UnsupportedVersion, ReadFailed }
public sealed record SlotRead(RecordStatus Status, SettingsData? Value = null, string? Error = null);
public enum LoadSource { Primary, Backup, Defaults, BlockedByVersion }
public sealed record LoadResult(SettingsData? Value, LoadSource Source, SlotRead Primary, SlotRead? Backup);
public enum SaveStatus { Saved, Failed, RejectedVersion, RejectedReadFailure }
public sealed record SaveResult(SaveStatus Status, string? Error = null);

// Fixed owned keys prevent callers from accidentally selecting arbitrary records.
// All operations require serialized access, including all users of the storage.
public sealed class SettingsStore
{
    public const string Primary = "demo.settings.primary";
    public const string Backup = "demo.settings.backup";
    public const string Temporary = "demo.settings.temporary";
    private readonly ITextStorage storage;

    public SettingsStore(ITextStorage storage) =>
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));

    private SlotRead ReadSlot(string key)
    {
        string? text;
        try { text = storage.Read(key); }
        catch (Exception failure) { return new(RecordStatus.ReadFailed, Error: failure.Message); }
        if (text is null) return new(RecordStatus.Missing);
        try
        {
            using var document = JsonDocument.Parse(text);
            if (document.RootElement.ValueKind != JsonValueKind.Object ||
                !document.RootElement.TryGetProperty(nameof(SettingsData.Version), out var version) ||
                version.ValueKind != JsonValueKind.Number ||
                !version.TryGetInt32(out var number))
                return new(RecordStatus.Malformed, Error: "Explicit integer version required.");
            if (number != 1) return new(RecordStatus.UnsupportedVersion, Error: "Only version 1 is supported.");
            var value = JsonSerializer.Deserialize<SettingsData>(text);
            return value is null ? new(RecordStatus.Malformed)
                : new(RecordStatus.Valid, value.Normalize());
        }
        catch (JsonException failure) { return new(RecordStatus.Malformed, Error: failure.Message); }
    }

    public LoadResult Load()
    {
        var primary = ReadSlot(Primary);
        if (primary.Status == RecordStatus.Valid)
            return new(primary.Value, LoadSource.Primary, primary, null);
        // Never silently downgrade an explicitly unsupported primary to an older backup.
        if (primary.Status == RecordStatus.UnsupportedVersion)
            return new(null, LoadSource.BlockedByVersion, primary, null);
        var backup = ReadSlot(Backup);
        if (backup.Status == RecordStatus.UnsupportedVersion)
            return new(null, LoadSource.BlockedByVersion, primary, backup);
        if (backup.Status == RecordStatus.Valid)
            return new(backup.Value, LoadSource.Backup, primary, backup);
        // Defaults are a caller-visible fallback, not a write or repair of bad storage.
        return new(new SettingsData(), LoadSource.Defaults, primary, backup);
    }

    public SaveResult Save(SettingsData value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Version != 1) return new(SaveStatus.RejectedVersion, "Unsupported input version.");
        var primary = ReadSlot(Primary);
        var backup = ReadSlot(Backup);
        if (primary.Status == RecordStatus.UnsupportedVersion || backup.Status == RecordStatus.UnsupportedVersion)
            return new(SaveStatus.RejectedVersion, "An unsupported record must not be overwritten.");
        if (primary.Status == RecordStatus.ReadFailed || backup.Status == RecordStatus.ReadFailed)
            return new(SaveStatus.RejectedReadFailure, "Cannot safely inspect existing records.");
        try
        {
            var candidate = JsonSerializer.Serialize(value.Normalize());
            storage.Write(Temporary, candidate);
            // Backup only a validated primary. If primary is corrupt, retain any
            // existing valid backup rather than copying corruption over it.
            if (primary.Status == RecordStatus.Valid)
                storage.Write(Backup, JsonSerializer.Serialize(primary.Value));
            storage.Write(Primary, candidate);
            return new(SaveStatus.Saved);
        }
        catch (Exception failure) { return new(SaveStatus.Failed, failure.Message); }
    }
}
