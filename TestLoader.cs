using Godot;
using RPG;

public partial class TestLoader : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		RPG_Loader.LoadProject("D:/Programs/Steam/steamapps/common/OneShot - Copy/");
	}
}
