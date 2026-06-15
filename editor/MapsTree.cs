using Godot.Collections;
using Godot;
using RPG;

public partial class MapsTree : Tree
{
	private Dictionary<int, TreeItem> _treeItems = [];

	private TreeItem _root;

	public override void _Ready()
	{
		_root = CreateItem();
		_root.SetText(0, "Root");

		RPG_Loader.ProjectLoaded += () =>
		{
			_treeItems = [];

			foreach ((int id, MapInfo info) in RPG_Data.MapInfos)
			{
				CreateMapItem(id);
			}
		};

		ItemActivated += () =>
		{
			TreeItem selected = GetSelected();
			int id = GetId(selected);
			if (id != -1)
			{
				RPG_Loader.LoadMap(id);
			}
		};
		
		SetColumnExpand(1, false);
		SetColumnCustomMinimumWidth(1, 40);
		SetColumnTitle(0, "Name");
		SetColumnTitle(1, "ID");
		SetColumnTitleAlignment(0, HorizontalAlignment.Left);
	}

	private void CreateMapItem(int id)
	{
		if (id == 0 || _treeItems.ContainsKey(id))
			return;

		MapInfo info = RPG_Data.MapInfos[id];

		if (info != null)
		{
			TreeItem parent = _root;
			if (_treeItems.TryGetValue(info.ParentID, out TreeItem value))
				parent = value;
			else
			{
				CreateMapItem(info.ParentID);
				if (_treeItems.TryGetValue(info.ParentID, out TreeItem value2))
					parent = value2;
			}

			TreeItem item = CreateItem(parent);
			if (info.ParentID == 0) 
				item.Collapsed = true;
			item.SetText(0, info.Name);
			item.SetText(1, id.ToString("D3"));
			_treeItems.Add(id, item);
		}
	}

	private int GetId(TreeItem item)
	{
		foreach ((int id, TreeItem f) in _treeItems)
		{
			if (f == item)
			{
				return id;
			}
		}
		return -1;
	}
}
