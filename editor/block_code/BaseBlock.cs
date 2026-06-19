using Godot;

namespace BlockCode
{
	[Icon("res://assets/icons/block_code/base_block.tres")]
	public partial class BaseBlock : Control
	{
		[Signal]
		public delegate void StartMovingEventHandler();
		[Signal]
		public delegate void StoppedMovingEventHandler();

		public const bool CONNECTABLE = false;

		private BlockCode _code;
		private bool _moving = false;
		private Vector2 _oldMousePos;
		private bool _mousePressed;

		public BlockConnector ConnectedTo = null;

		[Export]
		public BlockConnector.BlockConnectionType ConnectionType = BlockConnector.BlockConnectionType.Default;
		[Export]
		public Control MoveZone;
		[Export]
		public Container ContentConatiner;
		[Export]
		public BlockConnector[] Connectors;

		public bool Moving { get => _moving; }

		public override void _EnterTree()
		{
			Node parent = this;
			while (parent != null && parent is not BlockCode)
				parent = parent.GetParent();
			if (parent != null)
				_code = parent as BlockCode;
		}

		public override void _ExitTree()
		{
			_code = null;
		}


		public override void _Ready()
		{
			foreach (BlockConnector connector in Connectors)
			{
				connector.MouseHovered += () => _code?.ConnectorHovered(connector);
				connector.MouseUnhovered += () => _code?.ConnectorUnhovered(connector);
			}

			MoveZone.MouseDefaultCursorShape = CursorShape.Move;
			
			MoveZone.GuiInput += (@event) =>
			{
				if (@event is InputEventMouseButton EventButton)
				{
					if (EventButton.ButtonIndex == MouseButton.Left)
						if (EventButton.Pressed)
						{
							_mousePressed = true;
							StartMove();
						}
						/*else
							StopMove();*/
				}
				/*else if (@event is InputEventMouseMotion EventMotion)
					if (_moving)
						Position += EventMotion.Relative;*/
			};
			
		}

		public override void _Process(double delta)
		{
			Vector2 mousePos = MoveZone.GetGlobalMousePosition();

			bool currentMousePressed = Input.IsMouseButtonPressed(MouseButton.Left);
			if (currentMousePressed != _mousePressed)
			{
				if (!currentMousePressed)
				/*{
					if (MoveZone.GetGlobalRect().HasPoint(mousePos))
						StartMove();
				}
				else*/
					StopMove();
				
				_mousePressed = currentMousePressed;
			}

			if (_moving)
				Position += mousePos - _oldMousePos;
			
			_oldMousePos = mousePos;
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

		private void StartMove()
		{
			Size = Vector2.Zero;
			_moving = true;

			Disconnect();
			Node parent = GetParent();
			parent.MoveChild(this, -1);

			_code?.BlockStartedMoving(this);
			EmitSignal("StartMoving");
		}

		private void StopMove()
		{
			Size = Vector2.Zero;
			_moving = false;

			_code?.BlockStoppedMoving(this);
			EmitSignal("StoppedMoving");
		}
	}
}
