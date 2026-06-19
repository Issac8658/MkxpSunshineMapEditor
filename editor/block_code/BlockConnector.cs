using Godot;

namespace BlockCode
{
	[Icon("res://assets/icons/block_code/connector.tres")]
	public partial class BlockConnector : Node
	{
		[Signal]
		public delegate void MouseHoveredEventHandler();
		[Signal]
		public delegate void MouseUnhoveredEventHandler();

		private BaseBlock _connection = null;
		private bool _hovered = false;
		private Node _oldConnectionParent;
		private Viewport _viewport;

		public enum BlockConnectionType
		{
			Default = 0,
			Value = 1
		}

		[Export]
		public BlockConnectionType ConnectionType = BlockConnectionType.Default;
		/// <summary>
		/// мямямя
		/// </summary>
		[Export]
		public Control ConnectionZone;

		public BaseBlock Connection
		{
			get => _connection;
			set
			{
				if (value == null)
					Disconnect();
				else if (value != _connection)
				{
					BaseBlock oldConnection = _connection;
					Connect(value);
					value.Connect(oldConnection);
				}
			}
		}

		public override void _EnterTree()
		{
			_viewport = GetViewport();
		}

		public override void _ExitTree()
		{
			_viewport = null;
		}

		public override void _Process(double delta)
		{
			if (_viewport != null)
			{
				bool mouseHovered = ConnectionZone.GetGlobalRect().HasPoint(ConnectionZone.GetGlobalMousePosition());
				if (mouseHovered != _hovered)
				{
					if (mouseHovered)
						EmitSignal("MouseHovered");
					else
						EmitSignal("MouseUnhovered");
					_hovered = mouseHovered;
					GD.Print(mouseHovered);
				}
			}
		}

		public void Connect(BaseBlock block)
		{
			if (_connection == null && block.ConnectionType == ConnectionType)
			{
				Disconnect();
				block.Disconnect();
				_oldConnectionParent = block.GetParent();
				_connection = block;
				_oldConnectionParent?.RemoveChild(_connection);
				AddChild(_connection);
				block.ConnectedTo = this;
			}
		}

		public void Disconnect()
		{
			if (_connection != null)
			{
				Vector2 oldPos = _connection.GlobalPosition;
				_connection.GetParent()?.RemoveChild(_connection);
				_oldConnectionParent.AddChild(_connection);
				_connection.GlobalPosition = oldPos;
				_oldConnectionParent = null;
				_connection.ConnectedTo = null;
				_connection = null;
			}
		}

		public bool IsConnected() => _connection != null;
	}
}
