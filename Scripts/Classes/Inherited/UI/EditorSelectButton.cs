using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;

[GlobalClass]
public partial class EditorSelectButton : Button
{
    private static Vector2 minSize = new(64, 64);
    private static readonly Dictionary<int, string> objectText = new()
    {
        [0] = "Wall",
        [1] = "Green Enemy"
    };
    [Export]
    public int ObjectId { get; set; } = 0;
    public void LoadButton()
    {
        Text = objectText[ObjectId];
        CustomMinimumSize = minSize;
    }
}
