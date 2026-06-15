using Godot;

public partial class EntityHitboxesToggler : Button
{
	public override void _Ready()
	{
		Toggled += Entity.ToggleHitboxes;
	}
}
