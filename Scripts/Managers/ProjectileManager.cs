using Godot;
using System.Collections.Generic;

public partial class ProjectileManager : Node
{
    public static ProjectileManager Instance { get; private set; }
    public static List<BaseProjectile> ActiveProjectiles { get; private set; } = new();
    public static void AddProjectile(BaseProjectile proj)
    {
        ActiveProjectiles.Add(proj);
    }
    public static void RemoveProjectile(BaseProjectile proj)
    {
        ActiveProjectiles.Remove(proj);
    }
    public static List<BaseProjectile> GetProjectilesNear(Vector3 position, float radius)
    {
        var nearby = new List<BaseProjectile>();
        foreach (var projectile in ActiveProjectiles)
            if (!IsInstanceValid(projectile)) {
                RemoveProjectile(projectile);
                continue;
            }
            else if (projectile.GlobalPosition.DistanceTo(position) <= radius)
                nearby.Add(projectile);
            
        return nearby;
    }
}
