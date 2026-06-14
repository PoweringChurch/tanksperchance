using Godot;
using System;
using System.Runtime.CompilerServices;

[GlobalClass]
public partial class BaseProjectile : Hitbox
{
    public Vector3 Direction { get; set; } //init direction when fired
    public float Speed { get; set; }
    public float Duration { get; set; }
    public bool Active { get; set; }
    public bool DestroyOnHit = true;
    public int strength;
    public RayCast3D raycast;
    public CollisionShape3D projShape;
    public override void _Ready()
    {
        base._Ready();
        raycast = GetNode<RayCast3D>("RayCast3D");
        projShape = GetNode<CollisionShape3D>("CollisionShape3D");
        CollisionLayer = CollisionLayers.Hitboxes;
        CollisionMask  = CollisionLayers.Hurtboxes | CollisionLayers.HitboxColliders;
        raycast.CollisionMask = CollisionLayers.Hurtboxes | CollisionLayers.HitboxColliders;
        AreaEntered += OnAreaEntered;
        ProjectileManager.AddProjectile(this);
    }

    protected virtual void OnAreaEntered(Area3D area) {}

    private float elapsed = 0;
    public override void _PhysicsProcess(double delta)
    {
        Vector3 movement = Direction * Speed * (float)delta;
        raycast.TargetPosition = ToLocal(Direction + raycast.GlobalPosition) * 0.5f; 
        Position += movement;
        elapsed += (float)delta;
        if (elapsed >= Duration)
            Destroy();
    }
    public void Destroy()
    {
        ProjectileManager.RemoveProjectile(this);
        QueueFree();
    }
    public Vector3 PredictPosition(float foresight)
    {
        var Velocity = Direction * Speed;
        return GlobalPosition + Velocity * foresight;
    }
}
