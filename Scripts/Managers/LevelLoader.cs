using Godot;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

// data model
public class LevelData
{
    [JsonPropertyName("level_name")]   public string LevelName  { get; set; } = "unnamed";
    [JsonPropertyName("grid_width")]   public int GridWidth     { get; set; } = 20;
    [JsonPropertyName("grid_height")]  public int GridHeight    { get; set; } = 16;
    [JsonPropertyName("objects")]      public List<LevelObject> Objects { get; set; } = new();
}
public class ObjectParams
{
    [JsonPropertyName("data")] public int Data { get; set; } = 0;
}
public class LevelObject
{
    [JsonPropertyName("asset_id")] public string AssetId { get; set; } = "";
    [JsonPropertyName("x")]        public int X          { get; set; }
    [JsonPropertyName("y")]        public int Y          { get; set; }
    [JsonPropertyName("params")]   public ObjectParams Params  { get; set; } = null;
}
public partial class LevelLoader : Node
{
    // ── Asset registry ────────────────────────────────────────────────────────

    private static readonly Dictionary<string, string> AssetRegistry = new()
    {
        // Terrain
        { "breakable_wall", "res://Assets/Scenes/Tiles/breakable_wall.tscn"          },
        { "wall",           "res://Assets/Scenes/Tiles/wall.tscn"     },
        { "water",          "res://Assets/Scenes/Tiles/water.tscn"         },
        { "bush",           "res://Assets/Scenes/Tiles/bush.tscn"},
        // Enemies
        { "pink_enemy",     "res://Assets/Scenes/Enemies/pink_enemy.tscn"  },
        { "green_enemy",    "res://Assets/Scenes/Enemies/green_enemy.tscn"  },
        { "grey_enemy",     "res://Assets/Scenes/Enemies/grey_enemy.tscn"  },
        { "brown_enemy",     "res://Assets/Scenes/Enemies/brown_enemy.tscn"  },
        // Special
        { "player_spawn",   "res://Assets/Scenes/Player/Character/character.tscn"  },
        { "objective",      "res://Assets/Scenes/Tiles/objective.tscn"  }
    };
    // State
    private LevelData   _levelData;
    public Node        _objectRoot;
    private List<LevelObject> _pendingPostBakeObjects;
    public NavigationRegion3D _currentNavRegion;
    // Public API
    /// <summary>Load and instantiate a level from a JSON file path.</summary>
    /// <param name="jsonPath">Resource path, e.g. "res://levels/level_01.json"</param>
    public void LoadLevel(string jsonPath)
    {
        UnloadLevel();
        string json = ReadJson(jsonPath);
        if (json == null) return;
        _levelData = JsonSerializer.Deserialize<LevelData>(json);
        if (_levelData == null)
        {
            GD.PrintErr($"[LevelLoader] Failed to deserialize {jsonPath}");
            return;
        }
        GD.Print($"[LevelLoader] Loading '{_levelData.LevelName}' " +
                $"({_levelData.GridWidth}x{_levelData.GridHeight}, " +
                $"{_levelData.Objects.Count} objects)");

        _objectRoot = new Node3D { Name = "LevelObjects" };
        AddChild(_objectRoot);

        // load floor
        var floorScene = GD.Load<PackedScene>("res://Assets/Scenes/Tiles/floor.tscn");
        var floorInstance = floorScene.Instantiate<Node3D>();

        floorInstance.Scale = new(_levelData.GridWidth, 1, _levelData.GridHeight);
        floorInstance.Position = new((_levelData.GridWidth / 2f)-0.5f, 0, (_levelData.GridHeight / 2f)-0.5f);

        floorInstance.AddToGroup("navigation");
        _objectRoot.AddChild(floorInstance);
        // load walls
        for (int x = -1; x < _levelData.GridWidth+1; x++)
        {
            SpawnObject("wall", x, -1);
            SpawnObject("wall", x, _levelData.GridHeight);
        }
        for (int y = 0; y < _levelData.GridHeight;y++)
        {
            SpawnObject("wall", -1, y);
            SpawnObject("wall", _levelData.GridWidth, y);
        }
        // load player
        var playerSpawn = _levelData.Objects.Find(o => o.AssetId == "player_spawn");

        var characterScene = GD.Load<PackedScene>(AssetRegistry["player_spawn"]);
        var characterInstance = characterScene.Instantiate<Node3D>();

        characterInstance.Position = GridToWorld(playerSpawn.X, playerSpawn.Y);
        _objectRoot.AddChild(characterInstance);
        var playerNode = GetNode<Player>("/root/main3d/Player");
        playerNode.StartPlayer(_objectRoot);
        playerNode.camMinX = floorInstance.Position.X - _levelData.GridWidth/2f + 10;
        playerNode.camMaxX = floorInstance.Position.X + _levelData.GridWidth/2f - 10;
        playerNode.camMaxX = (playerNode.camMinX > playerNode.camMaxX) ? playerNode.camMinX : playerNode.camMaxX;
        playerNode.camMinZ = floorInstance.Position.Z - _levelData.GridHeight/2f + 15; 
        playerNode.camMaxZ = floorInstance.Position.Z + _levelData.GridHeight/2f;
        playerNode.camMaxZ = (playerNode.camMinZ > playerNode.camMaxZ) ? playerNode.camMinZ : playerNode.camMaxZ;

        // load tiles
        _pendingPostBakeObjects = new List<LevelObject>();
        foreach (var obj in _levelData.Objects)
            if (obj.AssetId.EndsWith("_enemy"))
                _pendingPostBakeObjects.Add(obj);
            else
                SpawnObject(obj);
        // set up nav region and bake
        _currentNavRegion = new() { Name = "NavRegion", NavigationMesh = new NavigationMesh() };
        _objectRoot.AddChild(_currentNavRegion);

        _currentNavRegion.NavigationMesh.GeometryParsedGeometryType = NavigationMesh.ParsedGeometryType.StaticColliders;
        _currentNavRegion.NavigationMesh.GeometrySourceGeometryMode = NavigationMesh.SourceGeometryMode.GroupsExplicit;
        _currentNavRegion.NavigationMesh.GeometrySourceGroupName = "navigation";

        _currentNavRegion.NavigationMesh.AgentRadius = 0.5f;
        _currentNavRegion.NavigationMesh.SetCollisionMaskValue(4,false);
        _currentNavRegion.NavigationMesh.SetCollisionMaskValue(2,false);
        _currentNavRegion.BakeFinished += OnNavBakeFinished;
        _currentNavRegion.BakeNavigationMesh(false);
    }
    private void OnNavBakeFinished()
    {
        if (_pendingPostBakeObjects != null)
        {   // load enemies
            foreach (var obj in _pendingPostBakeObjects)
                SpawnObject(obj);
            _pendingPostBakeObjects = null; // clear
        }
        _currentNavRegion.BakeFinished -= OnNavBakeFinished;
    }
    /// <summary>remove all spawned level objects from the scene</summary>
    public void UnloadLevel()
    {
        var playerNode = GetNode<Player>("/root/main3d/Player");
        playerNode.playerAlive = false;
        var children = _objectRoot?.GetChildren();
        if (children != null)
            foreach (var child in children)
                child.RemoveFromGroup("navigation");
        _objectRoot?.QueueFree();
        _objectRoot = null;
        _levelData  = null;
    }

