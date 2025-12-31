using Godot;
using System;

public partial class GreyEnemy : BaseEnemy
{
    private float shootTimer = 3f;
    private float thinkTimer = 0.5f;
    private float losShootTimer = 1f;
    private float shootInterval = 3f;
    private float thinkInterval = 2f;
    private float losShootInterval = 0.9f;
    public override void _Ready()
    {
        base._Ready();
        movespeed = 2;
        rotationSpeed = 30f;
    }

    public override void _PhysicsProcess(double dt)
    {
        base._PhysicsProcess(dt);
        LookAtPosition(plrchar.Position, dt);
        shootTimer -= (float)dt;
        thinkTimer -= (float)dt;
        if (HasLos) losShootTimer -= (float)dt;
        else losShootTimer = losShootInterval;
        Vector3 targetDirection = (turret.GlobalPosition - plrchar.Position).Normalized();
        float targetAngle = Mathf.Atan2(targetDirection.X, targetDirection.Z);
        float currentAngle = turret.Rotation.Y;
        float angleDiff = Mathf.AngleDifference(currentAngle, targetAngle);
        if (CanShootAtTarget(angleDiff))
        {
            shootTimer = shootInterval;
            Shoot();
        }
        if (thinkTimer <= 0)
        {
            thinkTimer = thinkInterval+(float)GD.RandRange(-1.5f, 1.5f);
            RandomRelocate();
        }
    }

    private bool CanShootAtTarget(float angleDiff)
    {
        return Mathf.Abs(angleDiff) < Mathf.DegToRad(rotationSpeed) * (float)GetProcessDeltaTime() * 2.4f
            && shootTimer <= 0
            && HasLos
            && losShootTimer <= 0;
    }

    private void RandomRelocate()
    {
        Vector3 randomOffset = new Vector3(
                (float)GD.RandRange(-5f, 5f),
                0,
                (float)GD.RandRange(-5f, 5f)
            );
        MoveTo(randomOffset + plrchar.Position);
    }
}
