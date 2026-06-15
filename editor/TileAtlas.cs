using Godot;
using RPG;

public partial class TileAtlas : TextureRect
{
	public override void _Ready()
	{
		RPG_Loader.MapLoaded += () =>
		{
			Texture = RPG_Data.TilesetCache[RPG_Data.CurrentMap.TilesetID];
		};
	}
}
