using Godot;
using System;

public partial class BaseEnemy : CharacterBody3D
{
    //public vars
    public int health = 1;
    public float movespeed = 4;
    public float rotationSpeed = 40;
    public bool IsMoving = false;
    public bool HasLos = false;
    public bool usePathfinding = true;
    public NavigationAgent3D navAgent;
    // protected vars
    protected MeshInstance3D bodyMesh;
    protected Hurtbox charHurtbox;
    protected Player player;
    protected Node3D turret;
    protected CharacterBody3D plrchar;
    protected Vector3 GoalPosition;
    protected LevelLoader levelLoader;
    protected Node3D barrelEnd;
    protected RayCast3D LosCast;
    public override void _Ready()
    {
        //debug
        //defining
        player = GetNode<Player>("/root/main3d/Player");
        turret = GetNode<Node3D>("Turret");
        charHurtbox = GetNode<Hurtbox>("Hurtbox");
        barrelEnd = GetNode<Node3D>("Turret/BarrelEnd");
        LosCast = GetNode<RayCast3D>("LOSCheck");
        levelLoader = GetNode<LevelLoader>("/root/main3d/LevelLoader");
        plrchar = levelLoader._objectRoot.GetNode<CharacterBody3D>("Character");
        bodyMesh = GetNode<MeshInstance3D>("Body");
        LosCast.AddException(plrchar);

        navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");

        // Configure the navigation agent
        navAgent.TargetDesiredDistance = 0.5f; // How close to get to target
        navAgent.PathDesiredDistance = 0.3f; // How close to path points
        navAgent.NavigationFinished += OnNavigationFinished;
        navAgent.TargetReached += OnTargetReached;
        navAgent.VelocityComputed += OnVelocityComputed;
        //init
        charHurtbox.HurtType = HurtType.Enemy;
        charHurtbox.OnHurt += Hurt;
        //MoveAndSlide();
        CallDeferred(MethodName.NavigationSetup);
    }
    private async void NavigationSetup()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }
    public override void _PhysicsProcess(double dt)
    {
        if (!player.playerAlive)
            return;
        LosCast.TargetPosition = ToLocal(plrchar.Position); //returns an error cause of queuefree, make it so that it checks if plrchar exists/ is disposed of and if it is return
        HasLos = !LosCast.IsColliding();
        if (IsMoving)
        {
            if (usePathfinding)
            {
                Vector3 nextPathPosition = navAgent.GetNextPathPosition();
                GoalPosition = nextPathPosition;
            }
            Move(dt);
        }
    }
    public void LookAtPosition(Vector3 targetPos, double dt)
    {
        targetPos = new Vector3(
            targetPos.X,
            turret.GlobalPosition.Y,
            targetPos.Z
        );

        // Calculate target direction (just Y rotation)
        Vector3 targetDirection = (turret.GlobalPosition - targetPos).Normalized();
        float targetAngle = Mathf.Atan2(targetDirection.X, targetDirection.Z);
        // Get current Y rotation
        float currentAngle = turret.Rotation.Y;
        // Calculate shortest rotation direction
        float angleDiff = Mathf.AngleDifference(currentAngle, targetAngle);
        // Rotate at constant speed towards target
        float rotationThisFrame = Mathf.DegToRad(rotationSpeed) * (float)GetProcessDeltaTime();

        if (Mathf.Abs(angleDiff) > rotationThisFrame)
        {
            // Still need to rotate
            float rotationDirection = Mathf.Sign(angleDiff);
            turret.Rotation = new Vector3(
                turret.Rotation.X,
                turret.Rotation.Y + (rotationDirection * rotationThisFrame),
                turret.Rotation.Z
            );
        }
        else
        {
            // Close enough, snap to final position
            turret.Rotation = new Vector3(turret.Rotation.X, targetAngle, turret.Rotation.Z);
        }
    }
    private float currentYaw;
    public void Move(double dt)
    {
        Vector3 dir = GoalPosition - Position;
        var dist = dir.Length();
        if (dist < 0.05)
        {
            IsMoving = false;
        }
        var velocity = Velocity;
        velocity.X = dir.X;
        velocity.Z = dir.Z;
        if (velocity.Normalized().Length() > 0.02f)
        {
            float targetYaw = Mathf.Atan2(velocity.X, velocity.Z);
            currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, 5f * (float)dt);
            bodyMesh.GlobalRotation = new Vector3(Mathf.DegToRad(-90), currentYaw, Mathf.DegToRad(90));
        }

        Velocity = velocity.Normalized() * movespeed;
        MoveAndSlide();
    }
    public void Hurt(Hitbox hitbox)
    {
        health -= hitbox.Damage;
        if (hitbox is BaseProjectile proj && proj.DestroyOnHit == true)
            proj.Destroy();
        if (health <= 0)
            Death();
    }
    //enemy actions
    public void Death()
    {
        QueueFree();
    }
    private PackedScene projectileScene = GD.Load<PackedScene>("res://Assets/Scenes/Combat/Projectiles/standardBullet.tscn");
    public void Shoot()
    {
        var newBullet = projectileScene.Instantiate<BaseProjectile>();
        GetNode<Node>("/root/main3d/Projectiles").AddChild(newBullet);
        newBullet.GlobalPosition = barrelEnd.GlobalPosition;
        newBullet.GlobalRotation = barrelEnd.GlobalRotation;
        Vector3 direction = -barrelEnd.GlobalTransform.Basis.Z;
        newBullet.Duration = 5;
        newBullet.Direction = direction.Normalized();
        newBullet.Speed = 5;
        newBullet.HurtType = HurtType.Friendly;
        newBullet.Damage = 1;
    }

    //pathfinding
    public void MoveTo(Vector3 target)
    {
        navAgent.TargetPosition = target;
        IsMoving = true;
    }

    private void OnVelocityComputed(Vector3 safeVelocity)
    {
        // Apply the safe velocity (avoids other agents/obstacles)
        Velocity = safeVelocity;
        MoveAndSlide();
    }
    // Called when navigation finishes
    private void OnNavigationFinished()
    {
        IsMoving = false;
    }
    // Called when target is reached
    private void OnTargetReached()
    {
        IsMoving = false;
    }
    public void MoveToRandomNearbyPoint(float radius = 10f)
    {
        Vector3 randomOffset = new Vector3(
            (float)GD.RandRange(-radius, radius),
            0,
            (float)GD.RandRange(-radius, radius)
        );
        Vector3 targetPos = GlobalPosition + randomOffset;
        MoveTo(targetPos);
    }
}
