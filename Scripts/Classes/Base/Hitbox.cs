using Godot;
using System;

[GlobalClass]
public partial class Hitbox : Area3D
{
	[Export]
	public int Damage { get; set; }
	[Export]
	public string HurtType { get; set; }
	public Hitbox()
	{
		CollisionLayer = 2;
		CollisionMask = 0;
	}
}
