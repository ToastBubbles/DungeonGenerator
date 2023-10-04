using Godot;
using System;
using System.Collections.Generic;

public partial class line : Line2D
{
	public Vector2[] points { get; set; }
	Color color = new Color(0, 1, 0, 1);
	public override void _Ready()
	{
		main mainboi = GetParent().GetParent<main>();
		mainboi.DebugLine += SetPoints;


	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public override void _Draw()
	{

		if (points != null && points.Length > 0)
		{


			DrawPolyline(points, color, 10);
		}
		else
		{
			GD.Print("No points found");
		}

	}

	public void SetPoints(Vector2[] points)
	{
		this.points = points;
		QueueRedraw();
	}


}
