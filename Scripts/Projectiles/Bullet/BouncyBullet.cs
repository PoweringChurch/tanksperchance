using Godot;
using System;

public partial class BouncyBullet : BaseProjectile
{
    public int MaxBounces = 1;
    private int currentBounces = 0;
    private PackedScene dustParticlesScene = GD.Load<PackedScene>("res://Assets/Scenes/Combat/VFX/dustParticles.tscn");
    private GpuParticles3D dustParticles;
    public override void _Ready() { 
        base._Ready(); strength = 1;
        dustParticles = dustParticlesScene.Instantiate<GpuParticles3D>();
        AddChild(dustParticles);
        dustParticles.GlobalRotation = Vector3.Zero;
     }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (raycast.IsColliding())
        {
            CollisionObject3D collisionInstance = (CollisionObject3D)raycast.GetCollider();
            if (collisionInstance is CharacterBody3D)
            {
                Destroy();
                return;
            }
            else if (collisionInstance is BaseTile tile)
                tile.Damage(strength);
            if (currentBounces == MaxBounces) {
                Destroy();
                return;
            }
            var collisionPoint = raycast.GetCollisionPoint();
            var rawNormal = raycast.GetCollisionNormal();
            if (rawNormal.IsZeroApprox())
                return;
            var normal = rawNormal.Normalized();
            Direction = Direction.Normalized();
            Direction = Direction.Bounce(normal); 
            GlobalPosition = collisionPoint + normal * 0.1f;
            if (Direction != Vector3.Zero && !Direction.IsEqualApprox(Vector3.Up) && !Direction.IsEqualApprox(Vector3.Down))
                LookAt(GlobalPosition + Direction, Vector3.Up);
            currentBounces++;
            OnBounce();
        }
        dustParticles.GlobalRotation = Vector3.Zero;
    }
    protected virtual void OnBounce()
    {}
    public override void _ExitTree()
    {
        dustParticles.Emitting = false;
        RemoveChild(dustParticles);
        GetTree().Root.AddChild(dustParticles);
        double lingerTime = dustParticles.Lifetime + 1.0f; // adjusting for safety
        GetTree().CreateTimer(lingerTime).Timeout += dustParticles.QueueFree;
        QueueFree();
    }
}
