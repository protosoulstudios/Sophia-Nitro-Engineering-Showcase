// Independent data-only demonstration, not a CharacterController implementation.
using System.Numerics;

namespace Showcase.CheckpointRespawn;

public sealed class PlayerRecoveryState : IRespawnTarget
{
    public bool IsAvailable { get; set; } = true;
    public Vector3 Position { get; private set; }
    public float YawDegrees { get; private set; }
    public Vector3 Velocity { get; set; }
    public Vector3 ExternalVelocity { get; set; }
    public Vector3 PendingDisplacement { get; set; }
    public Vector2 MoveInput { get; set; }
    public bool JumpQueued { get; set; }
    public bool AbilityHeld { get; set; }
    public float SpeedMultiplier { get; set; } = 1f;

    public void Restore(RespawnPose pose)
    {
        if (!IsAvailable) throw new InvalidOperationException("Player unavailable.");
        if (!pose.IsValid) throw new ArgumentException("Invalid recovery pose.", nameof(pose));
        Position = pose.Position;
        YawDegrees = pose.YawDegrees;
        Velocity = Vector3.Zero;
        ExternalVelocity = Vector3.Zero;
        PendingDisplacement = Vector3.Zero;
        MoveInput = Vector2.Zero;
        JumpQueued = false;
        AbilityHeld = false;
        SpeedMultiplier = 1f;
    }
}
