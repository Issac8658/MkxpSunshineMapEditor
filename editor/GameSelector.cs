using Godot;
using RPG;

public partial class GameSelector : Control
{
	[Export]
	public LineEdit PathInput;
	[Export]
	public Button SelectButton;
	[Export]
	public Button OpenButton;
	[Export]
	public FileDialog Dialog;
	[Export]
	public AcceptDialog ErrorDialog;
	[Export]
	public CanvasItem LoadingPanel;

	public override void _Ready()
	{
		SelectButton.Pressed += () =>
		{
			Dialog.PopupCenteredClamped();
		};

		Dialog.DirSelected += (path) =>
		{
			PathInput.Text = path;
		};

		OpenButton.Pressed += async () =>
		{
			LoadingPanel.Visible = true;
			Visible = false;
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			if (!RPG_Loader.LoadProject(PathInput.Text, out string errorMessage))
			{
				ErrorDialog.DialogText = errorMessage;
				ErrorDialog.PopupCentered();
				LoadingPanel.Visible = false;
				Visible = true;
			}
			else
			{
				GetTree().ChangeSceneToFile("res://main.tscn");
			}
		};
	}
}
