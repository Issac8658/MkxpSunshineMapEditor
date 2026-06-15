using System;
using System.IO;
using Godot;
using RPG;

public partial class Entity : Sprite2D
{
	public static event BoolEventHandler ToggleHitboxesEvent;
	public static void ToggleHitboxes(bool toggle)
	{
		_hitboxesEnabled = toggle;
		ToggleHitboxesEvent?.Invoke(toggle);
	}
	private static bool _hitboxesEnabled = true;

	private int _eventID = 0;
	private Event _eventData { get => RPG_Data.CurrentMap.Events[_eventID]; }
	
	[Export]
	public Panel SelectionRect;

	public int EventID
	{
		get => _eventID;
		set
		{
			_eventID = value;
			Update();
		}
	}

	public override void _Ready()
	{
		ToggleHitboxesEvent += UpdateHitboxVisibility;
		SelectionRect.Visible = _hitboxesEnabled;

		TreeExiting += () => ToggleHitboxesEvent -= UpdateHitboxVisibility;
	}

	private void UpdateHitboxVisibility(bool toggled)
	{
		SelectionRect.Visible = toggled;
	}

	private void Update()
	{
		Texture2D texture = RPG_Loader.GetSprite(Path.Combine(RPG_Loader.CHARACTERS_FOLDER, _eventData.Pages[0].PageGraphic.CharacterName + ".png"));
		Texture = texture;

		UpdateSpriteRect();

		Name = _eventData.Name;

		if (_eventData.Pages[0].AlwaysOnTop)
			ZIndex = 900;

		switch (_eventData.Pages[0].PageGraphic.BlendType)
		{
			case 0:
				Material = null;
				break;
			case 1:
				Material = GD.Load<CanvasItemMaterial>("res://materials/blend_add.tres");
				break;
			case 2:
				Material = GD.Load<CanvasItemMaterial>("res://materials/blend_sub.tres");
				break;
		}
	}

	private void UpdateSpriteRect()
	{
		Position = _eventData.Position * TilesetHelper.TileSize;

		Rect2I rect;
		if (Texture == null)
		{
			Texture = RPG_Data.GetCurrentTilesetTexture();
			rect = new(TilesetHelper.GetCoordsFromID(_eventData.Pages[0].PageGraphic.TileID) * TilesetHelper.TileSize, TilesetHelper.TileSize);
		}
		else
			rect = GetCharacterSpriteRegionByState((Vector2I)Texture.GetSize(), _eventData.Pages[0].PageGraphic.Direction, _eventData.Pages[0].PageGraphic.Pattern);
		RegionRect = rect;
		Vector2 sizeOffset = new Vector2(0, -rect.Size.Y / 2) + ((Vector2)TilesetHelper.TileSize) * new Vector2(0.5f, 1);
		Position += sizeOffset;
		SelectionRect.Position = Vector2.Zero - sizeOffset + (TilesetHelper.TileSize - SelectionRect.Size) / 2;
	}

	public static Rect2I GetCharacterSpriteRegionByState(Vector2I imageSize, int direction, int animationFrame) =>
		new (animationFrame * imageSize.X / 4, (direction / 2 - 1) * imageSize.Y / 4, imageSize / 4);
}
