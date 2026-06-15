using System.IO;
using Godot;
using Godot.Collections;

namespace RPG
{

    public static class TilesetHelper
    {
        public const int TILE_WIDTH = 32;
        public const int TILE_HEIGHT = 32;
        public const int TILESET_WIDTH_TILECOUNT = 8;
        public const int AUTOTILE_ZONE_HEIGHT_TILECOUNT = 6;
        public const int AUTOTILES_COUNT = 7;
        public const int AUTOTILES_HEIGHT = AUTOTILE_ZONE_HEIGHT_TILECOUNT * TILE_HEIGHT * (AUTOTILES_COUNT + 1);

		public static Vector2I TileSize { get => new(TILE_WIDTH, TILE_HEIGHT); }

        public static Image GenerateTilesetImage(int tilesetID)
        {
            Tileset tileset = RPG_Data.GetTileset(tilesetID);
            if (tileset != null)
            {
                Image result;
                string path = Path.Combine(RPG_Data.ProjectPath, RPG_Loader.TILESETS_FOLDER, tileset.TilesetName + ".png");
                if (!Godot.FileAccess.FileExists(path))
                    return null;
                Image tilesetImage = Image.LoadFromFile(path);
                
                result = Image.CreateEmpty(TILESET_WIDTH_TILECOUNT * TILE_WIDTH, AUTOTILES_HEIGHT + tilesetImage.GetHeight() / TILE_HEIGHT * TILE_HEIGHT, false, Image.Format.Rgba8);
                CloneFrom(tilesetImage, result, new(0, 0, tilesetImage.GetSize()), new(0, AUTOTILES_HEIGHT));

                for (int autotileID = 0; autotileID < AUTOTILES_COUNT; autotileID++)
                {
                    string autotilePath = Path.Combine(RPG_Data.ProjectPath, RPG_Loader.AUTOTILES_FOLDER, tileset.AutotileNames[autotileID] + ".png");
                    if (Godot.FileAccess.FileExists(autotilePath))
                    {
                        Image autotileImage = Image.LoadFromFile(autotilePath);
                        for (int autotileLayer = 0; autotileLayer < AUTOTILE_ZONE_HEIGHT_TILECOUNT; autotileLayer++)
                            for (int autotileColumn = 0; autotileColumn < TILESET_WIDTH_TILECOUNT; autotileColumn++)
                            {
                                int autotileReletiveID = GetIdFromCoords(autotileColumn, autotileLayer);
                                Rect2I[] smolTiles = GetSmolTiles(autotileReletiveID);

                                for(int smolTileID = 0; smolTileID < 4; smolTileID++)
                                {
                                    //GD.Print(autotileID, " ", autotileReletiveID, " ", smolTileID);
                                    CloneFrom(autotileImage, result, smolTiles[smolTileID], new Vector2I(autotileColumn, (autotileID + 1) * AUTOTILE_ZONE_HEIGHT_TILECOUNT + autotileLayer) * TileSize + SmolTileOffsets[smolTileID]);
                                }
                            }
                    }
                }
                return result;
            }
            return null;
        }

        public static void CloneFrom(Image from, Image to, Rect2I fromRect, Vector2I toPos)
        {
            for (int X = 0; X < fromRect.Size.X; X++)
                for (int Y = 0; Y < fromRect.Size.Y; Y++)
                {
                    Vector2I sourcePos = fromRect.Position + new Vector2I(X, Y);
                    Vector2I targetPos = toPos + new Vector2I(X, Y);
                    
                    Rect2I sourceRect = new(Vector2I.Zero, from.GetSize());
                    Rect2I targetRect = new(Vector2I.Zero, to.GetSize());

                    if (sourceRect.HasPoint(sourcePos) && targetRect.HasPoint(targetPos))
                        to.SetPixel(targetPos.X, targetPos.Y, from.GetPixelv(sourcePos));
                }
        }

        public static Rect2I[] GetSmolTiles(int autotileID) => [
            autotileRects[autotileID * 4],
            autotileRects[autotileID * 4 + 1],
            autotileRects[autotileID * 4 + 2],
            autotileRects[autotileID * 4 + 3]
        ];

        public static readonly Vector2I[] SmolTileOffsets = [
            new(0,0),
            new(TILE_WIDTH / 2, 0),
            new(0, TILE_HEIGHT / 2),
            new(TILE_WIDTH / 2, TILE_HEIGHT / 2)
        ];

