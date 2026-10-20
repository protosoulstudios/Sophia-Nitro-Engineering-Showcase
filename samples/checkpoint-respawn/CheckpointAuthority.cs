// Independently authored private portfolio candidate. Not copied Unity production code.
// Public redistribution and reuse terms remain pending owner review.
using System.Numerics;

namespace Showcase.CheckpointRespawn;

// Yaw-only recovery matches the game's upright-player responsibility.
public readonly record struct RespawnPose(Vector3 Position, float YawDegrees)
{
    public bool IsValid => float.IsFinite(Position.X) && float.IsFinite(Position.Y)
        && float.IsFinite(Position.Z) && float.IsFinite(YawDegrees);
}

// A value snapshot never retains a mutable/destroyed scene Transform.
// SceneId is a stale-work token, not an authorization or anti-cheat credential.
public readonly record struct CheckpointSnapshot(Guid SceneId, string Id, RespawnPose Pose);
public enum ActivationResult { Activated, Invalid, WrongScene, Busy }
public enum RespawnResult { Restored, Unavailable, WrongScene, Busy, Failed }

public interface IRespawnTarget
{
    bool IsAvailable { get; }
    // Synchronous owner-context operation. Return only after supported recovery is
    // complete. Throws may leave partial player changes; no transactional rollback.
    void Restore(RespawnPose pose);
}

// One authority per scene session. All access must be serialized by its owner.
// No Unity lifetime detection, physics integration, multiplayer or save persistence.
public sealed class CheckpointAuthority
{
    private RespawnPose initialSpawn;
    private bool restoring;
    public Guid SceneId { get; private set; } = Guid.NewGuid();
    public CheckpointSnapshot? ActiveCheckpoint { get; private set; }
    public Exception? LastFailure { get; private set; }

    public CheckpointAuthority(RespawnPose initialSpawn)
    {
        if (!initialSpawn.IsValid) throw new ArgumentException("Invalid spawn.", nameof(initialSpawn));
        this.initialSpawn = initialSpawn;
    }

    public ActivationResult Activate(CheckpointSnapshot checkpoint)
    {
        if (restoring) return ActivationResult.Busy;
        if (checkpoint.SceneId != SceneId) return ActivationResult.WrongScene;
        if (string.IsNullOrWhiteSpace(checkpoint.Id) || !checkpoint.Pose.IsValid)
            return ActivationResult.Invalid;
        ActiveCheckpoint = checkpoint;
        return ActivationResult.Activated;
    }

    // Caller supplies the token captured when its scene-local request was created.
    // Delayed work from an earlier scene must not recover a player in the new scene.
    public RespawnResult Respawn(Guid expectedScene, IRespawnTarget? player)
    {
        if (restoring) return RespawnResult.Busy;
        if (expectedScene != SceneId) return RespawnResult.WrongScene;
        if (player is null) return RespawnResult.Unavailable;
        restoring = true; // Reserve before target getters or callbacks can reenter.
        LastFailure = null;
        try
        {
            if (!player.IsAvailable) return RespawnResult.Unavailable;
            player.Restore(ActiveCheckpoint?.Pose ?? initialSpawn);
            return RespawnResult.Restored;
        }
        catch (Exception failure)
        {
            LastFailure = failure;
            return RespawnResult.Failed;
        }
        finally
        {
            restoring = false;
        }
    }

    // Explicit adapter responsibility at a scene boundary; no hidden reset event.
    public bool ResetScene(RespawnPose spawn)
    {
        if (restoring || !spawn.IsValid) return false;
        initialSpawn = spawn;
        ActiveCheckpoint = null;
        LastFailure = null;
        SceneId = Guid.NewGuid();
        return true;
    }
}
