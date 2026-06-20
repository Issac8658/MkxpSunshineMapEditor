using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BlockCode
{
	[GlobalClass]
	[Icon("res://assets/icons/block_code/block_code.tres")]
	public partial class BlockCode : Control
	{
		private BaseBlock _movingBlock;
		private BlockConnector _hoveredConnector;

		private Control _blockPreviewControl;
		private Control _blockValuePreviewControl;

		[Export]
		public PackedScene BlockPreviewTemplate;
		[Export]
		public PackedScene BlockValuePreviewTemplate;


		public override void _Ready()
		{
			_blockPreviewControl = BlockPreviewTemplate.Instantiate<Control>();
			_blockValuePreviewControl = BlockValuePreviewTemplate.Instantiate<Control>();
		}

		public void BlockStartedMoving(BaseBlock block) {
			BlockStoppedMoving(_movingBlock);
			_movingBlock = block;
		}
		public void BlockStoppedMoving(BaseBlock block)
		{
			//GD.Print("Stopped in code");
			if (block == _movingBlock)
			{
				_movingBlock = null;
				if (CanConnect(_hoveredConnector, block))
				{
					_hoveredConnector?.Connect(block);
					_blockPreviewControl.GetParent()?.RemoveChild(_blockPreviewControl);
					_blockValuePreviewControl.GetParent()?.RemoveChild(_blockValuePreviewControl);
				}
			}
		}

		public void ConnectorHovered(BlockConnector connector)
		{
			//GD.Print("Hovered");
			if (CanConnect(connector, _movingBlock))
			{
				_hoveredConnector = connector;
				if (connector.ConnectionType == BlockConnector.BlockConnectionType.Default)
				{
					_blockPreviewControl.CustomMinimumSize = _movingBlock.Size;
					_blockPreviewControl.GetParent()?.RemoveChild(_blockPreviewControl);
					_hoveredConnector.AddChild(_blockPreviewControl);
				}
				else
				{
					_blockValuePreviewControl.CustomMinimumSize = _movingBlock.Size;
					_blockValuePreviewControl.GetParent()?.RemoveChild(_blockValuePreviewControl);
					_hoveredConnector.AddChild(_blockValuePreviewControl);
				}
			}
		}
		public void ConnectorUnhovered(BlockConnector connector)
		{
			if (_hoveredConnector == connector)
			{
				if (connector.ConnectionType == BlockConnector.BlockConnectionType.Default)
				{
					if (_blockPreviewControl.GetParent() == _hoveredConnector)
						_hoveredConnector.RemoveChild(_blockPreviewControl);
				}
				else
					if (_blockValuePreviewControl.GetParent() == _hoveredConnector)
						_hoveredConnector.RemoveChild(_blockValuePreviewControl);
				_hoveredConnector = null;
			}
		}
		
		public static bool CanConnect(BlockConnector connector, BaseBlock block)
		{
			if (connector == null || block == null || block.ConnectionType != connector.ConnectionType || connector.IsConnected() || block.Connectors.Contains(connector))
				return false;
			
			Node previousNode = connector.GetParent();
			while (previousNode != null && previousNode is not BlockCode)
			{
				if (previousNode == block)
					return false;
				previousNode = previousNode.GetParent();
			}

			return true;
		}
	}
}
