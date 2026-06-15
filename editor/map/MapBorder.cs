using Godot;
using RPG;

public partial class MapBorder : Node2D
{
	public override void _Ready()
	{
		RPG_Loader.MapLoaded += () =>
		{
			Position = new Vector2(RPG_Data.CurrentMap.Data.TableData[0][0].Count, RPG_Data.CurrentMap.Data.TableData[0].Count) * new Vector2(TilesetHelper.TILE_WIDTH, TilesetHelper.TILE_HEIGHT);
		};
	}
}
