using Godot;
using System;

public partial class LayersToggler : Button
{
	[Export]
	public CanvasItem Layer;
	//[Export]
	//public Color ToggleColor;
	public override void _Ready()
	{
		Toggled += (Toggled) =>
		{
			Layer.Visible = Toggled;// ? Colors.White : ToggleColor;
		};
	}
}
