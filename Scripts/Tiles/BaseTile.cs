using Godot;
using System;

public static class CollisionLayers
{
    public const uint Hurtboxes          = 1 << 0; // layer 1
    public const uint Hitboxes         = 1 << 1; // layer 2
    public const uint HurtboxColliders      = 1 << 2; // layer 3
    public const uint HitboxColliders     = 1 << 3; // layer 4
}
public partial class BaseTile : StaticBody3D
{
    public bool characterCollide;
    public bool projectileCollide;
    public int durability;
    public int destroyThreshold;
    public int damage = 0;
    public override void _Ready()
    {
        CollisionLayer = 0;
        CollisionMask  = 0;
        AddToGroup("navigation");
        if (characterCollide)
        {
            CollisionLayer |= CollisionLayers.HurtboxColliders;
            float r = 0.5f;
            var navObstacle = new NavigationObstacle3D()
            {
                AffectNavigationMesh = true,
                CarveNavigationMesh = true,
                Vertices = [
                    new(-r,0,-r), new(r,0,-r),
                    new(r,0,r),   new(-r,0,r)
                ],
                Height = 1,
                AvoidanceEnabled = true
            };
            AddChild(navObstacle);
        }
        if (projectileCollide) CollisionLayer |= CollisionLayers.HitboxColliders;
    }
    public void Damage(int by)
    {
        damage += Math.Max(-durability + by, 0); // if it takes negative damage, dont heal wall
        if (damage >= destroyThreshold)
            QueueFree(); // destroy
    }
    public override void _ExitTree()
    {
        base._ExitTree();
        var levelLoader = GetNode<LevelLoader>("/root/main3d/LevelLoader");
        levelLoader._currentNavRegion.BakeNavigationMesh();
    }

}