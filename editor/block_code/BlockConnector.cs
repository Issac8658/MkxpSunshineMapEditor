using Godot;

namespace BlockCode
{
	[Icon("res://assets/icons/block_code/connector.tres")]
	public partial class BlockConnector : Node
	{
		public BaseBlock _connection = null;

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
		[Export]
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

		private Node _oldConnectionParent;

		public void Connect(BaseBlock block)
		{
			if (_connection == null && block.ConnectionType == ConnectionType)
			{
				Disconnect();
				block.Disconnect();
				_oldConnectionParent = block.GetParent();
				_connection = block;
				AddChild(_connection);
				block.ConnectedTo = this;
			}
		}

		public void Disconnect()
		{
			if (_connection != null)
			{
				_oldConnectionParent.AddChild(_connection);
				_oldConnectionParent = null;
				_connection.ConnectedTo = null;
				_connection = null;
			}
		}
	}
}
