using Godot;
using System;
using System.Collections.Generic;

public partial class main : Node2D
{
	TileMap tileMap;
	[Export]
	Rect2 borders = new Rect2(0, 0, 15, 15);
	[Export]
	Vector2I startingPos = new Vector2I(7, 7);
	[Export]
	float steps = 300;
	public override void _Ready()
	{
		tileMap = GetNode<TileMap>("Map");
		
		Walker walker = new Walker(startingPos, borders);

		List<Vector2I> room = walker.Walk(steps);

		Godot.Collections.Array<Vector2I> roomArray = new Godot.Collections.Array<Vector2I>(room);
		tileMap.SetCellsTerrainConnect(0, roomArray, 0, 0);

	}


	public override void _Process(double delta)
	{
	}
}
