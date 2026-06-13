public partial class BushTile : BaseTile
{
    public override void _Ready()
    {
        characterCollide = false;
        projectileCollide = true;
        destroyThreshold = 99;
        durability = 99;
        base._Ready();
    }
}