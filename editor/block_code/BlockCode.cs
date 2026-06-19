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

		public void BlockMoving(BaseBlock block) => _movingBlock = block;
		public void BlockStoppedMoving(BaseBlock block)
		{
			if (block == _movingBlock)
			{
				_movingBlock = null;
				_hoveredConnector?.Connect(block);
				_hoveredConnector?.RemoveChild(_blockPreviewControl);
			}
		}

		public void ConnectorHovered(BlockConnector connector)
		{
			GD.Print("Hovered");
			if (!connector.IsConnected() && _movingBlock != null && !_movingBlock.Connectors.Contains(connector))
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
		/*
		private void ConnectBlock(Node node)
		{
			if (node is BaseBlock block)
			{
				void Moving() => BlockMoving(block);
				void MovingStopped() => BlockStoppedMoving(block);

				_movingStartedHandlers[block] = Moving;
				_movingStoppedHandlers[block] = MovingStopped;

				block.StartMoving += Moving;
				block.StoppedMoving += MovingStopped;

				foreach (BlockConnector connector in block.Connectors)
				{
					void Hovered() => ConnectorHovered(connector);
					void Unhovered() => ConnectorUnhovered(connector);

					_hoverHandlers[connector] = Hovered;
					_unhoverHandlers[connector] = Unhovered;

					connector.MouseHovered += Hovered;
					connector.MouseUnhovered += Unhovered;
				}
			}
		}

		private void DisconnectBlock(Node node)
		{
			if (node is BaseBlock block)
			{
				if (_movingStartedHandlers.TryGetValue(block, out BaseBlock.StartMovingEventHandler movingStartedAction))
				{
					block.StartMoving -= movingStartedAction;
					_movingStartedHandlers.Remove(block);
				}
				if (_movingStoppedHandlers.TryGetValue(block, out BaseBlock.StoppedMovingEventHandler movingStoppedAction))
				{
					block.StoppedMoving -= movingStoppedAction;
					_movingStoppedHandlers.Remove(block);
				}
				foreach (BlockConnector connector in block.Connectors)
				{
					if (_hoverHandlers.TryGetValue(connector, out BlockConnector.MouseHoveredEventHandler hoverAction))
					{
						connector.MouseHovered -= hoverAction;
						_hoverHandlers.Remove(connector);
					}
					if (_unhoverHandlers.TryGetValue(connector, out BlockConnector.MouseUnhoveredEventHandler unhoverAction))
					{
						connector.MouseUnhovered -= unhoverAction;
						_unhoverHandlers.Remove(connector);
					}
				}
			}
		}
		*/
	}
}
