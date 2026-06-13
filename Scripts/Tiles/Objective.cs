using Godot;
using System;

public partial class Objective :  BaseTile
{
    public override void _Ready()
    {
        characterCollide = true;
        projectileCollide = true;
        destroyThreshold = 3;
        durability = 2;
    }
}
