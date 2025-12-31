using Godot;
using System;

public partial class Player : Node
{
    public bool debugMode = false;
    public int maxammo = 5;
    public int ammo = 5;
    public int dynamite = 2;
    public int maxdynamite = 2;
    public Character character;
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
    public void ReassignVars()
    {
        character = GetNode<Character>("Character");
        if (debugMode)
        {
            character.DebugMode();
            dynamiteReloadInterval = 0.01f;
            reloadInterval = 0.01f;
        }
        barrelEnd = GetNode<Node3D>("Character/Turret/BarrelEnd");
        gamecam = GetNode<Camera3D>("/root/main3d/Player/GameCamera");
        var offset = new Vector3(10f, 10f, 0); //because of orthogonal cameras just being better if you just set x and y to be the same the tank is always centered
        gamecam.Position = character.Position + offset;

        ammo = maxammo;
        dynamite = maxdynamite;
    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (ammo < maxammo)
        {
            reloadTimer -= (float)delta;
        }
        if (dynamite < maxdynamite)
        {
            dynamiteReloadTimer -= (float)delta;
        }
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
    public static void PrintProjectileList()
    {
        var ActiveProjectiles = PlayerProjectileManager.ActiveProjectiles;
        GD.Print($"Active Projectiles ({ActiveProjectiles.Count}):");
        for (int i = 0; i < ActiveProjectiles.Count; i++)
        {
            var proj = ActiveProjectiles[i];
            if (IsInstanceValid(proj))
            {
                GD.Print($"  [{i}] Valid projectile at {proj.GlobalPosition}");
            }
            else
            {
                GD.Print($"  [{i}] INVALID/DISPOSED projectile");
            }
        }
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (gamecam == null)
        {
            return;
        }
        var offset = new Vector3(10f, 10f, 0); //because of orthogonal cameras just being better if you just set x and y to be the same the tank is always centered
        gamecam.Position = gamecam.Position.Lerp(character.Position + offset, 0.1f);
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {
                Shoot();
            }
        }
        else if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Keycode == Key.Space && keyEvent.Pressed)
            {
                PlaceDynamite();
            }
        }

    }
    private PackedScene projectileScene = GD.Load<PackedScene>("res://Assets/Combat/Projectiles/bouncyBullet.tscn");
    private void Shoot()
    {
        if (ammo <= 0)
        {
            return;
        }
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
        newBullet.HurtType = "Enemy";
        newBullet.Damage = 1;

        PlayerProjectileManager.AddProjectile(newBullet);
    }
    private PackedScene timedDynamiteScene = GD.Load<PackedScene>("res://Assets/Combat/Dynamite/timedDynamite.tscn");
    private void PlaceDynamite()
    {
        if (dynamite <= 0)
        {
            return;
        }
        dynamite -= 1;
        var newTimedDynamite = timedDynamiteScene.Instantiate<TimedDynamite>();
        var dynamiteFolder = GetNode<Node>("/root/main3d/Dynamite");
        dynamiteFolder.AddChild(newTimedDynamite);
        newTimedDynamite.Type = "Enemy";
        newTimedDynamite.Duration = 5f;
        newTimedDynamite.Position = character.Position;
    }
}