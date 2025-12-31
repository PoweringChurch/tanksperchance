using Godot;
public partial class BaseDynamite : Hurtbox
{
    public Node dynamiteFolder;
    public PackedScene explosionScene = GD.Load<PackedScene>("res://Assets/Combat/Dynamite/explosionHitbox.tscn");
    public override void _Ready()
    {
        base._Ready();
        Type = "Dynamite";
        dynamiteFolder = GetNode<Node>("/root/main3d/Dynamite");
    }
    public void Explode(string hurtType)
    {
        var newExplosion = explosionScene.Instantiate<Hitbox>();
        newExplosion.Damage = 1;
        newExplosion.HurtType = hurtType;
        dynamiteFolder.AddChild(newExplosion);
        newExplosion.Position = Position;
        GetTree().CreateTimer(0.1).Timeout += () => newExplosion.QueueFree();
    }

}
