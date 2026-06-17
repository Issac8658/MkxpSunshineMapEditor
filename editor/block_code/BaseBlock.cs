using Godot;

namespace BlockCode
{
	[Icon("res://assets/icons/block_code/base_block.tres")]
	public partial class BaseBlock : Control
	{
		public const bool CONNECTABLE = false;

		private bool _moving = false;

		public BlockConnector ConnectedTo = null;

		[Export]
		public BlockConnector.BlockConnectionType ConnectionType = BlockConnector.BlockConnectionType.Default;
		[Export]
		public Control MoveZone;
		[Export]
		public Container ContentConatiner;
		[Export]
		public BlockConnector[] Connectors;

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
						Disconnect();
						Node parent = GetParent();
						parent.MoveChild(this, -1);
					}

				}
				else if (@event is InputEventMouseMotion EventMotion)
					if (_moving)
						Position += EventMotion.Relative;
			};
		}

		public bool IsConnected() => ConnectedTo != null;

		public void Connect(BaseBlock block, int connectiorId = 0)
		{
			if (connectiorId >= 0 && connectiorId < Connectors.Length)
				Connectors[connectiorId].Connect(block);
			else
				GD.PushWarning(connectiorId);
		}

		public void Disconnect() => ConnectedTo?.Disconnect();
		
		public virtual void _OnMove() { }
	}
}
