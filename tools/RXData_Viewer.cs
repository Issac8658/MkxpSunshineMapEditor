using Godot;
using RPG;

public partial class RXData_Viewer : Window
{
	[Export]
	public LineEdit PathInputLine;
	[Export]
	public Button ParseButton;
	[Export]
	public CodeEdit ResultPanel;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ParseButton.Pressed += () =>
		{
			string filePath = PathInputLine.Text;
			if (FileAccess.FileExists(filePath))
			{
				Variant result = new RXData_Parser(filePath).Data;
				ResultPanel.Text = IndentParsedRXData(GD.VarToStr(result));
				//ResultPanel.SelectAll();
				//ResultPanel.IndentLines();
				//ResultPanel.Deselect();
			}
		};

		CloseRequested += Hide;
	}

	public static string IndentParsedRXData(string data)
	{
		data = data.Replace("}]", "}\n]");
		int layer = 0;

		for (int i = 0; i < data.Length; i++)
			if (data[i] == '{' || data[i] == '[')
				layer += 1;
			else if (data[i] == '}' || data[i] == ']')
			{
				layer -= 1;
				if (data[i - 1] == '\t')
					data = data.Remove(i - 1, 1);
			}
			else if (data[i] == '\n')
			{
				string insert = "";
				for (;insert.Length < layer; insert += '\t');
				data = data.Insert(i + 1, insert);
			}
		return data;
	}
}
