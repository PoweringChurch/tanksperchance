using Godot;
using System;

public enum HurtType{ Enemy, Friendly, Neutral, None}
[GlobalClass]
public partial class Hitbox : Area3D
{
	[Export]
	public int Damage { get; set; }
	[Export]
	public HurtType HurtType { get; set; }
	public Hitbox()
	{
		CollisionLayer = CollisionLayers.Hitboxes;
		CollisionMask = CollisionLayers.Hurtboxes;
	}
}
