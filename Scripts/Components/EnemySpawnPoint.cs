using Godot;
using System;

[GlobalClass]
public partial class EnemySpawnPoint : Marker3D
{
    private static readonly string[] EnemyTypes = new[] {"Green", "Pink", "Grey"};

    [Export(PropertyHint.Enum, "Green,Pink,Grey")]
    public string EnemyType { get; set; } = "Green";
}
