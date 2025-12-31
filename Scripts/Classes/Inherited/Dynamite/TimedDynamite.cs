using Godot;
using System;
using System.Threading.Tasks;

public partial class TimedDynamite : BaseDynamite
{
    public float Duration;
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Duration -= (float)delta;
        if (Duration <= 0)
        {
            Explode(Type);
            QueueFree();
        }
    }

    protected override void OnAreaEntered(Area3D area)
    {
        if (area is Hitbox hitbox && (hitbox.HurtType == "Enemy" || hitbox.HurtType == "Player"))
        {
            if (hitbox is BaseProjectile proj && proj.DestroyOnHit)
            {
                proj.QueueFree();
            }
            Explode(hitbox.HurtType);
            QueueFree();
        }
    }
}
