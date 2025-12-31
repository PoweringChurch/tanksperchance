using Godot;
using System;
[GlobalClass]
public partial class Hurtbox : Area3D
{
	public delegate void HurtEventHandler(Hitbox hitbox);
	public event HurtEventHandler OnHurt;
	[Export]
	public string Type { get; set; }
	public Hurtbox()
	{
		CollisionLayer = 0;
		CollisionMask = 2;
		AreaEntered += OnAreaEntered;
	}

	protected virtual void OnAreaEntered(Area3D area)
	{
		if (area is Hitbox hitbox && hitbox.HurtType == Type)
		{
			OnHurt?.Invoke(hitbox);
		}
	}

}
