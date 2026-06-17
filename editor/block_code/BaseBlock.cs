using Godot;

namespace BlockCode
{
	public partial class BaseBlock : Control
	{
		[Export]
		public Control MoveZone;
		[Export]
		public Container ContentConatiner;

		private bool _moving = false;

		public override void _Ready()
		{
			MoveZone.MouseDefaultCursorShape = CursorShape.Move;
			MoveZone.GuiInput += (@event) =>
			{
				if (@event is InputEventMouseButton EventButton)
				{
					//if (EventButton.ButtonIndex == MouseButton.Left || EventButton.ButtonIndex == MouseButton.Right)
					_moving = EventButton.Pressed;
					if (_moving)
					{
						Node parent = GetParent();
						parent.MoveChild(this, -1);
					}

				}
				else if (@event is InputEventMouseMotion EventMotion)
					if (_moving)
						Position += EventMotion.Relative;
			};
		}
		
		public virtual void _OnMove() { }
	}
}
