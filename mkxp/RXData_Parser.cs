using System.IO;
using System.Text;
using Godot;
using Godot.Collections;

namespace RPG
{
    public partial class RXData_Parser : RefCounted
    {
        private Array _symbolCache = [];
        private Array _objectCache = [];

        public Variant Data = default;

        public RXData_Parser(string path)
        {
            Data = ParseRXData(path);
        }

        public Variant ParseRXData(string path)
        {
            path = ProjectSettings.GlobalizePath(path);

            GD.Print($"Parsing {path}");

            _symbolCache.Clear();
            _objectCache.Clear();

            if (!File.Exists(path))
            {
                GD.PushWarning($"Unable to open file");
                return default;
            }

            using FileStream fs = new(path, FileMode.Open, System.IO.FileAccess.Read);
            using BinaryReader reader = new(fs);
            
            byte major = reader.ReadByte();
            byte minor = reader.ReadByte();

            GD.Print($"Marshal {major}.{minor}");
            if (major != 4 || minor != 8)
            {
                GD.PushWarning($"Unsupported Marshal version, deserializing impossible");
                return default;
            }

            GD.Print("Deserializing...");
            Variant data = ParseMarshalElement(reader);
            GD.Print("Deserializing success");
            return data;
        }

        private Variant ParseMarshalElement(BinaryReader reader)
        {
            if (reader.BaseStream.Position >= reader.BaseStream.Length)
                return default;
            
            char type_flag = (char)reader.ReadByte();

            switch (type_flag)
            {
                case '0': // null
                    { return default; }
                case 'T':
                    { return true; }
                case 'F':
                    { return false; }
                
                case 'i': // fixnum (int)
                    { return ParseFixnum(reader); }

                case ':': // symbol
                    {
                        int len = ParseFixnum(reader);
                        byte[] bytes = reader.ReadBytes(len);
                        string sym = Encoding.UTF8.GetString(bytes);
                        _symbolCache.Add(sym);
                        return sym;
                    }
                
                case ';': // symlink
                    {
                        int i = ParseFixnum(reader);
                        if (i >= 0 && i < _symbolCache.Count)
                            return _symbolCache[i];
                        return $"unknown_sym_{i}";
                    }
                
                case '"': // string
                    {
                        int len = ParseFixnum(reader);
                        byte[] bytes = reader.ReadBytes(len);
                        string str = Encoding.UTF8.GetString(bytes);
                        _objectCache.Add(str);
                        return str;
                    }
                
                case '[': // array
                    {
                        int size = ParseFixnum(reader);

                        Array arr = [];

                        if (size < 0 || size > 50000)
                        {
                            GD.PrintErr($"Array length is incorrect ({size})");
                            return arr;
                        }

                        _objectCache.Add(arr);

                        for (int i = 0; i < size; i++)
                            arr.Add(ParseMarshalElement(reader));
                        return arr;
                    }
                
                case '{': // Hash (Dictionary)
                    {
                        int size = ParseFixnum(reader);

                        Dictionary dict = [];

                        if (size < 0 || size > 10000)
                        {
                            GD.PrintErr($"Hash length is incorrect ({size})");
                            return dict;
                        }

                        _objectCache.Add(dict);
                        for (int i = 0; i < size; i++)
                        {
                            Variant key = ParseMarshalElement(reader);
                            Variant value = ParseMarshalElement(reader);
                            if (key.VariantType != Variant.Type.Nil)
                                dict[key] = value;
                        }
                        return dict;
                    }
                
                case 'o': // class object
                    {
                        Variant klass = ParseMarshalElement(reader); // symbol
                        int num_vars = ParseFixnum(reader);
                        
                        Dictionary obj_data = new() {{"__class__", klass}};

                        if (num_vars < 0 || num_vars > 256)
                        {
                            GD.PrintErr($"Incorrect attributes count (class \"{klass}\", {num_vars})");
                            return obj_data;
                        }
                        _objectCache.Add(obj_data);

                        for (int i = 0; i < num_vars; i++)
                        {
                            Variant var_name = ParseMarshalElement(reader);
                            Variant var_value = ParseMarshalElement(reader);
                            if (var_name.VariantType != Variant.Type.Nil)
                                obj_data[var_name] = var_value;
                        }
                        return obj_data;
                    }

                case 'I': // IVAR
                    {
                        Variant i_el = ParseMarshalElement(reader); // symbol
                        int num_ivars = ParseFixnum(reader);
                        
                        Dictionary ivar_data = new() {{"__data__", i_el}};

                        if (num_ivars < 0 || num_ivars > 256)
                        {
                            GD.PrintErr($"Incorrect ivars count (ivar \"{i_el}\", {num_ivars})");
                            return ivar_data;
                        }

                        //_objectCache.Add(ivar_data);
                        for (int i = 0; i < num_ivars; i++)
                        {
                            Variant ivar_name = ParseMarshalElement(reader);
                            Variant ivar_value = ParseMarshalElement(reader);
                            if (ivar_name.VariantType != Variant.Type.Nil)
                                ivar_data[ivar_name] = ivar_value;
                        }
                        return ivar_data;
                    }
                
                case '@': // object link
                    {
                        int i = ParseFixnum(reader);
                        if (i >= 0 && i < _objectCache.Count)
                            return _objectCache[i];
                        return default;
                    }
                
                case 'u': // userdata or table or color or tone
                    {
                        Variant klass = ParseMarshalElement(reader);
                        int len = ParseFixnum(reader);
                        byte[] buffer = reader.ReadBytes(len);

                        Dictionary obj_data = new() {{"__class__", klass}, {"__is_userdata__", true}};
                        _objectCache.Add(obj_data);

                        switch (klass.AsString())
                        {
                            case "Table":
                                {
                                    obj_data["table_data"] = ParseTable(buffer);
                                    break;
                                }
                            case "Color":
                                {
                                    obj_data["color"] = ParseColor(buffer);
                                    break;
                                }
                            case "Tone":
                                {
                                    obj_data["tone"] = ParseColor(buffer);
                                    break;
                                }
                            default:
                                {
                                    obj_data["raw"] = buffer;
                                    break;
                                }
                        }
                        
                        return obj_data;
                    }
                default:
                    {
                        GD.PushWarning($"Unknown Marshal flag '{type_flag}' in {reader.BaseStream.Position - 1}");
                        return default;
                    }
            }
        }

