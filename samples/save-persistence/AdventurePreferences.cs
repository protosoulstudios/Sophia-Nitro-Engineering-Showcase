// Independent illustration of a narrow reset over a separate preference store.
namespace Showcase.SavePersistence;

public sealed class AdventurePreferences
{
    public const string LevelKey = "demo.progress.level";
    public const string RewardsKey = "demo.progress.rewards";
    private readonly IDictionary<string, int> preferences;
    public AdventurePreferences(IDictionary<string, int> preferences) =>
        this.preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));

    public int HighestLevel => preferences.TryGetValue(LevelKey, out var value) ? Math.Clamp(value, 1, 3) : 1;
    public int RewardBits => preferences.TryGetValue(RewardsKey, out var value) && value is >= 0 and <= 7
        ? value : 0;

    public void CompleteLevel(int level)
    {
        if (level is < 1 or > 3) throw new ArgumentOutOfRangeException(nameof(level));
        preferences[LevelKey] = Math.Max(HighestLevel, Math.Min(3, level + 1));
        preferences[RewardsKey] = RewardBits | (1 << (level - 1));
    }

    public void ResetAdventure()
    {
        preferences.Remove(LevelKey);
        preferences.Remove(RewardsKey);
    }
}
