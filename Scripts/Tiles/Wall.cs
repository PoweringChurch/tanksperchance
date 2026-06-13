using Godot;
using System;

public partial class Wall : BaseTile
{
    public override void _Ready()
    {
        characterCollide = true;
        projectileCollide = true;
        destroyThreshold = 3;
        durability = 1;
        base._Ready();
    }
}