        private static int ParseFixnum(BinaryReader reader)
        {
            sbyte c = reader.ReadSByte();

            if (c == 0) return 0;
            if (c > 5)  return c - 5;
            if (c < -5) return c + 5;

            if (c > 0 && c <= 4)
            {
                int value = 0;
                for (int i = 0; i < c; i++)
                    value |= reader.ReadByte() << (8 * i);
                return value;
            }

            if (c < 0 && c >= -4)
            {
                int value = -1;
                int bytes_count = -c;
                for (int i = 0; i < bytes_count; i++)
                {
                    int mask = ~(0xFF << (8 * i));
                    value = (value & mask) | (reader.ReadByte() << (8 * i));
                }
                return value;
            }

            return 0;
        }

        private static Array<Array<Array<short>>> ParseTable(byte[] buffer)
        {
            StreamPeerBuffer stream = new() {DataArray = buffer};

            if (buffer.Length < 20)
                return [];
            
            int d = stream.Get32(); // dimentions // always 3 for mkxp
            int w = stream.Get32(); // width
            int h = stream.Get32(); // height
            int l = stream.Get32(); // layers
            int t = stream.Get32(); // total tiles (w * h * l)

            Array<Array<Array<short>>> grid = [];
            for (int z = 0; z < l; z++)
            {
                Array<Array<short>> layer = [];
                for (int y = 0; y < h; y++)
                {
                    Array<short> row = [];
                    for (int x = 0; x < w; x++)
                        row.Add(stream.Get16());
                    layer.Add(row);
                }
                grid.Add(layer);
            }

            return grid;
        }

        private static Color ParseColor(byte[] buffer)
        {
            StreamPeerBuffer stream = new() {DataArray = buffer};
            if (buffer.Length < 32) return Colors.White;
            return new(
                (float)(stream.GetDouble() / 255.0),
                (float)(stream.GetDouble() / 255.0),
                (float)(stream.GetDouble() / 255.0),
                (float)(stream.GetDouble() / 255.0)
            );
        }
    }
}