using Godot;
using System;

public partial class Debugger : Node2D
{
	private PackedScene labelPath = GD.Load<PackedScene>("res://label.tscn");
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
			AddChild(label);
			label.Position = new Vector2(dataSet.X, dataSet.Y);
			label.Text = $"({dataSet.X / 32},{dataSet.Y / 32}) ID:{dataSet.Z}";
			if (lastDataSet.Z != dataSet.Z)
			{
				Label lengthLabel = (Label)labelPath.Instantiate();
				AddChild(lengthLabel);
				Vector2 middlePos = new Vector2((dataSet.X + lastDataSet.X) / 2, (dataSet.Y + lastDataSet.Y) / 2);
				float distance = MathF.Round(CalculateDistance(new Vector2(dataSet.X, dataSet.Y), new Vector2(lastDataSet.X, lastDataSet.Y)) / 32, 2);
				lengthLabel.Position = middlePos;
				lengthLabel.Text = $"{distance}";

			}
			lastDataSet = dataSet;
		}
	}
	static float CalculateDistance(Vector2 point1, Vector2 point2)
	{
		float dx = point1.X - point2.X;
		float dy = point1.Y - point2.Y;
		float output = (float)Math.Sqrt(dx * dx + dy * dy);

		return output;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
