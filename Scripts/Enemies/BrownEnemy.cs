using Godot;
using System;

public partial class BrownEnemy : BaseEnemy
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
        rotationSpeed = 40f;
        movespeed = 2;
        usePathfinding = true;
        currentBehavior = (int)Behaviors.Charge;
    }
    public override void _PhysicsProcess(double dt)
    {
        base._PhysicsProcess(dt);
        if (!player.playerAlive)
            return;
        LookAtPosition(plrchar.Position, dt);
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
            MoveTo(plrchar.Position);
        }
        Vector3 targetDirection = (turret.GlobalPosition - plrchar.Position).Normalized();
        float targetAngle = Mathf.Atan2(targetDirection.X, targetDirection.Z);
        float currentAngle = turret.Rotation.Y;
        float angleDiff = Mathf.AngleDifference(currentAngle, targetAngle);
        if (CanShootAtTarget(angleDiff))
        {
            Shoot();
            shootTimer = shootCd;
        }
    }
    private bool CanShootAtTarget(float angleDiff)
    {
        return Mathf.Abs(angleDiff) < Mathf.DegToRad(rotationSpeed) * (float)GetProcessDeltaTime() * 1.4f
            && shootTimer <= 0
            && HasLos
            && losShootTimer <= 0;
    }
}