    /// <summary>return the current level data (null if no level is loaded)</summary>
    public LevelData GetLevelData() => 
        _levelData;

    /// <summary>convert grid coordinates to world position (top-left origin)</summary>
    public Vector3 GridToWorld(int x, int y) => 
        new Vector3(x, 1, y);

    // Internals
    private void SpawnObject(LevelObject obj)
    {
        if (!AssetRegistry.TryGetValue(obj.AssetId, out string scenePath)) {
            GD.PrintErr($"[LevelLoader] Unknown asset_id '{obj.AssetId}' at ({obj.X},{obj.Y})");
            return;
        }
        if (obj.AssetId == "player_spawn")
            return; // dont handle player spawn in spawnobj
            
        var scene = GD.Load<PackedScene>(scenePath);
        if (scene == null) {
            GD.PrintErr($"[LevelLoader] Scene not found: {scenePath}");
            return;
        }

        var instance = scene.Instantiate<Node3D>();

        instance.Position = GridToWorld(obj.X, obj.Y);
        instance.Name     = $"{obj.AssetId}_{obj.X}_{obj.Y}";

        if (obj.AssetId == "objective" && obj.Params != null)
        { /* pass */ }
        _objectRoot.AddChild(instance);
    }
    private void SpawnObject(string assetId, int x, int y, ObjectParams objectParams = null) =>
        SpawnObject(new LevelObject()
        {
            AssetId = assetId,
            X = x,
            Y = y,
            Params = objectParams
        });
    private static string ReadJson(string path)
    {
        if (!FileAccess.FileExists(path)) {
            GD.PrintErr($"[LevelLoader] File not found: {path}");
            return null;
        }
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null) {
            GD.PrintErr($"[LevelLoader] Cannot open: {path}  error={FileAccess.GetOpenError()}");
            return null;
        }
        return file.GetAsText();
    }
}