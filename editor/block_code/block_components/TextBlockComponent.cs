using Godot;

namespace BlockCode
{
    public partial class TextBlockComponent : BaseBlockComponent
    {
        [Export]
        public Label TextLabel;

        public string Text
        {
            get => TextLabel.Text;
            set => TextLabel.Text = value;
        }
    }
}