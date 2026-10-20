using System.Numerics;
using Showcase.CheckpointRespawn;

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
var passed = 0;
void Test(string name, Action body)
{
    body();
    Console.WriteLine("PASS " + name);
    passed++;
}
var spawn = new RespawnPose(new Vector3(1, 2, 3), 30);
var checkpointPose = new RespawnPose(new Vector3(10, 20, 30), 90);

Test("initial spawn fallback", () =>
{
    var authority = new CheckpointAuthority(spawn);
    var player = new PlayerRecoveryState();
    Check(authority.Respawn(authority.SceneId, player) == RespawnResult.Restored, "restored");
    Check(player.Position == spawn.Position && player.YawDegrees == spawn.YawDegrees, "fallback pose");
});
Test("activation uses a value snapshot", () =>
{
    var authority = new CheckpointAuthority(spawn);
    var snapshot = new CheckpointSnapshot(authority.SceneId, "bridge", checkpointPose);
    Check(authority.Activate(snapshot) == ActivationResult.Activated, "activated");
    snapshot = snapshot with { Pose = spawn };
    var player = new PlayerRecoveryState();
    authority.Respawn(authority.SceneId, player);
    Check(player.Position == checkpointPose.Position && player.YawDegrees == 90, "copied snapshot");
});
Test("replacement selects latest accepted checkpoint", () =>
{
    var authority = new CheckpointAuthority(spawn);
    authority.Activate(new(authority.SceneId, "first", spawn));
    authority.Activate(new(authority.SceneId, "second", checkpointPose));
    Check(authority.ActiveCheckpoint?.Id == "second", "latest");
    var player = new PlayerRecoveryState();
    authority.Respawn(authority.SceneId, player);
    Check(player.Position == checkpointPose.Position, "latest destination");
});
Test("invalid activation preserves last valid checkpoint", () =>
{
    var authority = new CheckpointAuthority(spawn);
    var valid = new CheckpointSnapshot(authority.SceneId, "valid", checkpointPose);
    authority.Activate(valid);
    Check(authority.Activate(valid with { Id = " " }) == ActivationResult.Invalid, "blank id");
    Check(authority.Activate(valid with { Pose = new(new(float.NaN, 0, 0), 0) })
        == ActivationResult.Invalid, "nonfinite position");
    Check(authority.Activate(valid with { Pose = new(Vector3.Zero, float.PositiveInfinity) })
        == ActivationResult.Invalid, "nonfinite yaw");
    Check(authority.ActiveCheckpoint == valid, "no lost checkpoint");
});
Test("repeated recovery clears supported motion and input", () =>
{
    var authority = new CheckpointAuthority(spawn);
    var player = new PlayerRecoveryState();
    for (var i = 0; i < 3; i++)
    {
        player.Velocity = player.ExternalVelocity = player.PendingDisplacement = Vector3.One;
        player.MoveInput = Vector2.One;
        player.JumpQueued = player.AbilityHeld = true;
        player.SpeedMultiplier = 2;
        Check(authority.Respawn(authority.SceneId, player) == RespawnResult.Restored, "repeat restored");
        Check(player.Velocity == Vector3.Zero && player.ExternalVelocity == Vector3.Zero
            && player.PendingDisplacement == Vector3.Zero && player.MoveInput == Vector2.Zero
            && !player.JumpQueued && !player.AbilityHeld && player.SpeedMultiplier == 1, "clean motion");
    }
});
Test("scene reset invalidates old checkpoints and queued failures", () =>
{
    var authority = new CheckpointAuthority(spawn);
    var old = new CheckpointSnapshot(authority.SceneId, "old", checkpointPose);
    authority.Activate(old);
    Check(authority.ResetScene(checkpointPose), "reset");
    Check(authority.SceneId != old.SceneId && authority.ActiveCheckpoint == null, "fresh session");
    Check(authority.Activate(old) == ActivationResult.WrongScene, "stale checkpoint");
    var player = new PlayerRecoveryState();
    Check(authority.Respawn(old.SceneId, player) == RespawnResult.WrongScene, "stale failure");
    Check(player.Position == Vector3.Zero, "no stale write");
    Check(authority.Respawn(authority.SceneId, player) == RespawnResult.Restored
        && player.Position == checkpointPose.Position, "new fallback");
});
Test("invalid reset preserves session and checkpoint", () =>
{
    var authority = new CheckpointAuthority(spawn);
    var old = new CheckpointSnapshot(authority.SceneId, "keep", checkpointPose);
    authority.Activate(old);
    Check(!authority.ResetScene(new(Vector3.Zero, float.NaN)), "invalid reset");
    Check(authority.SceneId == old.SceneId && authority.ActiveCheckpoint == old, "unchanged");
});
Test("null and unavailable targets do not retain ownership", () =>
{
    var authority = new CheckpointAuthority(spawn);
    Check(authority.Respawn(authority.SceneId, null) == RespawnResult.Unavailable, "null");
    var player = new PlayerRecoveryState { IsAvailable = false };
    Check(authority.Respawn(authority.SceneId, player) == RespawnResult.Unavailable, "unavailable");
    Check(player.Position == Vector3.Zero, "no write");
    player.IsAvailable = true;
    Check(authority.Respawn(authority.SceneId, player) == RespawnResult.Restored, "retry");
});
Test("reentrant recovery cannot change checkpoint or reset scene", () =>
{
    var authority = new CheckpointAuthority(spawn);
    var token = authority.SceneId;
    var calls = 0;
    var target = new CallbackTarget { RestoreAction = pose =>
    {
        calls++;
        Check(authority.Respawn(token, new PlayerRecoveryState()) == RespawnResult.Busy, "nested recovery");
        Check(authority.Activate(new(token, "nested", checkpointPose)) == ActivationResult.Busy, "nested activation");
        Check(!authority.ResetScene(checkpointPose), "nested reset");
        Check(pose == spawn && authority.SceneId == token, "original recovery");
    } };
    Check(authority.Respawn(token, target) == RespawnResult.Restored && calls == 1, "one call");
});
Test("target failure releases ownership without claiming rollback", () =>
{
    var authority = new CheckpointAuthority(spawn);
    var failure = new IOException("synthetic recovery failure");
    var partialChange = false;
    var target = new CallbackTarget { RestoreAction = _ => { partialChange = true; throw failure; } };
    Check(authority.Respawn(authority.SceneId, target) == RespawnResult.Failed, "failure");
    Check(partialChange && ReferenceEquals(authority.LastFailure, failure), "failure observable, no rollback");
    Check(authority.Respawn(authority.SceneId, new PlayerRecoveryState()) == RespawnResult.Restored
        && authority.LastFailure == null, "retry releases and clears diagnostic");
});
Test("availability exception releases ownership", () =>
{
    var authority = new CheckpointAuthority(spawn);
    var target = new CallbackTarget { Availability = () => throw new IOException("synthetic availability") };
    Check(authority.Respawn(authority.SceneId, target) == RespawnResult.Failed, "availability failure");
    Check(authority.LastFailure is IOException, "diagnostic");
    Check(authority.ResetScene(spawn), "ownership released");
});
Test("cross-authority snapshots and invalid initial spawn are rejected", () =>
{
    var first = new CheckpointAuthority(spawn);
    var second = new CheckpointAuthority(spawn);
    Check(second.Activate(new(first.SceneId, "foreign", checkpointPose)) == ActivationResult.WrongScene,
        "cross authority");
    var rejected = false;
    try { _ = new CheckpointAuthority(new(Vector3.Zero, float.NaN)); }
    catch (ArgumentException) { rejected = true; }
    Check(rejected, "invalid constructor");
});
Console.WriteLine($"{passed} checkpoint contract checks passed.");

sealed class CallbackTarget : IRespawnTarget
{
    public Func<bool> Availability { get; init; } = () => true;
    public Action<RespawnPose> RestoreAction { get; init; } = _ => { };
    public bool IsAvailable => Availability();
    public void Restore(RespawnPose pose) => RestoreAction(pose);
}
