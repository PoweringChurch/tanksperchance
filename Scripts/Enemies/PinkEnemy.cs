using Godot;
using System;
using System.Runtime.InteropServices;

public partial class PinkEnemy : BaseEnemy
{
    public enum Behaviors { Reposition, Linger, Evacuate };
    public int currentBehavior;
    public float acceptableDistance = 10f;
    public float distTolerance = 3f;
    public float foresight = 2f;
    public float detectRadius = 3f;
    public float avoidanceRadius = 3.0f;
    private float patience;
    private float patienceFactor;
    //timers
    private float thinkTimer;
    private float shootTimer;
    private float losShootTimer = 0.5f;
    //intervals
    private float shootInterval = 1f;
    private float thinkInterval = 0.3f;
    private float losShootInterval = 0.5f;
    private Vector3 lastSeenPos;

    private MeshInstance3D debugsphere;

    public override void _Ready()
    {
        base._Ready();
        rotationSpeed = 60f;
        movespeed = 3;
        usePathfinding = true;
        currentBehavior = (int)Behaviors.Reposition;

        debugsphere = GetNode<MeshInstance3D>("/root/main3d/DebugSphere");
    }
    public override void _PhysicsProcess(double dt)
    {
        base._PhysicsProcess(dt);
        if (!player.playerAlive)
            return;
        //Debug3D.VisualizeSphere(plrchar.Position, acceptableDistance, null, true, 0.01f);
        LookAtPosition(plrchar.Position, dt);
        shootTimer -= (float)dt;
        thinkTimer -= (float)dt;
        if (HasLos)
        {
            patience += 3 * (float)dt;
            losShootTimer -= (float)dt;
            lastSeenPos = plrchar.Position;
            Vector3 targetDirection = (turret.GlobalPosition - plrchar.Position).Normalized();
            float targetAngle = Mathf.Atan2(targetDirection.X, targetDirection.Z);
            float currentAngle = turret.Rotation.Y;
            float angleDiff = Mathf.AngleDifference(currentAngle, targetAngle);
            if (CanShootAtTarget(angleDiff))
            {
                Shoot();
                shootTimer = shootInterval;
            }
        }
        else
        {
            patience -= (float)dt;
            losShootTimer = losShootInterval;
        }
        if (thinkTimer <= 0)
        {
            thinkTimer = thinkInterval;
            patienceFactor = (Mathf.Atan(patience * 0.3f) / 4) + 0.7f;
            acceptableDistance = 10f * patienceFactor;
            DetermineBehavior();
        }
        AvoidIncomingProjectile();

    }
    private bool CanShootAtTarget(float angleDiff)
    {
        return Mathf.Abs(angleDiff) < Mathf.DegToRad(rotationSpeed) * (float)GetProcessDeltaTime() * 1.4f
            && shootTimer <= 0
            && HasLos
            && losShootTimer <= 0;
    }
    private void DetermineBehavior()
    {
        float currentDistance = (Position - plrchar.Position).Length();
        if (Math.Abs(currentDistance - acceptableDistance) <= distTolerance) //if within acceptable distance
        {
            currentBehavior = (int)Behaviors.Linger;
            Linger();
        }
        else if (currentDistance - acceptableDistance < distTolerance * -1.4f) //if *really* not within acceptable distance (lower)
        {
            currentBehavior = (int)Behaviors.Evacuate;
            Evacuate();
        }
        else if (Math.Abs(currentDistance - acceptableDistance) > distTolerance) //if not within acceptable distance (lower/higher)
        {
            currentBehavior = (int)Behaviors.Reposition;
            Reposition(currentDistance);
        }
    }
    private void Evacuate()
    {
        Vector3 oppositeSide = 2f * (Position - plrchar.Position);
        MoveTo(oppositeSide);
    }
    private void Reposition(float currentDistance)
    {
        if (currentDistance > acceptableDistance + distTolerance || !HasLos)
        {
            // Too far - move closer to player
            Vector3 directionToPlayer = (lastSeenPos - Position).Normalized();
            Vector3 targetPos = lastSeenPos - (directionToPlayer * acceptableDistance * patienceFactor);

            // Add some randomness to avoid clustering
            Vector3 randomOffset = new Vector3(
                (float)GD.RandRange(-2f, 2f),
                0,
                (float)GD.RandRange(-2f, 2f)
            );
            targetPos += randomOffset;

            MoveTo(targetPos);
        }
        else if (currentDistance < acceptableDistance - distTolerance)
        {
            // Too close - back away from player
            Vector3 directionFromPlayer = (Position - plrchar.Position).Normalized();
            Vector3 targetPos = plrchar.Position + (directionFromPlayer * acceptableDistance);

            // Add some randomness
            Vector3 randomOffset = new Vector3(
                (float)GD.RandRange(-2f, 2f),
                0,
                (float)GD.RandRange(-2f, 2f)
            );
            targetPos += randomOffset;

            MoveTo(targetPos);
        }
    }
    private void AvoidIncomingProjectile()
    {
        BaseProjectile priorityProjectile = null;
        float shortestDistance = 100f;
        foreach (var projectile in ProjectileManager.ActiveProjectiles)
        {
            if (projectile.HurtType != HurtType.Enemy) 
                return;
            var projectileDistance = (projectile.GlobalPosition - GlobalPosition).Length();
            if (projectileDistance < shortestDistance)
            {
                shortestDistance = projectileDistance;
                priorityProjectile = projectile;
            }
        }
        if (priorityProjectile == null) return;
        //Debug3D.VisualizePoint(priorityProjectile.Position, Colors.Aqua, 3, 0.01f);
        if (GlobalPosition.DistanceTo(priorityProjectile.GlobalPosition) <= detectRadius)
        {
            // Check if projectile is moving toward us
            Vector3 toEnemy = (GlobalPosition - priorityProjectile.GlobalPosition).Normalized();
            Vector3 projectileDirection = priorityProjectile.Direction;

            float dot = toEnemy.Dot(projectileDirection);
            if (dot > 0.4f) // Projectile is heading roughly toward us
            {
                if (WillProjectileHit(priorityProjectile))
                {
                    var predictedPos = priorityProjectile.PredictPosition(foresight);
                    Vector3 avoidDirection = (predictedPos- GlobalPosition).Normalized().Cross(Vector3.Up);
                    Debug3D.VisualizePoint(avoidDirection*1.5f + GlobalPosition, null, 0.5f, 0.01f);
                    MoveTo(avoidDirection + Position);
                }
            }
        }
    }
    private bool WillProjectileHit(BaseProjectile projectile)
    {
        float step = 0.1f;
        for (float t = 0; t < foresight; t += step)
        {
            Vector3 futurePos = projectile.PredictPosition(t);
            //Debug3D.VisualizePoint(futurePos,null,1,0.2f);
            if (futurePos.DistanceTo(GlobalPosition) <= 0.5) //is the dist closer than radius 0.5
                return true;
        }
        return false;
    }
    private void Linger()
    {
        int lingerAction = GD.RandRange(0, 4);
        switch (lingerAction)
        {
            case 0:
                // Stay put and focus on shooting
                break;

            case 1:
                // Strafe left
                StrafeMovement(-1);
                break;

            case 2:
                // Strafe right  
                StrafeMovement(1);
                break;

            case 3:
                // Small repositioning move
                SmallRepositioning();
                break;
        }
    }
    private void StrafeMovement(int direction)
    {
        // Get perpendicular direction to player
        Vector3 toPlayer = (plrchar.Position - Position).Normalized();
        Vector3 strafeDir = new Vector3(-toPlayer.Z * direction, 0, toPlayer.X * direction);
        
        // Move to a strafe position
        Vector3 targetPos = Position + (strafeDir * (float)GD.RandRange(3f, 6f));
        MoveTo(targetPos);
        
    }
    private void SmallRepositioning()
    {
        // Small random movement while maintaining distance
        Vector3 currentDir = (Position - plrchar.Position).Normalized();
        
        // Rotate the direction slightly
        float angleOffset = (float)GD.RandRange(-45f, 45f) * Mathf.Pi / 180f;
        Vector3 newDir = new Vector3(
            currentDir.X * Mathf.Cos(angleOffset) - currentDir.Z * Mathf.Sin(angleOffset),
            0,
            currentDir.X * Mathf.Sin(angleOffset) + currentDir.Z * Mathf.Cos(angleOffset)
        );
        
        Vector3 targetPos = plrchar.Position + (newDir * acceptableDistance);
        MoveTo(targetPos);
    }
}
