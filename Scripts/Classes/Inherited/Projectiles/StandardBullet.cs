using Godot;
using System;
using System.Collections;

public partial class StandardBullet : BaseProjectile
{
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
            Destroy();
        }
    }

    public void OnAreaEntered(Area3D area)
    {
        if (area is BaseProjectile proj && proj.HurtType != HurtType)
        {
            proj.Destroy();
        }
    }
}
