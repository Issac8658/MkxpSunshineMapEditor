using Godot;
using RPG;

public partial class TilesetTest : TextureRect
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		RPG_Loader.ProjectLoaded += () =>
		{
			Texture = ImageTexture.CreateFromImage(TilesetHelper.GenerateTilesetImage(4));
		};
	}
}
