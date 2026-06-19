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
		//private readonly Dictionary<BlockConnector, BlockConnector.MouseHoveredEventHandler> _hoverHandlers = [];
		//private readonly Dictionary<BlockConnector, BlockConnector.MouseUnhoveredEventHandler> _unhoverHandlers = [];
		//private readonly Dictionary<BaseBlock, BaseBlock.StartMovingEventHandler> _movingStartedHandlers = [];
		//private readonly Dictionary<BaseBlock, BaseBlock.StoppedMovingEventHandler> _movingStoppedHandlers = [];

		private BaseBlock _movingBlock;
		private BlockConnector _hoveredConnector;

		private Control _blockPreviewControl;

		[Export]
		public PackedScene BlockPreviewTemplate;


		public override void _Ready()
		{
			_blockPreviewControl = BlockPreviewTemplate.Instantiate<Control>();

			/*
			foreach(Node node in GetChildren())
				ConnectBlock(node);

			ChildEnteredTree += ConnectBlock;
			ChildExitingTree += DisconnectBlock;
			*/
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
				}
			}
		}

		public void ConnectorHovered(BlockConnector connector)
		{
			//GD.Print("Hovered");
			if (CanConnect(connector, _movingBlock))
			{
				_hoveredConnector = connector;
				_blockPreviewControl.CustomMinimumSize = _movingBlock.Size;
				_blockPreviewControl.GetParent()?.RemoveChild(_blockPreviewControl);
				_hoveredConnector.AddChild(_blockPreviewControl);
			}
		}
		public void ConnectorUnhovered(BlockConnector connector)
		{
			if (_hoveredConnector == connector)
			{
				if (_blockPreviewControl.GetParent() == _hoveredConnector)
					_hoveredConnector.RemoveChild(_blockPreviewControl);
				_hoveredConnector = null;
			}
		}
		
		public static bool CanConnect(BlockConnector connector, BaseBlock block)
		{
			if (connector == null || block == null || connector.IsConnected() || block.Connectors.Contains(connector))
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
