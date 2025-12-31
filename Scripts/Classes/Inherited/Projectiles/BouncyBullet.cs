using Godot;
using System;

public partial class BouncyBullet : BaseProjectile
{
    public int MaxBounces = 1;
    private int currentBounces = 0;

    public override void _Ready()
    {
        base._Ready();
        //detect other bullets
        CollisionLayer = 2 | 3;
        CollisionMask = 3;
        AreaEntered += OnAreaEntered;
    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (raycast.IsColliding())
        {
            CollisionObject3D collisionInstance = (CollisionObject3D)raycast.GetCollider();
            if (collisionInstance.IsClass("CharacterBody3D"))
            {
                return;
            }
            if (currentBounces == MaxBounces) Destroy();
            // Get collision info
            var collisionPoint = raycast.GetCollisionPoint();
            var normal = raycast.GetCollisionNormal();
            Direction = Direction.Bounce(normal.Normalized());
            // Move bullet slightly away from collision point to prevent getting stuck
            GlobalPosition = collisionPoint + normal * 0.05f;

            // Update bullet direction/rotation to match new velocity
            if (Direction.Length() > 0)
            {
                LookAt(GlobalPosition + Direction.Normalized(), Vector3.Up);
            }

            currentBounces++;
            // Optional: Add bounce effect (sound, particles, etc.)
            OnBounce();
        }
    }
    public void OnAreaEntered(Area3D area)
    {
        if (area is BaseProjectile proj && proj.HurtType != HurtType)
        {
            proj.Destroy();
        }
    }
    protected virtual void OnBounce()
    {}
}
