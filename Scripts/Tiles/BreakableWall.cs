using Godot;
using System;

public partial class BreakableWall : BaseTile
{
    public override void _Ready()
    {
        characterCollide = true;
        projectileCollide = true;
        destroyThreshold = 2;
        durability = 0;
        base._Ready();
    }
}
