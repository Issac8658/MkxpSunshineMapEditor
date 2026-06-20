using Godot;

namespace BlockCode
{
    public partial class ValueBlockComponent : BaseBlockComponent
    {
        [Export]
        public BlockConnector Connector;

        public virtual Variant GetValue() => new();
    }
}