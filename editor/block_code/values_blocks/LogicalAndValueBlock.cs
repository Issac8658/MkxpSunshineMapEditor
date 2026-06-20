using Godot;

namespace BlockCode
{
    public partial class LogicalAndValueBlock : BaseBlock
    {
        public virtual bool GetValue()
        {
            bool val1 = false;
            bool val2 = false;

            if (Connectors[0].Connection is BoolValueBlock)
            {
                
            }

            return val1 && val2;
        }
    }
}