using Godot;
using System.IO;

public partial class Debug : Node
{
    private LevelLoader levelLoader;
    private VBoxContainer levelButtonContainer;
    public override void _Ready()
    {
        levelButtonContainer = GetNode<VBoxContainer>("/root/main3d/UI/LevelSelect/VBoxContainer");
        levelLoader = GetNode<LevelLoader>("/root/main3d/LevelLoader");
        string stageDirectoryPath = "res://Assets/Stages/";
        using var dir = DirAccess.Open(stageDirectoryPath);
        dir.ListDirBegin();
        string fileName = dir.GetNext();
        while (fileName != "")
        {
            if (!dir.CurrentIsDir() && fileName.EndsWith(".json"))
            {
                string stageName = Path.GetFileNameWithoutExtension(fileName);
                string fullPath = stageDirectoryPath + fileName;
                Button levelButton = new Button();
                levelButton.Text = stageName;
                levelButton.Name = stageName;
                levelButton.Pressed += () => levelLoader.LoadLevel(fullPath);
                levelButtonContainer.AddChild(levelButton);
            }
            fileName = dir.GetNext();
        }
    }
    public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		if (@event is InputEventKey keyEvent)
			if (keyEvent.Keycode == Key.K && keyEvent.Pressed)
            {
                var navRegion = GetTree().Root.FindChild("NavRegion", true, false) as NavigationRegion3D;
                GD.Print("rebaked?");
                navRegion.BakeNavigationMesh();
            }
    }
}
