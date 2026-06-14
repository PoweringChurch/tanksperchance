using System;
using Godot;

public partial class Player : Node
{
	// public
	public bool debugMode = true;
	public int maxammo = 5;
	public int ammo = 5;
	public int dynamite = 2;
	public int maxdynamite = 2;
	public Character character;
	public bool playerAlive = false;

	public float camMinX = 0;
	public float camMaxX = 0;
	public float camMinZ = 0;
	public float camMaxZ = 0;

	// private
	private Camera3D gamecam;
	private Node3D barrelEnd;
	private AudioStreamPlayer2D reloadsfx;
	private AudioStreamPlayer2D shootsfx;
	private float reloadTimer = 1;
	private float reloadInterval = 1;
	private float dynamiteReloadTimer = 5;
	private float dynamiteReloadInterval = 5;

	private enum PlayerModes
	{
		UI = 0,
		Ingame = 1,
		Editor = 2,
	};
	private int playermode = 0;
	public override void _Ready()
	{
		reloadsfx = GetNode<AudioStreamPlayer2D>("SFX/Reload");
		shootsfx = GetNode<AudioStreamPlayer2D>("SFX/Shoot");
	}
	public void StartPlayer(Node objectRoot)
	{
		playerAlive = true;
		character = objectRoot.GetNode<Character>("Character");

		character.charHurtbox.HurtType = debugMode ? HurtType.None : HurtType.Friendly;
		character.movespeed = debugMode ? 8 : 3;
		dynamiteReloadInterval = debugMode ? 0.01f : 5;
		reloadInterval = debugMode ? 0.01f : 1;

		barrelEnd = objectRoot.GetNode<Node3D>("Character/Turret/BarrelEnd");
		gamecam = GetNode<Camera3D>("/root/main3d/Player/GameCamera");
		var offset = new Vector3(10f, 10f, 0); //because of orthogonal cameras just being better if you just set x and y to be the same the tank is always centered
		gamecam.Position = character.Position + offset;

		ammo = maxammo;
		dynamite = maxdynamite;

		character.charHurtbox.OnHurt += CheckHealth;
	}
	private void CheckHealth(Hitbox hitbox)
	{
		if (character.health <= 0)
		{
			playerAlive = false;
			character.QueueFree();
			character.charHurtbox.OnHurt -= CheckHealth;
		}
	}
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		if (!playerAlive) return;
		if (ammo < maxammo)
			reloadTimer -= (float)delta;
		if (dynamite < maxdynamite)
			dynamiteReloadTimer -= (float)delta;
		if (reloadTimer <= 0)
		{
			reloadsfx.Play();
			ammo += 1;
			reloadTimer = reloadInterval;
		}
		if (dynamiteReloadTimer <= 0)
		{
			dynamite += 1;
			dynamiteReloadTimer = dynamiteReloadInterval;
		}
	}
	public override void _Process(double delta)
	{
		base._Process(delta);
		if (!playerAlive) return;
		if (gamecam == null)
			return;
		var offset = new Vector3(0, 10f, 10f);
    	Vector3 targetCamPos = character.Position + offset;
		float clampedX = Math.Clamp(targetCamPos.X, camMinX, camMaxX);
		float clampedZ = Math.Clamp(targetCamPos.Z, camMinZ, camMaxZ);
		gamecam.Position = new Vector3(clampedX, offset.Y, clampedZ);
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		if (!playerAlive) return;
		if (@event is InputEventMouseButton mouseEvent)
			if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
				Shoot();
		if (@event is InputEventKey keyEvent)
			if (keyEvent.Keycode == Key.E && keyEvent.Pressed)
				PlaceDynamite();

	}
	private PackedScene projectileScene = GD.Load<PackedScene>("res://Assets/Scenes/Combat/Projectiles/bouncyBullet.tscn");
	private void Shoot()
	{
		if (ammo <= 0)
			return;
		shootsfx.Play();
		ammo -= 1;
		reloadTimer = reloadInterval;
		var newBullet = projectileScene.Instantiate<BaseProjectile>();
		var projectileFolder = GetNode<Node>("/root/main3d/Projectiles");
		projectileFolder.AddChild(newBullet);
		newBullet.GlobalPosition = barrelEnd.GlobalPosition;
		newBullet.GlobalRotation = barrelEnd.GlobalRotation;
		Vector3 direction = -barrelEnd.GlobalTransform.Basis.Z;
		newBullet.Duration = 5;
		newBullet.Direction = direction.Normalized();
		newBullet.Speed = 5;
		newBullet.HurtType = HurtType.Enemy;
		newBullet.Damage = 1;
	}
	private PackedScene timedDynamiteScene = GD.Load<PackedScene>("res://Assets/Scenes/Combat/Dynamite/timedDynamite.tscn");
	private void PlaceDynamite()
	{
		if (dynamite <= 0)
			return;
		dynamite -= 1;
		var newTimedDynamite = timedDynamiteScene.Instantiate<TimedDynamite>();
		var dynamiteFolder = GetNode<Node>("/root/main3d/Dynamite");
		dynamiteFolder.AddChild(newTimedDynamite);
		newTimedDynamite.Duration = 5f;
		newTimedDynamite.HitType = HurtType.Enemy;
		newTimedDynamite.Position = character.Position;
	}
}