        public static Vector2I GetCoordsFromID(int ID) =>
            new(ID % TILESET_WIDTH_TILECOUNT, ID / TILESET_WIDTH_TILECOUNT);
        
        public static int GetIdFromCoords(Vector2I coords) => coords.Y * TILESET_WIDTH_TILECOUNT + coords.X;
        public static int GetIdFromCoords(int x, int y) => y * TILESET_WIDTH_TILECOUNT + x;

        // yes, i taked this from OneShot: Sunshine
        public static readonly Rect2I[] autotileRects = [
    	    // where "-" - "Water", "#" - filled "water" connected subtile, "C" - cetral subtile, "U" - unused
    	    // autotile example (Look Graphics/Autotiles/minitiles.png), in this example mini tile is 2x2
    	    /*
    	    UUUU UUUU -##-
    	    UCCU UUUU #CC#
    	    UCCU UUUU #CC#
    	    UUUU UUUU -##-

    	    ---- ---- ----
    	    -C#C #CC# C#C-
    	    -### #### ###-
    	    -C#C #CC# C#C-

    	    -### #### ###-
    	    -C#C #CC# C#C-
    	    -C#C #CC# C#C-
    	    -### #### ###-

    	    -C#C #CC# C#C-
    	    -### #### ###-
    	    -C#C #CC# C#C-
    	    ---- ---- ----
    	    */
        // 0
        	new( 32,  64, 16, 16 ), // ####
        	new( 48,  64, 16, 16 ), // #CC#
        	new( 32,  80, 16, 16 ), // #CC#
        	new( 48,  80, 16, 16 ), // ####
        // 1
        	new( 64,   0, 16, 16 ), // -###
        	new( 48,  64, 16, 16 ), // #CC#
        	new( 32,  80, 16, 16 ), // #CC#
        	new( 48,  80, 16, 16 ), // ####
        // 2
        	new( 32,  64, 16, 16 ), // ###-
        	new( 80,   0, 16, 16 ), // #CC#
        	new( 32,  80, 16, 16 ), // #CC#
        	new( 48,  80, 16, 16 ), // ####
        // 3
        	new( 64,   0, 16, 16 ), // -##-
        	new( 80,   0, 16, 16 ), // #CC#
        	new( 32,  80, 16, 16 ), // #CC#
        	new( 48,  80, 16, 16 ), // ####
        // 4
        	new( 32,  64, 16, 16 ), // ####
        	new( 48,  64, 16, 16 ), // #CC#
        	new( 32,  80, 16, 16 ), // #CC#
        	new( 80,  16, 16, 16 ), // ###-
        // 5
        	new( 64,   0, 16, 16 ), // -###
        	new( 48,  64, 16, 16 ), // #CC#
        	new( 32,  80, 16, 16 ), // #CC#
        	new( 80,  16, 16, 16 ), // ###-
        // 6
        	new( 32,  64, 16, 16 ), // ###-
        	new( 80,   0, 16, 16 ), // #CC#
        	new( 32,  80, 16, 16 ), // #CC#
        	new( 80,  16, 16, 16 ), // ###-
        // 7
        	new( 64,   0, 16, 16 ), // -##-
        	new( 80,   0, 16, 16 ), // #CC#
        	new( 32,  80, 16, 16 ), // #CC#
        	new( 80,  16, 16, 16 ), // ###-
        // 8
        	new( 32,  64, 16, 16 ), // ####
        	new( 48,  64, 16, 16 ), // #CC#
        	new( 64,  16, 16, 16 ), // #CC#
        	new( 48,  80, 16, 16 ), // -###
        // 9
        	new( 64,   0, 16, 16 ), // -###
        	new( 48,  64, 16, 16 ), // #CC#
        	new( 64,  16, 16, 16 ), // #CС#
        	new( 48,  80, 16, 16 ), // -###
        // 10
        	new( 32,  64, 16, 16 ), // ###-
        	new( 80,   0, 16, 16 ), // #CC#
        	new( 64,  16, 16, 16 ), // #CC#
        	new( 48,  80, 16, 16 ), // -###
        // 11
        	new( 64,   0, 16, 16 ), // -##-
        	new( 80,   0, 16, 16 ), // #CC#
        	new( 64,  16, 16, 16 ), // #CC#
        	new( 48,  80, 16, 16 ), // -###
        // 12
        	new( 32,  64, 16, 16 ), // ####
        	new( 48,  64, 16, 16 ), // #CC#
        	new( 64,  16, 16, 16 ), // #CC#
        	new( 80,  16, 16, 16 ), // -##-
        // 13
        	new( 64,   0, 16, 16 ), // -###
        	new( 48,  64, 16, 16 ), // #CC#
        	new( 64,  16, 16, 16 ), // #CC#
        	new( 80,  16, 16, 16 ), // -##-
        // 14
        	new( 32,  64, 16, 16 ), // ###-
        	new( 80,   0, 16, 16 ), // #CC#
        	new( 64,  16, 16, 16 ), // #CC#
        	new( 80,  16, 16, 16 ), // -##-
        // 15
        	new( 64,   0, 16, 16 ), // -##-
        	new( 80,   0, 16, 16 ), // #CC#
        	new( 64,  16, 16, 16 ), // #CC#
        	new( 80,  16, 16, 16 ), // -##-
        // 16
        	new(  0,  64, 16, 16 ), // -###
        	new( 16,  64, 16, 16 ), // -C#C
        	new(  0,  80, 16, 16 ), // -C#C
        	new( 16,  80, 16, 16 ), // -###
        // 17
        	new(  0,  64, 16, 16 ), // -##-
        	new( 80,   0, 16, 16 ), // -CC#
        	new(  0,  80, 16, 16 ), // -C#C
        	new( 16,  80, 16, 16 ), // -###
        // 18
        	new(  0,  64, 16, 16 ), // -###
        	new( 16,  64, 16, 16 ), // -C#C
        	new(  0,  80, 16, 16 ), // -CC#
        	new( 80,  16, 16, 16 ), // -##-
        // 19
        	new(  0,  64, 16, 16 ), // -##-
        	new( 80,   0, 16, 16 ), // -CC#
        	new(  0,  80, 16, 16 ), // -CC#
        	new( 80,  16, 16, 16 ), // -##-
        // 20
        	new( 32,  32, 16, 16 ), // ----
        	new( 48,  32, 16, 16 ), // #CC#
        	new( 32,  48, 16, 16 ), // ####
        	new( 48,  48, 16, 16 ), // #CC#
        // 21
        	new( 32,  32, 16, 16 ), // ----
        	new( 48,  32, 16, 16 ), // #CC#
        	new( 32,  48, 16, 16 ), // ##C#
        	new( 80,  16, 16, 16 ), // #C#-
        // 22
        	new( 32,  32, 16, 16 ), // ----
        	new( 48,  32, 16, 16 ), // #CC#
        	new( 64,  16, 16, 16 ), // #C##
        	new( 48,  48, 16, 16 ), // -#C#
        // 23
        	new( 32,  32, 16, 16 ), // ----
        	new( 48,  32, 16, 16 ), // #CC#
        	new( 64,  16, 16, 16 ), // #CC#
        	new( 80,  16, 16, 16 ), // -##-
        // 24
        	new( 64,  64, 16, 16 ), // ###-
        	new( 80,  64, 16, 16 ), // C#C-
        	new( 64,  80, 16, 16 ), // C#C-
        	new( 80,  80, 16, 16 ), // ###-
        // 25
        	new( 64,  64, 16, 16 ), // ###-
        	new( 80,  64, 16, 16 ), // C#C-
        	new( 64,  16, 16, 16 ), // #CC-
        	new( 80,  80, 16, 16 ), // -##-
        // 26
        	new( 64,   0, 16, 16 ), // -##-
        	new( 80,  64, 16, 16 ), // #CC-
        	new( 64,  80, 16, 16 ), // C#C-
        	new( 80,  80, 16, 16 ), // ###-
        // 27
        	new( 64,   0, 16, 16 ), // -##-
        	new( 80,  64, 16, 16 ), // #CC-
        	new( 64,  16, 16, 16 ), // #CC-
        	new( 80,  80, 16, 16 ), // -##-
        // 28
        	new( 32,  96, 16, 16 ), // #CC#
        	new( 48,  96, 16, 16 ), // ####
        	new( 32, 112, 16, 16 ), // #CC#
        	new( 48, 112, 16, 16 ), // ----
        // 29
        	new( 64,   0, 16, 16 ), // -#C#
        	new( 48,  96, 16, 16 ), // #C##
        	new( 32, 112, 16, 16 ), // #CC#
        	new( 48, 112, 16, 16 ), // ----
        // 30
        	new( 32,  96, 16, 16 ), // #C#-
        	new( 80,   0, 16, 16 ), // ##C#
        	new( 32, 112, 16, 16 ), // #CC#
        	new( 48, 112, 16, 16 ), // ----
        // 31
        	new( 64,   0, 16, 16 ), // -##-
        	new( 80,   0, 16, 16 ), // #CC#
        	new( 32, 112, 16, 16 ), // #CC#
        	new( 48, 112, 16, 16 ), // ----
        // 32
        	new(  0,  64, 16, 16 ), // -##-
        	new( 80,  64, 16, 16 ), // -CC-
        	new(  0,  80, 16, 16 ), // -CC-
        	new( 80,  80, 16, 16 ), // -##-
        // 33
        	new( 32,  32, 16, 16 ), // ----
        	new( 48,  32, 16, 16 ), // #CC#
        	new( 32, 112, 16, 16 ), // #CC#
        	new( 48, 112, 16, 16 ), // ----
        // 34
        	new(  0,  32, 16, 16 ), // ----
        	new( 16,  32, 16, 16 ), // -C#C
        	new(  0,  48, 16, 16 ), // -###
        	new( 16,  48, 16, 16 ), // -C#C
        // 35
        	new(  0,  32, 16, 16 ), // ----
        	new( 16,  32, 16, 16 ), // -C#C
        	new(  0,  48, 16, 16 ), // -#C#
        	new( 80,  16, 16, 16 ), // -C#-
        // 36
        	new( 64,  32, 16, 16 ), // ----
        	new( 80,  32, 16, 16 ), // C#C-
        	new( 64,  48, 16, 16 ), // ###-
        	new( 80,  48, 16, 16 ), // C#C-
        // 37
        	new( 64,  32, 16, 16 ), // ----
        	new( 80,  32, 16, 16 ), // C#C-
        	new( 64,  16, 16, 16 ), // #C#-
        	new( 80,  48, 16, 16 ), // -#C-
        // 38
        	new( 64,  96, 16, 16 ), // C#C-
        	new( 80,  96, 16, 16 ), // ###-
        	new( 64, 112, 16, 16 ), // C#C-
        	new( 80, 112, 16, 16 ), // ----
        // 39
        	new( 64,   0, 16, 16 ), // -#C-
        	new( 80,  96, 16, 16 ), // #C#-
        	new( 64, 112, 16, 16 ), // C#C-
        	new( 80, 112, 16, 16 ), // ----
        // 40
        	new(  0,  96, 16, 16 ), // -C#C
        	new( 16,  96, 16, 16 ), // -###
        	new(  0, 112, 16, 16 ), // -C#C
        	new( 16, 112, 16, 16 ), // ----
        // 41
        	new(  0,  96, 16, 16 ), // -C#-
        	new( 80,   0, 16, 16 ), // -#C#
        	new(  0, 112, 16, 16 ), // -C#C
        	new( 16, 112, 16, 16 ), // ----
        // 42
        	new(  0,  32, 16, 16 ), // ----
        	new( 80,  32, 16, 16 ), // -CC-
        	new(  0,  48, 16, 16 ), // -##-
        	new( 80,  48, 16, 16 ), // -CC-
        // 43
        	new(  0,  32, 16, 16 ), // ----
        	new( 16,  32, 16, 16 ), // -C#C
        	new(  0, 112, 16, 16 ), // -C#C
        	new( 16, 112, 16, 16 ), // ----
        // 44
        	new(  0,  96, 16, 16 ), // -CC-
        	new( 80,  96, 16, 16 ), // -##-
        	new(  0, 112, 16, 16 ), // -CC-
        	new( 80, 112, 16, 16 ), // ----
        // 45
        	new( 64,  32, 16, 16 ), // ----
        	new( 80,  32, 16, 16 ), // C#C-
        	new( 64, 112, 16, 16 ), // C#C-
        	new( 80, 112, 16, 16 ), // ----
        // 46
        	new(  0,  32, 16, 16 ), // ----
        	new( 80,  32, 16, 16 ), // -CC-
        	new(  0, 112, 16, 16 ), // -CC-
        	new( 80, 112, 16, 16 ), // ----
        // 47
        	new(  0,   0, 16, 16 ), // UUUU
        	new( 16,   0, 16, 16 ), // UCCU
        	new(  0,  16, 16, 16 ), // UCCU
        	new( 16,  16, 16, 16 )  // UUUU
        ];
    }
}