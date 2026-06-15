using System.IO;
using Godot;
using Godot.Collections;

namespace RPG
{
	public static class RPG_Loader
	{
		public const string DATA_FOLDER_NAME = "Data";
		public const string GRAPHICS_FOLDER_NAME = "Graphics";

		public const string TILESETS_FOLDER = $"{GRAPHICS_FOLDER_NAME}/Tilesets";
		public const string AUTOTILES_FOLDER = $"{GRAPHICS_FOLDER_NAME}/Autotiles";
		public const string PANORAMAS_FOLDER = $"{GRAPHICS_FOLDER_NAME}/Panoramas";
		public const string CHARACTERS_FOLDER = $"{GRAPHICS_FOLDER_NAME}/Characters";

		public const string MAP_INFOS_NAME = "MapInfos.rxdata";
		public const string MAP_NAME = "Map{0}.rxdata";
		public const string TILESETS_NAME = "Tilesets.rxdata";

		public static event EmptyEventHandler ProjectLoaded;
		public static event EmptyEventHandler MapLoaded;

		private static DirAccess ProjectFolder;
		private static DirAccess DataFolder;
		private static DirAccess GraphicsFolder;

		private static Dictionary<string, Texture2D> SpriteCache = [];

		public static bool LoadProject(string path, out string errorMessage)
		{
			Reset();

			path = ProjectSettings.GlobalizePath(path);

			ProjectFolder = DirAccess.Open(path);
			if (ProjectFolder != null)
			{
				DataFolder = DirAccess.Open(Path.Combine(path, DATA_FOLDER_NAME));
				if (DataFolder == null)
					errorMessage = $"Unable to open /{DATA_FOLDER_NAME} folder\n{DirAccess.GetOpenError()}";
				else
				{
					GraphicsFolder = DirAccess.Open(Path.Combine(path, GRAPHICS_FOLDER_NAME));
					if (GraphicsFolder == null)
						errorMessage = $"Unable to open /{GRAPHICS_FOLDER_NAME} folder\n{DirAccess.GetOpenError()}";
					else
					{
						RPG_Data.ProjectPath = ProjectFolder.GetCurrentDir();

						RXData_Parser TilesetsParser = new(Path.Combine(DataFolder.GetCurrentDir(), TILESETS_NAME));

						if (!TilesetsParser.Success)
							errorMessage = TilesetsParser.ErrorMessage;
						else
						{
							Array RawTilesets = TilesetsParser.Data.AsGodotArray();

							foreach(Variant RawTileSet in RawTilesets)
							{
								Tileset tileset = RubySerializator.DeserializeData<Tileset>(RawTileSet);
								RPG_Data.Tilesets.Add(tileset);
								if (tileset != null)
								{
									Image image = TilesetHelper.GenerateTilesetImage(tileset.ID);
									if (image != null)
										RPG_Data.TilesetCache.Add(tileset.ID, ImageTexture.CreateFromImage(image));}
								else
									RPG_Data.TilesetCache.Add(tileset.ID, null);
							}

							RXData_Parser MapInfosParser = new(Path.Combine(DataFolder.GetCurrentDir(), MAP_INFOS_NAME));
							if (!MapInfosParser.Success)
								errorMessage = MapInfosParser.ErrorMessage;
							else
							{
								Dictionary<int, Variant> MapInfosData = MapInfosParser.Data.AsGodotDictionary<int, Variant>();
								foreach (var (mapID, mapInfo) in MapInfosData)
									RPG_Data.MapInfos.Add(mapID, RubySerializator.DeserializeData<MapInfo>(mapInfo));

								RPG_Data.InitTileSets();

								ProjectLoaded?.Invoke();
								errorMessage = "Ok";
								return true;
							}
						}
					}
				}
			}
			else
				errorMessage = $"Unable to open project folder {path}\n{DirAccess.GetOpenError()}";
			GD.PushWarning(errorMessage);
			return false;
		}
/*
		public static bool CanLoad(string path, out string errorMessage)
		{
			DataFolder = DirAccess.Open(Path.Combine(path, DATA_FOLDER_NAME));
			if (DataFolder == null)
			{
				errorMessage = $"Unable to open /{DATA_FOLDER_NAME} folder\n{DirAccess.GetOpenError()}";
				GD.PushWarning(errorMessage);
				return false;
			}
			GraphicsFolder = DirAccess.Open(Path.Combine(path, GRAPHICS_FOLDER_NAME));
			if (GraphicsFolder == null)
			{
				errorMessage = $"Unable to open /{GRAPHICS_FOLDER_NAME} folder\n{DirAccess.GetOpenError()}";
				GD.PushWarning(errorMessage);
				return false;
			}
			if (!DataFolder.FileExists(MAP_INFOS_NAME))
			{
				errorMessage = $"/{DATA_FOLDER_NAME}/{MAP_INFOS_NAME} isn't exist";
				GD.PushWarning(errorMessage);
				return false;
			}
		}
*/
		public static void LoadMap(int mapID)
		{
			string fileName = string.Format(MAP_NAME, mapID.ToString("D3"));
			if (DataFolder.FileExists(fileName))
				RPG_Data.CurrentMap = RubySerializator.DeserializeData<Map>(new RXData_Parser(Path.Combine(DataFolder.GetCurrentDir(), fileName)).Data);

			MapLoaded?.Invoke();
		}

		public static Texture2D GetSprite(string SpritePath)
		{
			if (SpriteCache.TryGetValue(SpritePath, out Texture2D value))
				return value;
			
			string resultPath = Path.Combine(RPG_Data.ProjectPath, SpritePath);
			if (!Godot.FileAccess.FileExists(resultPath))
				return null;
			
			Texture2D result = ImageTexture.CreateFromImage(Image.LoadFromFile(resultPath));
			SpriteCache[SpritePath] = result;
			return result;
		}

		public static void Reset()
		{
			RPG_Data.ProjectPath = null;
			RPG_Data.Tilesets = [];
			RPG_Data.MapInfos = [];
			RPG_Data.CurrentMap = null;
		}
	}
}
