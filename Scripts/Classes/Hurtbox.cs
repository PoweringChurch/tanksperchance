using Godot;
using System;
[GlobalClass]
public partial class Hurtbox : Area3D
{
	public delegate void HurtEventHandler(Hitbox hitbox);
	public event HurtEventHandler OnHurt;
	[Export]
	public HurtType HurtType { get; set; }
	public Hurtbox()
	{
		CollisionLayer = CollisionLayers.Hurtboxes;
		AreaEntered += OnAreaEntered;
	}

	protected virtual void OnAreaEntered(Area3D area)
	{
		if (area is Hitbox hitbox && hitbox.HurtType == HurtType)
		{
			OnHurt?.Invoke(hitbox);
		}
	}

}
