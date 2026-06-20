using Godot;

namespace BlockCode
{
    public partial class BoolBlockComponent : ValueBlockComponent
    {
        public override Variant GetValue() => Connector.Connection is BoolValueBlock valueBlock && valueBlock.GetValue();
    }
}