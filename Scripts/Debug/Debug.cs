using Godot;
using System;

public partial class Debug : Node
{
    private Node3D main3d;
    private Stage currentStage;
    private Node enemiesFolder;
    private Node projectilesFolder;
    private Node dynamiteFolder;
    private Player plr;
    public override void _Ready()
    {
        main3d = GetNode<Node3D>("/root/main3d");
        enemiesFolder = GetNode<Node>("/root/main3d/Enemies");
        projectilesFolder = GetNode<Node>("/root/main3d/Projectiles");
        dynamiteFolder = GetNode<Node>("/root/main3d/Dynamite");
        plr = GetNode<Player>("/root/main3d/Player");
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Keycode == Key.R && keyEvent.Pressed)
            {
                StartStage("stage_testing");
            }
        }

    }
    public void QueueFreeChildren(Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is BaseProjectile proj)
            {
                proj.Destroy();
                continue;
            }
            child.QueueFree();
        }
    }
    public void StartStage(string stageName)
    {
        QueueFreeChildren(enemiesFolder);
        QueueFreeChildren(projectilesFolder);
        QueueFreeChildren(dynamiteFolder);
        currentStage?.QueueFree();

        var stageScene = GD.Load<PackedScene>($"res://Assets/Stages/{stageName}.tscn");
        currentStage = (Stage)stageScene.Instantiate();
        main3d.AddChild(currentStage);
        currentStage.InitStage();
    }
}
