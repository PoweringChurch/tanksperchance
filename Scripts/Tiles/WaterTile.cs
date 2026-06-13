public partial class WaterTile : BaseTile
{
    public override void _Ready()
    {
        characterCollide = true;
        projectileCollide = false;
        destroyThreshold = 99;
        durability = 99;
        base._Ready();
    }
}