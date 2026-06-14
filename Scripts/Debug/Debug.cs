using Godot;
using System.IO;

public partial class Debug : Node
{
    private LevelLoader levelLoader;
    private VBoxContainer levelButtonContainer;
    private Button enableDebugMode;
    private Button disableDebugMode;
    public override void _Ready()
    {
        var ui = GetNode<Control>("/root/main3d/UI");

        var debugList = ui.GetNode<VBoxContainer>("DebugPanel/VBoxContainer");
        
        // debug mode
        enableDebugMode = debugList.GetNode<Button>("DebugMode/Enable");
        disableDebugMode = debugList.GetNode<Button>("DebugMode/Disable");
        var playerNode = GetNode<Player>("/root/main3d/Player");

        enableDebugMode.Pressed += () => playerNode.debugMode = true;
        disableDebugMode.Pressed += () => playerNode.debugMode = false;

        // level select list
        levelButtonContainer = debugList.GetNode<VBoxContainer>("LevelSelect/VBoxContainer");
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
}
