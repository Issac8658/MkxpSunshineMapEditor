using System.IO;
using Godot;
using RPG;

public partial class Panorama : Sprite2D
{
	[Export]
	public Camera2D Camera;
	[Export]
	public bool InvertOffset = false;
	[Export]
	public double Zoom = 2;

	public override void _Ready()
	{
		RPG_Loader.MapLoaded += () =>
		{
			if (RPG_Data.TilesetExist(RPG_Data.CurrentMap.TilesetID))
			{
				string panoramaName = RPG_Data.GetTileset(RPG_Data.CurrentMap.TilesetID).PanoramaName;
				string panoramaPath = Path.Combine(RPG_Loader.PANORAMAS_FOLDER, panoramaName + ".png");
				Texture2D texture = RPG_Loader.GetSprite(panoramaPath);
				if (texture != null)
				{
					Texture = texture;
					return;
				}
			}
			Image emptyPanorama = Image.CreateEmpty(32, 32, false, Image.Format.R8);
			emptyPanorama.Fill(Colors.Black);
			Texture = ImageTexture.CreateFromImage(emptyPanorama);
		};
	}

	public override void _Process(double delta)
	{
		(Material as ShaderMaterial).SetShaderParameter("zoom", Camera.Zoom.X);
		(Material as ShaderMaterial).SetShaderParameter("bgZoom", Zoom);
		(Material as ShaderMaterial).SetShaderParameter("offset", (InvertOffset ? Camera.Position + Camera.Offset : Camera.Offset) + Position);
	}
}
