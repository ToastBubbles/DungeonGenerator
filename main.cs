using Godot;
using System;
using System.Collections.Generic;

public partial class main : Node2D
{
	TileMap tileMap;
	[Export]
	Rect2 borders = new Rect2(0, 0, 100, 40);
	[Export]
	Vector2I startingPos = new Vector2I(50, 20);
	[Export]
	float steps = 200;
	public override void _Ready()
	{
		tileMap = GetNode<TileMap>("Map");

		Walker walker = new Walker(startingPos, borders);
		genMap(walker);


	}

	public void genMap(Walker walker)
	{
		List<Vector2I> map = walker.Walk(steps);

		Godot.Collections.Array<Vector2I> mapArray = new Godot.Collections.Array<Vector2I>(map);
		tileMap.SetCellsTerrainConnect(0, mapArray, 0, 0);
	}

	public void RegenLevel()
	{
		GetTree().ReloadCurrentScene();
	}


	public override void _Process(double delta)
	{

	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);

		if (@event.IsActionPressed("ui_accept"))
		{
			RegenLevel();
		}
	}



}
