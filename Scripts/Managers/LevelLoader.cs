using Godot;
using System.Collections.Generic;
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

// ── Loader node ───────────────────────────────────────────────────────────────

/// <summary>
/// Attach this node to your level scene. Call LoadLevel() with a path like
/// "res://levels/level_01.json" and it will instantiate every object
/// defined in the file.
///
/// Asset scene paths are resolved through the AssetRegistry dictionary below.
/// Add or modify entries to match your project's scene paths.
/// </summary>
public partial class LevelLoader : Node
{
    // ── Asset registry ────────────────────────────────────────────────────────
    // Maps each asset_id string to a PackedScene resource path.
    // Adjust these paths to match your project layout.

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
        floorInstance.Position = new(_levelData.GridWidth / 2f, 0, _levelData.GridHeight / 2f);
        floorInstance.AddToGroup("toBake");
        _objectRoot.AddChild(floorInstance);
        // load tiles only
        var postBakeObjects = new List<LevelObject>();
        foreach (var obj in _levelData.Objects)
            if (obj.AssetId.EndsWith("_enemy"))
                postBakeObjects.Add(obj);
            else
                SpawnObject(obj);
        // set up nav region and bake
        var navRegion = new NavigationRegion3D { Name = "NavRegion" };
        navRegion.NavigationMesh = new NavigationMesh();
        _objectRoot.AddChild(navRegion);
        navRegion.NavigationMesh.GeometryParsedGeometryType = NavigationMesh.ParsedGeometryType.StaticColliders;
        navRegion.NavigationMesh.GeometrySourceGeometryMode = NavigationMesh.SourceGeometryMode.GroupsExplicit;
        navRegion.NavigationMesh.GeometrySourceGroupName = "toBake";
        navRegion.NavigationMesh.AgentRadius = 0.5f;
        navRegion.NavigationMesh.SetCollisionMaskValue(4,false);
        navRegion.NavigationMesh.SetCollisionMaskValue(2,false);
        navRegion.BakeFinished += () => OnNavBakeFinished(postBakeObjects);
        navRegion.BakeNavigationMesh(false);
    }
    private void OnNavBakeFinished(List<LevelObject> remaining)
    {
        // load player
        var playerSpawn = _levelData.Objects.Find(o => o.AssetId == "player_spawn");
        var characterScene = GD.Load<PackedScene>(AssetRegistry["player_spawn"]);
        var characterInstance = characterScene.Instantiate<Node3D>();
        characterInstance.Position = GridToWorld(playerSpawn.X, playerSpawn.Y);
        _objectRoot.AddChild(characterInstance);
        var playerNode = GetNode<Player>("/root/main3d/Player");
        playerNode.StartPlayer(_objectRoot);
        // load objects & enemies
        foreach (var obj in remaining)
            SpawnObject(obj);
    }
    /// <summary>remove all spawned level objects from the scene</summary>
    public void UnloadLevel()
    {
        var playerNode = GetNode<Player>("/root/main3d/Player");
        playerNode.playerAlive = false;
        _objectRoot?.QueueFree();
        _objectRoot = null;
        _levelData  = null;
    }

    /// <summary>return the current level data (null if no level is loaded)</summary>
    public LevelData GetLevelData() => _levelData;

    /// <summary>convert grid coordinates to world position (top-left origin)</summary>
    public Vector3 GridToWorld(int x, int y)
    {
        return new Vector3(x, 1, y);
    }

    // Internals
    private void SpawnObject(LevelObject obj)
    {
        if (!AssetRegistry.TryGetValue(obj.AssetId, out string scenePath))
        {
            GD.PrintErr($"[LevelLoader] Unknown asset_id '{obj.AssetId}' at ({obj.X},{obj.Y})");
            return;
        }
        if (obj.AssetId == "player_spawn")
            return; // pass, as this is handled in boilerplate
        var scene = GD.Load<PackedScene>(scenePath);
        if (scene == null)
        {
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

    private static string ReadJson(string path)
    {
        if (!FileAccess.FileExists(path))
        {
            GD.PrintErr($"[LevelLoader] File not found: {path}");
            return null;
        }
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PrintErr($"[LevelLoader] Cannot open: {path}  error={FileAccess.GetOpenError()}");
            return null;
        }
        return file.GetAsText();
    }
}