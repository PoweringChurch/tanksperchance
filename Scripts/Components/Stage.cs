using Godot;
using Godot.Collections;
using System;

[GlobalClass]
public partial class Stage : Node3D
{
    public Array<Node> InitSpawns;
    public Array<Node> PostCollectionSpawns;
    public Node enemiesFolder;
    public Marker3D playerSpawn;
    private Player player;
    public override void _Ready()
    {
        base._Ready();
        InitSpawns = GetNode<Node3D>("InitSpawns").GetChildren();
        PostCollectionSpawns = GetNode<Node3D>("PostCollectionSpawns").GetChildren();
        enemiesFolder = GetNode<Node>("/root/main3d/Enemies");
        player = GetNode<Player>("/root/main3d/Player");
        playerSpawn = GetNode<Marker3D>("PlayerSpawn");
    }
    private PackedScene charScene = GD.Load<PackedScene>("res://Assets/Player/Character/character.tscn");
    public void PrintChildren(Node node)
    {
        foreach (Node child in node.GetChildren()) GD.Print(child.Name);
    }
    public void InitStage()
    {
        GetNode<Character>("/root/main3d/Player/Character")?.Free();
        var playerCharacter = charScene.Instantiate<Character>();
        player.AddChild(playerCharacter);
        playerCharacter.Position = playerSpawn.Position;
        player.ReassignVars();
        foreach (var node in InitSpawns)
        {
            if (node is EnemySpawnPoint spawnPoint)
            {
                string enemyType = spawnPoint.EnemyType;
                Vector3 spawnPosition = spawnPoint.GlobalTransform.Origin;
                SpawnEnemy(enemyType, spawnPosition);
            }
        }
    }
    public void StartPostCollection()
    {
        foreach (var node in PostCollectionSpawns)
        {
            if (node is EnemySpawnPoint spawnPoint)
            {
                string enemyType = spawnPoint.EnemyType;
                Vector3 spawnPosition = spawnPoint.GlobalTransform.Origin;
                SpawnEnemy(enemyType, spawnPosition);
            }
        }
    }
    private PackedScene GreenEnemy = GD.Load<PackedScene>("res://Assets/Enemies/Models/GreenEnemy.tscn");
    private PackedScene PinkEnemy = GD.Load<PackedScene>("res://Assets/Enemies/Models/PinkEnemy.tscn");
    private PackedScene GreyEnemy = GD.Load<PackedScene>("res://Assets/Enemies/Models/GreyEnemy.tscn");

    public void SpawnEnemy(string enemyType, Vector3 spawnPoint)
    {
        BaseEnemy newenemy;
        switch (enemyType)
        {
            case "Green":
                newenemy = GreenEnemy.Instantiate<GreenEnemy>();
                break;
            case "Pink":
                newenemy = PinkEnemy.Instantiate<PinkEnemy>();
                break;
            case "Grey":
                newenemy = GreyEnemy.Instantiate<GreyEnemy>();
                break;
            default:
                GD.Print("idk wtf happen switch went wrong??");
                return;
        }
        enemiesFolder.AddChild(newenemy);
        newenemy.GlobalPosition = spawnPoint;
    }
}
