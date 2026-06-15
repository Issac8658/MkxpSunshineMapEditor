using Godot;

public partial class MenuBarItem_Tools : PopupMenu
{
	[Export]
	public Window RXDataView_Window;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		IndexPressed += (ix) => { if (ix == 0) RXDataView_Window.Show(); };
	}
}
