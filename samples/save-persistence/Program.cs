using System.Text.Json;
using Showcase.SavePersistence;

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
static string Json(SettingsData value) => JsonSerializer.Serialize(value);
var passed = 0;
void Test(string name, Action body)
{
    body();
    Console.WriteLine("PASS " + name);
    passed++;
}
var old = new SettingsData { MasterVolume = 0.25, TargetFrameRate = 30 };
var updated = new SettingsData { MasterVolume = 0.75, Haptics = false };

Test("missing records return explicit defaults without writes", () =>
{
    var storage = new FakeStorage();
    var result = new SettingsStore(storage).Load();
    Check(result.Source == LoadSource.Defaults && result.Value == new SettingsData(), "defaults");
    Check(result.Primary.Status == RecordStatus.Missing && result.Backup?.Status == RecordStatus.Missing
        && storage.Writes.Count == 0, "missing status, no repair write");
});
Test("valid settings round trip through a fresh repository instance", () =>
{
    var storage = new FakeStorage();
    Check(new SettingsStore(storage).Save(old).Status == SaveStatus.Saved, "saved");
    var result = new SettingsStore(storage).Load();
    Check(result.Source == LoadSource.Primary && result.Value == old, "round trip");
});
Test("version one missing optional fields keeps declared defaults", () =>
{
    var storage = new FakeStorage();
    storage.Data[SettingsStore.Primary] = "{\"Version\":1}";
    Check(new SettingsStore(storage).Load().Value == new SettingsData(), "field defaults");
});
Test("malformed JSON and wrong field types are rejected", () =>
{
    foreach (var text in new[] { "{", "null", "[]", "{}", "{\"Version\":\"1\"}",
        "{\"Version\":null}", "{\"Version\":1,\"Haptics\":\"yes\"}" })
    {
        var storage = new FakeStorage();
        storage.Data[SettingsStore.Primary] = text;
        var result = new SettingsStore(storage).Load();
        Check(result.Source == LoadSource.Defaults && result.Primary.Status == RecordStatus.Malformed,
            "malformed input: " + text);
        Check(storage.Writes.Count == 0, "read never repairs");
    }
});
Test("deserialized fields and nonfinite caller input normalize", () =>
{
    var storage = new FakeStorage();
    storage.Data[SettingsStore.Primary] = "{\"Version\":1,\"MasterVolume\":4,\"TargetFrameRate\":17}";
    var repository = new SettingsStore(storage);
    Check(repository.Load().Value == new SettingsData { MasterVolume = 1 }, "normalize read");
    Check(repository.Save(new SettingsData { MasterVolume = double.NaN, TargetFrameRate = 120 }).Status
        == SaveStatus.Saved, "normalize before serialize");
    Check(repository.Load().Value == new SettingsData(), "finite safe default");
});
Test("corrupt primary recovers valid backup without rewriting either", () =>
{
    var storage = new FakeStorage();
    storage.Data[SettingsStore.Primary] = "broken";
    storage.Data[SettingsStore.Backup] = Json(old);
    var result = new SettingsStore(storage).Load();
    Check(result.Source == LoadSource.Backup && result.Value == old, "backup recovered");
    Check(result.Primary.Status == RecordStatus.Malformed && storage.Writes.Count == 0, "visible, read only");
});
Test("both corrupt records return defaults with both diagnostics", () =>
{
    var storage = new FakeStorage();
    storage.Data[SettingsStore.Primary] = storage.Data[SettingsStore.Backup] = "broken";
    var result = new SettingsStore(storage).Load();
    Check(result.Source == LoadSource.Defaults && result.Primary.Status == RecordStatus.Malformed
        && result.Backup?.Status == RecordStatus.Malformed, "explicit failure provenance");
});
Test("unsupported primary blocks fallback and overwrite", () =>
{
    var storage = new FakeStorage();
    storage.Data[SettingsStore.Primary] = "{\"Version\":2}";
    storage.Data[SettingsStore.Backup] = Json(old);
    var repository = new SettingsStore(storage);
    Check(repository.Load().Source == LoadSource.BlockedByVersion && repository.Load().Value == null,
        "no silent downgrade");
    Check(repository.Save(updated).Status == SaveStatus.RejectedVersion && storage.Writes.Count == 0,
        "protect future primary");
});
Test("unsupported backup and unsupported input are not overwritten", () =>
{
    var storage = new FakeStorage();
    storage.Data[SettingsStore.Primary] = Json(old);
    storage.Data[SettingsStore.Backup] = "{\"Version\":2}";
    var repository = new SettingsStore(storage);
    Check(repository.Load().Source == LoadSource.Primary, "valid primary readable");
    Check(repository.Save(updated).Status == SaveStatus.RejectedVersion, "protect future backup");
    Check(repository.Save(updated with { Version = 99 }).Status == SaveStatus.RejectedVersion
        && storage.Writes.Count == 0, "reject input version");
    storage.Data[SettingsStore.Primary] = "broken";
    Check(repository.Load().Source == LoadSource.BlockedByVersion, "unsupported fallback reported");
});
Test("read error may recover backup but blocks save", () =>
{
    var storage = new FakeStorage { UnreadableKey = SettingsStore.Primary };
    storage.Data[SettingsStore.Backup] = Json(old);
    var repository = new SettingsStore(storage);
    var result = repository.Load();
    Check(result.Source == LoadSource.Backup && result.Primary.Status == RecordStatus.ReadFailed,
        "recover with diagnostic");
    Check(repository.Save(updated).Status == SaveStatus.RejectedReadFailure && storage.Writes.Count == 0,
        "no overwrite of unreadable data");
});
Test("write ordering retains previous validated primary as backup", () =>
{
    var storage = new FakeStorage();
    var repository = new SettingsStore(storage);
    repository.Save(old);
    storage.Writes.Clear();
    Check(repository.Save(updated).Status == SaveStatus.Saved, "second save");
    Check(storage.Writes.SequenceEqual(new[] { SettingsStore.Temporary, SettingsStore.Backup,
        SettingsStore.Primary }), "staging then backup then primary");
    Check(storage.Data[SettingsStore.Backup] == Json(old) && repository.Load().Value == updated, "previous version");
});
Test("partial temporary write leaves previous primary intact", () =>
{
    var storage = new FakeStorage();
    storage.Data[SettingsStore.Primary] = Json(old);
    storage.FailOnKey = SettingsStore.Temporary;
    Check(new SettingsStore(storage).Save(updated).Status == SaveStatus.Failed, "failure returned");
    Check(storage.Data[SettingsStore.Primary] == Json(old) && storage.Writes.Count == 1,
        "primary not touched");
});
Test("partial backup write does not attempt primary overwrite", () =>
{
    var storage = new FakeStorage();
    storage.Data[SettingsStore.Primary] = Json(old);
    storage.FailOnKey = SettingsStore.Backup;
    var repository = new SettingsStore(storage);
    Check(repository.Save(updated).Status == SaveStatus.Failed, "backup failure");
    Check(storage.Data[SettingsStore.Primary] == Json(old)
        && !storage.Writes.Contains(SettingsStore.Primary) && repository.Load().Value == old, "old primary preserved");
});
Test("partial primary write recovers backup and retry retains good backup", () =>
{
    var storage = new FakeStorage();
    storage.Data[SettingsStore.Primary] = Json(old);
    storage.FailOnKey = SettingsStore.Primary;
    var repository = new SettingsStore(storage);
    Check(repository.Save(updated).Status == SaveStatus.Failed, "primary failure");
    Check(repository.Load().Source == LoadSource.Backup && repository.Load().Value == old, "known good backup");
    storage.FailOnKey = null;
    storage.Writes.Clear();
    Check(repository.Save(updated).Status == SaveStatus.Saved && repository.Load().Value == updated, "retry");
    Check(storage.Data[SettingsStore.Backup] == Json(old) && !storage.Writes.Contains(SettingsStore.Backup),
        "corrupt primary not copied over good backup");
});
Test("failed first save has no fabricated recoverable previous record", () =>
{
    var storage = new FakeStorage { FailOnKey = SettingsStore.Primary };
    var repository = new SettingsStore(storage);
    Check(repository.Save(updated).Status == SaveStatus.Failed, "first save failure");
    Check(repository.Load().Source == LoadSource.Defaults && !storage.Data.ContainsKey(SettingsStore.Backup),
        "temporary is not committed recovery");
});
Test("progression reset preserves character, settings and unrelated values", () =>
{
    var storage = new FakeStorage();
    storage.Data["unrelated.text"] = "keep";
    var repository = new SettingsStore(storage);
    repository.Save(old);
    var preferences = new Dictionary<string, int> { ["avatar.choice"] = 2, ["other.preference"] = 42 };
    var progression = new AdventurePreferences(preferences);
    progression.CompleteLevel(1);
    progression.CompleteLevel(2);
    Check(new AdventurePreferences(preferences).HighestLevel == 3 && progression.RewardBits == 3, "retained progress");
    progression.CompleteLevel(1);
    Check(progression.HighestLevel == 3 && progression.RewardBits == 3, "no downgrade");
    progression.ResetAdventure();
    Check(progression.HighestLevel == 1 && progression.RewardBits == 0 && preferences.Count == 2, "reset scope");
    Check(preferences["avatar.choice"] == 2 && preferences["other.preference"] == 42, "unrelated preferences");
    Check(repository.Load().Value == old && storage.Data["unrelated.text"] == "keep", "separate settings intact");
});
Test("invalid progression values normalize without granting rewards", () =>
{
    var values = new Dictionary<string, int>
    {
        [AdventurePreferences.LevelKey] = -9, [AdventurePreferences.RewardsKey] = -1
    };
    var progression = new AdventurePreferences(values);
    Check(progression.HighestLevel == 1 && progression.RewardBits == 0, "invalid values");
    progression.CompleteLevel(1);
    Check(progression.HighestLevel == 2 && progression.RewardBits == 1, "only earned reward");
    var rejected = false;
    try { progression.CompleteLevel(4); }
    catch (ArgumentOutOfRangeException) { rejected = true; }
    Check(rejected && progression.HighestLevel == 2 && progression.RewardBits == 1, "invalid completion no mutation");
});
Console.WriteLine($"{passed} persistence contract checks passed.");

// Synthetic store only. A failing write deliberately leaves truncated content so
// checks exercise partial mutation rather than an unrealistically all-or-nothing fake.
sealed class FakeStorage : ITextStorage
{
    public Dictionary<string, string> Data { get; } = new();
    public List<string> Writes { get; } = new();
    public string? FailOnKey { get; set; }
    public string? UnreadableKey { get; init; }
    public string? Read(string key)
    {
        if (key == UnreadableKey) throw new IOException("synthetic read failure");
        return Data.GetValueOrDefault(key);
    }
    public void Write(string key, string text)
    {
        Writes.Add(key);
        if (key == FailOnKey)
        {
            Data[key] = "{partial";
            throw new IOException("synthetic partial write");
        }
        Data[key] = text;
    }
}
