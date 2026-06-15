using System;
using Godot;
using Godot.Collections;

namespace RPG
{
    #region Ruby Classes
    public partial class RubySerializable : RefCounted
    {
        public Variant RawData;
    }

    [RubySerializableClass("RPG::MapInfo")]
    public partial class MapInfo : RubySerializable
    {
        [RubySerializable("@scroll_x")]
        private int _scrollX = 0;
        [RubySerializable("@scroll_y")]
        private int _scrollY = 0;

        [RubySerializable("@name")]
        public string Name = "";
        [RubySerializable("@parent_id")]
        public int ParentID = 0;
        [RubySerializable("@order")]
        public int Order = 0;
        [RubySerializable("@expanded")]
        public bool Expanded = false;
        public Vector2I Scroll
        {
            get => new(_scrollX, _scrollY);
            set
            {
                _scrollX = value.X;
                _scrollY = value.Y;
            }
        }
    }

    #region RPG::Map
    [RubySerializableClass("RPG::Map")]
    public partial class Map : RubySerializable
    {
        [RubySerializable("@width")]
        private int _width = 0;
        [RubySerializable("@height")]
        private int _height = 0;

        [RubySerializable("@tileset_id")]
        public int TilesetID = 1;
        public Vector2I Size
        {
            get => new(_width, _height);
        }
        [RubySerializable("@autoplay_bgm")]
        public bool AutoplayBGM = false;
        [RubySerializable("@bgm")]
        public AudioFile BGM;
        [RubySerializable("@autoplay_bgs")]
        public bool AutoplayBGS = false;
        [RubySerializable("@bgs")]
        public AudioFile BGS;
        [RubySerializable("@encounter_list")]
        public Godot.Collections.Array EncounterList = [];
        [RubySerializable("@encounter_step")]
        public int EncounterStep = 30;
        [RubySerializable("@data")]
        public Table Data;
        [RubySerializable("@events")]
        public Dictionary<int, Event> Events = [];
    }

    // RPG::Event
    [RubySerializableClass("RPG::Event")]
    public partial class Event : RubySerializable
    {
        [RubySerializable("@x")]
        private int _positionX = 0;
        [RubySerializable("@y")]
        private int _positionY = 0;

        [RubySerializable("@id")]
        public int ID = 0;
        [RubySerializable("@name")]
        public string Name = "";
        public Vector2I Position
        {
            get => new(_positionX, _positionY);
            set
            {
                _positionX = value.X;
                _positionY = value.Y;
            }
        }
        [RubySerializable("@pages")]
        public Array<Page> Pages = [new()];

        [RubySerializableClass("RPG::Event::Page")]
        public partial class Page : RubySerializable
        {
            [RubySerializable("@condition")]
            public Condition PageCondition = new();
            [RubySerializable("@graphic")]
            public Graphic PageGraphic = new();
            [RubySerializable("@move_type")]
            public int MoveType = 0;
            [RubySerializable("@move_speed")]
            public int MoveSpeed = 3;
            [RubySerializable("@move_frequency")]
            public int MoveFrequency = 3;
            [RubySerializable("@move_route")]
            public MoveRoute PageMoveRoute = new();
            [RubySerializable("@walk_anime")]
            public bool WalkAnime = true;
            [RubySerializable("@step_anime")]
            public bool StepAnime = false;
            [RubySerializable("@direction_fix")]
            public bool DirectionFix = false;
            [RubySerializable("@through")]
            public bool Through = false;
            [RubySerializable("@always_on_top")]
            public bool AlwaysOnTop = false;
            [RubySerializable("@trigger")]
            public int trigger = 0;
            [RubySerializable("@list")]
            public Array<EventCommand> List = [];

