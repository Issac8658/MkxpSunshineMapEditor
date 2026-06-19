using Godot;

namespace BlockCode
{
    public partial class BoolBlockComponent : BaseBlockComponent
    {
        [Export]
        public BlockConnector Connector;

        public bool GetValue() => Connector.Connection is BoolValueBlock valueBlock && valueBlock.GetValue();
    }
}