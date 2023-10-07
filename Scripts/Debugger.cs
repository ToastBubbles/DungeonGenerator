using Godot;
using System;

public partial class Debugger : Node2D
{
	private PackedScene labelPath = GD.Load<PackedScene>("res://label.tscn");
	float scale = 10;
	public override void _Ready()
	{
		main mainboi = GetParent<main>();
		mainboi.DebugRooms += SetData;

	}

	private void SetData(Vector3[] data)
	{

		Vector3 lastDataSet = data[0];
		foreach (Vector3 dataSet in data)
		{
			Label label = (Label)labelPath.Instantiate();
			GetNode<Node2D>("Labels").AddChild(label);
			label.Position = new Vector2(dataSet.X, dataSet.Y);
			label.Text = $"({dataSet.X / 32},{dataSet.Y / 32}) ID:{dataSet.Z}";
			label.ZIndex = 10;
			if (lastDataSet.Z != dataSet.Z)
			{
				// Label lengthLabel = (Label)labelPath.Instantiate();
				// GetNode<Node2D>("Labels").AddChild(lengthLabel);
				// Vector2 middlePos = new Vector2((dataSet.X + lastDataSet.X) / 2, (dataSet.Y + lastDataSet.Y) / 2);
				// float distance = MathF.Round(CalculateDistance(new Vector2(dataSet.X, dataSet.Y), new Vector2(lastDataSet.X, lastDataSet.Y)) / 32, 2);
				// lengthLabel.Position = middlePos;
				// lengthLabel.Text = $"{distance}";
				// lengthLabel.Modulate = new Color(0, 0, 0, 1);
				// lengthLabel.ZIndex = 10;

			}
			lastDataSet = dataSet;
		}
	}

	public override void _Process(double delta)
	{
		scale = (float)Mathf.Clamp(3 - GetParent().GetNode<Node2D>("Player").GetNode<Camera2D>("Camera").Zoom.X, 0.5, 3);
		Godot.Collections.Array<Godot.Node> children = GetNode<Node2D>("Labels").GetChildren();

		foreach (Label child in children)
		{
			child.Scale = new Vector2(scale, scale);
		}
	}
}