            [RubySerializableClass("RPG::Event::Page::Condition")]
            public partial class Condition : RubySerializable
            {
                [RubySerializable("@switch1_valid")]
                public bool Switch1Valid = false;
                [RubySerializable("@switch2_valid")]
                public bool Switch2Valid = false;
                [RubySerializable("@variable_valid")]
                public bool VariableValid = false;
                [RubySerializable("@self_switch_valid")]
                public bool SelfSwitchValid = false;
                [RubySerializable("@switch1_id")]
                public int Switch1ID = 1;
                [RubySerializable("@switch2_id")]
                public int Switch2ID = 1;
                [RubySerializable("@variable_id")]
                public int VariableID = 1;
                [RubySerializable("@variable_value")]
                public int VariableValue = 1;
                [RubySerializable("@self_switch_ch")]
                public string SelfSwitchCH = "A";
            }

            [RubySerializableClass("RPG::Event::Page::Graphic")]
            public partial class Graphic : RubySerializable
            {
                [RubySerializable("@tile_id")]
                public int TileID = 0;
                [RubySerializable("@character_name")]
                public string CharacterName = "";
                [RubySerializable("@character_hue")]
                public int CharacterHue = 0;
                [RubySerializable("@direction")]
                public int Direction = 2;
                [RubySerializable("@pattern")]
                public int Pattern = 0;
                [RubySerializable("@opacity")]
                public int Opacity = 255;
                [RubySerializable("@blend_type")]
                public int BlendType = 0;
            }
        }
    }

    [RubySerializableClass("RPG::MoveRoute")]
    public partial class MoveRoute : RubySerializable
    {
        [RubySerializable("@repeat")]
        public bool Repeat = true;
        [RubySerializable("@skippable")]
        public bool Skippable = false;
        [RubySerializable("@list")]
        public Array<MoveCommand> List = [];
    }

    [RubySerializableClass("RPG::MoveCommand")]
    public partial class MoveCommand : RubySerializable
    {
        [RubySerializable("@code")]
        public int Code = 0;
        [RubySerializable("@parameters")]
        public Godot.Collections.Array Parameters = [];
    }

    [RubySerializableClass("RPG::EventCommand")]
    public partial class EventCommand : RubySerializable
    {
        [RubySerializable("@code")]
        public int Code = 0;
        [RubySerializable("@indent")]
        public int Indent = 0;
        [RubySerializable("@parameters")]
        public Godot.Collections.Array RawParameters;
    }
    #endregion
    #region RPG::Tileset
    [RubySerializableClass("RPG::Tileset")]
    public partial class Tileset : RubySerializable
    {
        [RubySerializable("@id")]
        public int ID = 0;
        [RubySerializable("@name")]
        public string Name = "";
        [RubySerializable("@tileset_name")]
        public string TilesetName = "";
        [RubySerializable("@autotile_names")]
        public Array<string> AutotileNames = ["", "", "", "", "", "", ""];
        [RubySerializable("@panorama_name")]
        public string PanoramaName = "";
        [RubySerializable("@panorama_hue")]
        public int PanoramaHUE = 0;
        [RubySerializable("@fog_name")]
        public string FogName = "";
        [RubySerializable("@fog_hue")]
        public int FogHUE = 0;
        [RubySerializable("@fog_opacity")]
        public int FogOpacity = 64;
        [RubySerializable("@fog_blend_type")]
        public int FogBlendType = 0;
        [RubySerializable("@fog_zoom")]
        public int FogZoom = 200;
        [RubySerializable("@fog_sx")]
        public int FogSX = 0;
        [RubySerializable("@fog_sy")]
        public int FogSY = 0;
        [RubySerializable("@battleback_name")]
        public string BattleBackName = "";
        [RubySerializable("@passages")]
        public Table Passages;
        [RubySerializable("@priorities")]
        public Table Priorities;
        [RubySerializable("@terrain_tags")]
        public Table TerrainTags;
    }
    #endregion
    #region Other
    [RubySerializableClass("Table")]
    public partial class Table : RubySerializable
    {
        [RubySerializable("table_data")]
        public Array<Array<Array<int>>> TableData;
    }

    [RubySerializableClass("RPG::AudioFile")]
    public partial class AudioFile : RubySerializable
    {
        [RubySerializable("@name")]
        public string Name = "";
        [RubySerializable("@volume")]
        public int Volume = 100;
        [RubySerializable("@pitch")]
        public int Pitch = 100;
    }
    #endregion
    #endregion
}