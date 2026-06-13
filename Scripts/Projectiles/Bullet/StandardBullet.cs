using Godot;
using System;
using System.Collections;

public partial class StandardBullet : BaseProjectile
{
    private PackedScene dustParticlesScene = GD.Load<PackedScene>("res://Assets/Scenes/Combat/VFX/dustParticles.tscn");
    private GpuParticles3D dustParticles;
    public override void _Ready() 
    { 
        base._Ready();
        strength = 1; 
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
            if (collisionInstance is BaseTile tile)
                tile.Damage(strength);
            Destroy();
        }
        dustParticles.GlobalRotation = Vector3.Zero;
    }
    protected override void OnAreaEntered(Area3D area)
    {
        if (area is BaseProjectile proj && proj.HurtType != HurtType)
            proj.Destroy();
    }
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
