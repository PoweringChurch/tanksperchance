using System;
using Godot;

[GlobalClass]
public partial class Character : CharacterBody3D
{
    public int health = 1;
    public int movespeed = 4;

    public Camera3D gameCamera;
    public CharacterBody3D character;
    public Node3D turret;
    public CollisionShape3D charCollision;
    public Hurtbox charHurtbox;
    public MeshInstance3D bodyMesh;
    public MeshInstance3D debugSphere;
    public override void _Ready()
    {
        gameCamera = GetNode<Camera3D>("/root/main3d/Player/GameCamera");

        character = this; //ik that this isnt necessary but i like it for readability
        turret = GetNode<Node3D>("Turret");
        charCollision = GetNode<CollisionShape3D>("Collision");
        bodyMesh = GetNode<MeshInstance3D>("Body");
        charHurtbox = GetNode<Hurtbox>("Hurtbox");
        charHurtbox.OnHurt += Hurt;

        debugSphere = GetNode<MeshInstance3D>("/root/main3d/DebugSphere");
    }
    public override void _Process(double delta)
    {
        CharacterMovement(delta);
        FollowMouse();
    }
    public void Hurt(Hitbox hitbox)
    {
        GD.Print(hitbox.Name);
        health -= hitbox.Damage;
        if (hitbox is BaseProjectile proj && proj.DestroyOnHit == true)
        {
            hitbox.QueueFree();
        }
        if (health <= 0)
        {
            Death();
        }
    }
    private float currentYaw;
    public void CharacterMovement(double dt)
    {
        var plrinput = Vector3.Zero;
        plrinput.X = Input.GetAxis("moveleft", "moveright");
        plrinput.Z = Input.GetAxis("moveforward", "movebackward");
        var movedir = gameCamera.Basis * plrinput;
        var velocity = character.Velocity;
        velocity.X = movedir.X;
        velocity.Z = movedir.Z;
        
        // Determine if we're moving forward or backward
        bool isMovingBackward = plrinput.Z < 0; // Assuming negative Z is backward input
        
        // Calculate speed with direction
        float speed = velocity.Normalized().Length() * movespeed;
        if (isMovingBackward)
            speed = -speed; // Negative speed for backward movement
        
        character.Velocity = velocity.Normalized() * Math.Abs(speed);
        
        if (velocity.Normalized().Length() > 0.02f)
        {
            // Always rotate towards the movement direction, but consider forward/backward
            Vector3 rotationDirection = isMovingBackward ? -velocity.Normalized() : velocity.Normalized();
            float targetYaw = Mathf.Atan2(rotationDirection.X, rotationDirection.Z);
            currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, 6f * (float)dt);
            bodyMesh.GlobalRotation = new Vector3(Mathf.DegToRad(-90), currentYaw, Mathf.DegToRad(90));
        }
        
        character.MoveAndSlide();
    }
    public void FollowMouse()
    {
        var mousexy = GetViewport().GetMousePosition();
        var rayOrigin = gameCamera.ProjectRayOrigin(mousexy);
        var rayDirection = gameCamera.ProjectRayNormal(mousexy);

        float targetY = turret.GlobalPosition.Y; // Or whatever Y you want
        float t = (targetY - rayOrigin.Y) / rayDirection.Y;
        Vector3 targetPos = rayOrigin + rayDirection * t;

        turret.LookAt(targetPos, Vector3.Up);
    }
    public Vector3 ScreenPointToRay()
    {
        var spaceState = GetWorld3D().DirectSpaceState;
        var mousexy = GetViewport().GetMousePosition();
        var rayOrigin = gameCamera.ProjectRayOrigin(mousexy);
        var rayEnd = rayOrigin + gameCamera.ProjectRayNormal(mousexy) * 2000;
        PhysicsRayQueryParameters3D parameters3D = new PhysicsRayQueryParameters3D();
        parameters3D.From = rayOrigin;
        parameters3D.To = rayEnd;
        var rayArray = spaceState.IntersectRay(parameters3D);
        if (rayArray.ContainsKey("position"))
        {
            return (Vector3)rayArray["position"];
        }
        return Vector3.Zero;
    }
    public void DebugMode()
    {
        charHurtbox.Type = "Debug";
        movespeed = 8;
        charCollision.QueueFree();
    }
    public void Death()
    {
        QueueFree();
    }
}
