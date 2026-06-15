using Godot;
using RPG;

public partial class MapEnities : Node2D
{
	[Export]
	public PackedScene EntityTemplate;
	public override void _Ready()
	{
		RPG_Loader.MapLoaded += () =>
		{
			Clear();
			foreach ((int key, Event e) in RPG_Data.CurrentMap.Events)
			{
				Entity entity = EntityTemplate.Instantiate<Entity>();
				entity.EventID = key;
				AddChild(entity);
			}
		};
	}

	public void Clear()
	{
		foreach (Node Child in GetChildren())
			Child.QueueFree();
	}
}
