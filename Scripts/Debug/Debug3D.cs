using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class Debug3D : Node3D
{
    public static Debug3D Instance { get; private set; }
    
    private List<MeshInstance3D> debugObjects = new List<MeshInstance3D>();
    
    // Materials for different debug visuals
    private StandardMaterial3D pointMaterial;
    private StandardMaterial3D lineMaterial;
    private StandardMaterial3D vectorMaterial;
    
    // Meshes
    private SphereMesh sphereMesh;
    private BoxMesh lineMesh;
    private CylinderMesh arrowMesh;
    
    public override void _Ready()
    {
        Instance = this;
        SetupMaterials();
        SetupMeshes();
    }
    
    private void SetupMaterials()
    {
        // Point material (red)
        pointMaterial = new StandardMaterial3D();
        pointMaterial.AlbedoColor = Colors.Red;
        pointMaterial.AlbedoColor = new Color(1, 0, 0, 0.8f);
        
        // Line material (green)
        lineMaterial = new StandardMaterial3D();
        lineMaterial.AlbedoColor = Colors.Green;
        
        // Vector material (blue)
        vectorMaterial = new StandardMaterial3D();
        vectorMaterial.AlbedoColor = Colors.Blue;
    }
    
    private void SetupMeshes()
    {
        sphereMesh = new SphereMesh();
        sphereMesh.Radius = 0.1f;
        sphereMesh.Height = 0.2f;
        
        lineMesh = new BoxMesh();
        lineMesh.Size = new Vector3(0.05f, 0.05f, 1.0f);
        
        arrowMesh = new CylinderMesh();
        arrowMesh.TopRadius = 0.0f;
        arrowMesh.BottomRadius = 0.1f;
        arrowMesh.Height = 0.3f;
    }
    
    /// <summary>
    /// Visualize a point in 3D space
    /// </summary>
    /// <param name="point">Position to visualize</param>
    /// <param name="color">Optional color (default: red)</param>
    /// <param name="scale">Optional scale (default: 1.0)</param>
    /// <param name="duration">How long to show it in seconds (0 = permanent)</param>
    public static void VisualizePoint(Vector3 point, Color? color = null, float scale = 1.0f, float duration = 0.0f)
    {
        if (Instance == null) return;
        
        var meshInstance = new MeshInstance3D();
        meshInstance.Mesh = Instance.sphereMesh;
        meshInstance.Scale = Vector3.One * scale;
        
        var material = Instance.pointMaterial.Duplicate() as StandardMaterial3D;
        if (color.HasValue)
            material.AlbedoColor = color.Value;
        
        meshInstance.MaterialOverride = material;
        Instance.AddChild(meshInstance);
        meshInstance.GlobalPosition = point;
        Instance.debugObjects.Add(meshInstance);
        
        if (duration > 0)
        {
            Instance.GetTree().CreateTimer(duration).Timeout += () => Instance.RemoveDebugObject(meshInstance);
        }
    }
    
    /// <summary>
    /// Visualize a line between two points
    /// </summary>
    /// <param name="start">Start position</param>
    /// <param name="end">End position</param>
    /// <param name="color">Optional color (default: green)</param>
    /// <param name="thickness">Line thickness (default: 0.05)</param>
    /// <param name="duration">How long to show it in seconds (0 = permanent)</param>
    public static void VisualizeLine(Vector3 start, Vector3 end, Color? color = null, float thickness = 0.05f, float duration = 0.0f)
    {
        if (Instance == null) return;
        
        var meshInstance = new MeshInstance3D();
        var mesh = new BoxMesh();
        
        Vector3 direction = end - start;
        float distance = direction.Length();
        
        mesh.Size = new Vector3(thickness, thickness, distance);
        meshInstance.Mesh = mesh;
        
        // Position at midpoint
        meshInstance.GlobalPosition = start + direction * 0.5f;
        
        // Rotate to face the end point
        meshInstance.LookAt(end, Vector3.Up);
        
        var material = Instance.lineMaterial.Duplicate() as StandardMaterial3D;
        if (color.HasValue)
            material.AlbedoColor = color.Value;
        
        meshInstance.MaterialOverride = material;
        Instance.AddChild(meshInstance);
        Instance.debugObjects.Add(meshInstance);
        
        if (duration > 0)
        {
            Instance.GetTree().CreateTimer(duration).Timeout += () => Instance.RemoveDebugObject(meshInstance);
        }
    }
    
    /// <summary>
    /// Visualize a vector (direction + magnitude) from a starting point
    /// </summary>
    /// <param name="start">Starting position</param>
    /// <param name="vector">Vector to visualize</param>
    /// <param name="color">Optional color (default: blue)</param>
    /// <param name="arrowScale">Scale of the arrowhead (default: 1.0)</param>
    /// <param name="duration">How long to show it in seconds (0 = permanent)</param>
    public static void VisualizeVector(Vector3 start, Vector3 vector, Color? color = null, float arrowScale = 1.0f, float duration = 0.0f)
    {
        if (Instance == null) return;
        
        Vector3 end = start + vector;
        
        // Draw the line
        VisualizeLine(start, end, color, 0.03f, duration);
        
        // Draw the arrowhead
        var arrowInstance = new MeshInstance3D();
        arrowInstance.Mesh = Instance.arrowMesh;
        arrowInstance.GlobalPosition = end;
        arrowInstance.Scale = Vector3.One * arrowScale;
        
        // Point the arrow in the direction of the vector
        if (vector.Length() > 0.001f)
        {
            arrowInstance.LookAt(end + vector.Normalized(), Vector3.Up);
        }
        
        var material = Instance.vectorMaterial.Duplicate() as StandardMaterial3D;
        if (color.HasValue)
            material.AlbedoColor = color.Value;
        
        arrowInstance.MaterialOverride = material;
        Instance.AddChild(arrowInstance);
        Instance.debugObjects.Add(arrowInstance);
        
        if (duration > 0)
        {
            Instance.GetTree().CreateTimer(duration).Timeout += () => Instance.RemoveDebugObject(arrowInstance);
        }
    }
    
    /// <summary>
    /// Visualize a ray (like raycast visualization)
    /// </summary>
    /// <param name="origin">Ray origin</param>
    /// <param name="direction">Ray direction (normalized)</param>
    /// <param name="length">Ray length</param>
    /// <param name="color">Optional color</param>
    /// <param name="duration">Duration to show</param>
    public static void VisualizeRay(Vector3 origin, Vector3 direction, float length, Color? color = null, float duration = 0.0f)
    {
        Vector3 end = origin + direction.Normalized() * length;
        VisualizeVector(origin, direction.Normalized() * length, color ?? Colors.Yellow, 0.5f, duration);
    }
    
    /// <summary>
    /// Visualize a sphere (good for radius visualization)
    /// </summary>
    /// <param name="center">Center position</param>
    /// <param name="radius">Sphere radius</param>
    /// <param name="color">Optional color</param>
    /// <param name="wireframe">Show as wireframe (default: true)</param>
    /// <param name="duration">Duration to show</param>
    public static void VisualizeSphere(Vector3 center, float radius, Color? color = null, bool transparent = true, float duration = 0.0f)
    {
        if (Instance == null) return;
        
        var meshInstance = new MeshInstance3D();
        var sphere = new SphereMesh();
        sphere.Radius = radius;
        sphere.Height = radius * 2;
        
        meshInstance.Mesh = sphere;
        
        var material = new StandardMaterial3D();
        material.AlbedoColor = color ?? new Color(1, 1, 1, 0.05f);

        if (transparent)
        {
            material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
            material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
        }
        
        meshInstance.MaterialOverride = material;
        Instance.AddChild(meshInstance);
        Instance.debugObjects.Add(meshInstance);
        meshInstance.GlobalPosition = center;
        
        if (duration > 0)
        {
            Instance.GetTree().CreateTimer(duration).Timeout += () => Instance.RemoveDebugObject(meshInstance);
        }
    }
    
    /// <summary>
    /// Clear all debug objects
    /// </summary>
    public static void ClearAll()
    {
        if (Instance == null) return;
        
        foreach (var obj in Instance.debugObjects)
        {
            if (IsInstanceValid(obj))
                obj.QueueFree();
        }
        Instance.debugObjects.Clear();
    }
    
    /// <summary>
    /// Print debug info to console
    /// </summary>
    /// <param name="message">Message to print</param>
    /// <param name="position">Optional world position context</param>
    public static void Log(string message, Vector3? position = null)
    {
        string output = $"[DEBUG] {message}";
        if (position.HasValue)
            output += $" at {position.Value}";
        
        GD.Print(output);
    }
    
    private void RemoveDebugObject(MeshInstance3D obj)
    {
        if (IsInstanceValid(obj))
        {
            debugObjects.Remove(obj);
            obj.QueueFree();
        }
    }
}