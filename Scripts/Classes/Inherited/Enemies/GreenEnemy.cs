using Godot;
using System;

public partial class GreenEnemy : BaseEnemy
{
    public enum Behaviors { Charge, Linger };
    public float goalDistance = 12f;
    public float distTolerance = 1f;
    public int currentBehavior;
    private float confidence;
    private float thinkTimer = 0.5f;
    private float shootTimer = 0f;
    private float losShootTimer = 0.25f;
    private float thinkCd = 0.3f;
    private float shootCd = 1f;
    private float losShootCd = 0.5f;
    public override void _Ready()
    {
        base._Ready();
        rotationSpeed = 120f;
        movespeed = 4;
        usePathfinding = true;
        currentBehavior = (int)Behaviors.Charge;
    }
    public override void _PhysicsProcess(double dt)
    {
        base._PhysicsProcess(dt);
        shootTimer -= (float)dt;
        thinkTimer -= (float)dt;
        if (HasLos)
        {
            confidence = Mathf.Clamp(confidence - 2 * (float)dt,-10,10);
            losShootTimer -= (float)dt;
        }
        else
        {
            confidence = Mathf.Clamp(confidence + 3 * (float)dt,-10,10);
            losShootTimer = losShootCd;
        }
        if (thinkTimer <= 0)
        {
            thinkTimer = thinkCd;
            goalDistance = 10f * (0.7f - (Mathf.Atan(confidence * 0.3f) / 2.4f));
            //goalDistance = Mathf.Clamp(goalDistance, 4, 15);
            DetermineBehavior();
        }
    }
    public void DetermineBehavior()
    {
        float currentDistance = (Position - plrchar.Position).Length();
        if (Math.Abs(currentDistance - goalDistance) <= distTolerance)
        {
            currentBehavior = (int)Behaviors.Linger;
            Linger();
        }
        else
        {
            currentBehavior = (int)Behaviors.Charge;
            Reposition();
        }
    }
    private bool CanShootAtTarget(float angleDiff)
    {
        return Mathf.Abs(angleDiff) < Mathf.DegToRad(rotationSpeed) * (float)GetProcessDeltaTime() * 1.4f
            && shootTimer <= 0
            && HasLos
            && losShootTimer <= 0;
    }
    public void Reposition()
    {
        float currentDistance = (Position - plrchar.Position).Length();

        if (currentDistance > goalDistance + distTolerance)
        {
            // Too far - move closer to player
            Vector3 directionToPlayer = (plrchar.Position - Position).Normalized();
            Vector3 targetPos = plrchar.Position - (directionToPlayer * goalDistance);

            // Add some randomness to avoid clustering
            Vector3 randomOffset = new Vector3(
                (float)GD.RandRange(-2f, 2f),
                0,
                (float)GD.RandRange(-2f, 2f)
            );
            targetPos += randomOffset;

            MoveTo(targetPos);
        }
        else if (currentDistance < goalDistance - distTolerance)
        {
            // Too close - back away from player
            Vector3 directionFromPlayer = (Position - plrchar.Position).Normalized();
            Vector3 targetPos = plrchar.Position + (directionFromPlayer * goalDistance);

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
    public void Linger()
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
        Vector3 targetPos = Position + (strafeDir * (float)GD.RandRange(1f, 3f));
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
        
        Vector3 targetPos = plrchar.Position + (newDir * goalDistance);
        MoveTo(targetPos);
    }
}
