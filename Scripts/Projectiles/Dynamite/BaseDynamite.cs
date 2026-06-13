using Godot;
public partial class BaseDynamite : Hurtbox
{
    public Node dynamiteFolder;
    public PackedScene explosionScene = GD.Load<PackedScene>("res://Assets/Scenes/Combat/Dynamite/explosionHitbox.tscn");
    public HurtType HitType;
    public override void _Ready()
    {
        base._Ready();
        HurtType = HurtType.Neutral;
        dynamiteFolder = GetNode<Node>("/root/main3d/Dynamite");
    }
    public void Explode()
    {
        var newExplosion = explosionScene.Instantiate<Hitbox>();
        newExplosion.Damage = 1;
        newExplosion.HurtType = HitType;
        dynamiteFolder.AddChild(newExplosion);
        newExplosion.Position = Position;
        GetTree().CreateTimer(0.1).Timeout += newExplosion.QueueFree;
    }

}
