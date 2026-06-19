using Godot;

namespace BlockCode
{
    public partial class BoolValueBlock : BaseBlock
    {
        [Export]
        public bool Value = false;

        public virtual bool GetValue() => Value;
    }
}