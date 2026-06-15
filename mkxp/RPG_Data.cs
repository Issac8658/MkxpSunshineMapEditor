using Godot;
using Godot.Collections;

namespace RPG
{
	public static class RPG_Data
	{
		// ------------------- TileBits;
		public const byte TILE_BLOCK_DOWN   = 0b00000001;
		public const byte TILE_BLOCK_LEFT   = 0b00000010;
		public const byte TILE_BLOCK_RIGHT  = 0b00000100; 
		public const byte TILE_BLOCK_UP     = 0b00001000;
		public const byte TILE_RESERVED1    = 0b00010000;
		public const byte TILE_RESERVED2    = 0b00100000;
		public const byte TILE_BUSH_FLUG    = 0b01000000;
		public const byte TILE_COUNTER_FLAG = 0b10000000;

		//// ------------------- Events;
		//public static event EmptyEventHandler TilesetChanged;

		// ------------------- RPG Maker XP Values;
		public static string ProjectPath = null;
		public static Array<Tileset> Tilesets = [];
		public static Dictionary<int, MapInfo> MapInfos = [];
		public static Map CurrentMap = null;

		// ------------------- Editor Cache;
		public static Dictionary<int, Texture2D> TilesetCache = [];
		public static Dictionary<int, TileSet> GodotTileSets = [];

		public static bool TilesetExist(int tilesetID)
		{
			foreach(Tileset tileset in Tilesets)
				if (tileset != null && tileset.ID == tilesetID)
					return true;
			return false;
		}
		public static Tileset GetTileset(int tilesetID)
		{
			foreach(Tileset tileset in Tilesets)
				if (tileset.ID == tilesetID)
					return tileset;
			return null;
		}

		public static Tileset GetCurrentTileset()
		{
			if (CurrentMap != null)
				return Tilesets[CurrentMap.TilesetID];
			return null;
		}

		public static Texture2D GetCurrentTilesetTexture()
		{
			if (CurrentMap != null)
				return TilesetCache[CurrentMap.TilesetID];
			return TilesetCache[0];
		}

		public static void InitTileSets()
		{
			foreach (Tileset tileset in Tilesets)
			{
				TileSet godotTileset = new(){TileSize = TilesetHelper.TileSize};
				if (tileset != null)
				{
					if (!TilesetCache.ContainsKey(tileset.ID))
						continue;

					Texture2D atlas = TilesetCache[tileset.ID];
					TileSetAtlasSource tileSetAtlasSource = new() { Texture = atlas, TextureRegionSize = TilesetHelper.TileSize};
					Vector2I atlasImageSize = atlas.GetImage().GetSize();

					for (int y = 0; y < atlasImageSize.Y / TilesetHelper.TILE_HEIGHT; y++)
						for (int x = 0; x < atlasImageSize.X / TilesetHelper.TILE_WIDTH; x++)
						{
							Vector2I tileCoords = new (x, y);
							tileSetAtlasSource.CreateTile(tileCoords, Vector2I.One);
							tileSetAtlasSource.GetTileData(tileCoords, 0).ZIndex = tileset.Priorities.TableData[0][0][TilesetHelper.GetIdFromCoords(tileCoords)];
						}

					godotTileset.AddSource(tileSetAtlasSource);
				}

				GodotTileSets.Add(tileset.ID, godotTileset);
			}
		}
	}
}
