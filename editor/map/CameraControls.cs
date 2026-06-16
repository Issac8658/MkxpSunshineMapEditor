using Godot;
using RPG;
using System;

public partial class CameraControls : Camera2D
{
	private Window window;

	[Export]
	public Control CameraControlZone;
	[Export]
	public bool BoundToMap = true;
	[Export]
	public double TargetZoom = 1;
	[Export]
	public double MinZoom = 1.0 / 16.0;
	[Export]
	public double MaxZoom = 128.0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		window = GetWindow();

		CameraControlZone.GuiInput += @event =>
		{
			if (@event is InputEventMouseButton eventButton)
			{
				if (eventButton.Pressed)
					if (eventButton.ButtonIndex == MouseButton.WheelUp)
						TargetZoom *= 2;
					else if (eventButton.ButtonIndex == MouseButton.WheelDown)
						TargetZoom *= 0.5;
					TargetZoom = Mathf.Clamp(TargetZoom, MinZoom, MaxZoom);
			}
			else if (@event is InputEventMouseMotion eventMotion)
				if ((eventMotion.ButtonMask & MouseButtonMask.Middle) == MouseButtonMask.Middle)
				{
					GlobalPosition -= eventMotion.Relative / Zoom;
					if (BoundToMap)
					if (RPG_Data.CurrentMap != null)
						GlobalPosition = new(
							Mathf.Clamp(GlobalPosition.X, 0, RPG_Data.CurrentMap.Size.X * TilesetHelper.TILE_WIDTH),
							Mathf.Clamp(GlobalPosition.Y, 0, RPG_Data.CurrentMap.Size.Y * TilesetHelper.TILE_HEIGHT)
						);
					else
						GlobalPosition = Vector2.Zero;
				}
		};
	}

	public override void _Process(double delta)
	{
		Offset = -(CameraControlZone.GlobalPosition - new Vector2(window.Size.X - CameraControlZone.GlobalPosition.X - CameraControlZone.Size.X, 0)) / Zoom * 0.5f;
		float res = (float)Mathf.Lerp(Zoom.X, TargetZoom, 0.5);
		Zoom = new(res, res);
	}
}
