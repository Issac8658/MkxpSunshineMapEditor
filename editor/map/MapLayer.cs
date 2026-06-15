using Godot;
using RPG;

public partial class MapLayer : TileMapLayer
{
	[Export]
	public int Layer = 0;
	
	public override void _Ready()
	{
		RPG_Loader.MapLoaded += () =>
		{
			Clear();
			TileSet = RPG_Data.GodotTileSets[RPG_Data.CurrentMap.TilesetID];
			for (int y = 0; y < RPG_Data.CurrentMap.Data.TableData[0].Count; y++)
				for (int x = 0; x < RPG_Data.CurrentMap.Data.TableData[0][0].Count; x++)
					SetCell(new(x, y), TileSet.GetSourceId(0), TilesetHelper.GetCoordsFromID(RPG_Data.CurrentMap.Data.TableData[Layer][y][x]));
			GD.Print($"Map loaded");
		};
	}
}
