using Godot;

public partial class TimedDynamite : BaseDynamite
{
    public float Duration;
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Duration -= (float)delta;
        if (Duration <= 0)
        {
            Explode();
            QueueFree();
        }
    }
    protected override void OnAreaEntered(Area3D area)
    {
        if (area is Hitbox hitbox)
        {
            HitType = hitbox.HurtType;
            if (hitbox is BaseProjectile proj && proj.DestroyOnHit)
                proj.QueueFree();
            Explode();
            QueueFree();
        }
    }
}
