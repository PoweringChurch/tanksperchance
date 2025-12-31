using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

public partial class EditorManager : Control
{
    private GridContainer AssetSelectionGrid;
    private static readonly Dictionary<int, PackedScene> objectIds = new()
    {
        [0] = GD.Load<PackedScene>("res://Assets/Tiles/wall.tscn")
    };
    public override void _Ready()
    {
        AssetSelectionGrid = GetNode<GridContainer>("AssetSelectionGrid");

        LoadAssetSelection();
    }
    public void LoadAssetSelection()
    {
        foreach (KeyValuePair<int, PackedScene> keyValuePair in objectIds)
        {
            EditorSelectButton selectButton = new();
            selectButton.ObjectId = keyValuePair.Key;
            selectButton.LoadButton();
            AssetSelectionGrid.AddChild(selectButton);
        }
    }
}
