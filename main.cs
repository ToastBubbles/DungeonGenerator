using Godot;
using System;
using System.Collections.Generic;

public partial class main : Node2D
{
	TileMap tileMap;
	TileMap doorMap;
	[Export]
	Rect2 borders = new Rect2(0, 0, 100, 40);
	[Export]
	Vector2I startingPos = new Vector2I(50, 20);
	[Export]
	float steps = 200;
	HashSet<Vector2I> map;
	List<Vector2I> halls = new List<Vector2I>();

	Vector2I doorLeft = new Vector2I(0, 0);
	Vector2I doorRight = new Vector2I(1, 0);
	Vector2I doorTop = new Vector2I(2, 0);
	Vector2I doorBottom = new Vector2I(3, 0);

	Vector2I doorTopBottom = new Vector2I(4, 0);
	Vector2I doorLeftRight = new Vector2I(5, 0);
	public override void _Ready()
	{
		tileMap = GetNode<TileMap>("Map");
		doorMap = GetNode<TileMap>("MapAdd");

		Walker walker = new Walker(startingPos, borders);
		genMap(walker);


	}


	public void genMap(Walker walker)
	{
		map = new HashSet<Vector2I>(walker.Walk(steps));
		FindHallways();

		Godot.Collections.Array<Vector2I> mapArray = new Godot.Collections.Array<Vector2I>(map);
		tileMap.SetCellsTerrainConnect(0, mapArray, 0, 0);
		Godot.Collections.Array<Vector2I> hallArray = new Godot.Collections.Array<Vector2I>(halls);
		tileMap.SetCellsTerrainConnect(1, hallArray, 0, 0);
	}

	public void RegenLevel()
	{
		GetTree().ReloadCurrentScene();
	}


	public override void _Process(double delta)
	{

	}

	private void FindHallways()
	{
		foreach (Vector2I tile in map)
		{
			bool isEmptyUp = false;
			bool isEmptyDown = false;
			bool isEmptyLeft = false;
			bool isEmptyRight = false;

			if (!map.Contains(tile + Vector2I.Up))
			{
				isEmptyUp = true;
			}
			if (!map.Contains(tile + Vector2I.Down))
			{
				isEmptyDown = true;
			}
			if (!map.Contains(tile + Vector2I.Right))
			{
				isEmptyRight = true;
			}
			if (!map.Contains(tile + Vector2I.Left))
			{
				isEmptyLeft = true;
			}

			if (isEmptyUp && isEmptyDown || isEmptyLeft && isEmptyRight)
			{
				map.Remove(tile);
				halls.Add(tile);
			}

		}
		FindDoors();
	}
	private void FindDoors()
	{
		GD.Print("Starting Door Building");
		foreach (Vector2I hallTile in halls)
		{
			// GD.Print(hallTile);
			bool isEmptyUp = false;
			bool isEmptyDown = false;
			bool isEmptyLeft = false;
			bool isEmptyRight = false;
			int connectingTiles = 4;
			if (!halls.Contains(hallTile + Vector2I.Up))
			{
				isEmptyUp = true;
				connectingTiles--;
			}
			if (!halls.Contains(hallTile + Vector2I.Down))
			{
				isEmptyDown = true;
				connectingTiles--;
			}
			if (!halls.Contains(hallTile + Vector2I.Right))
			{
				isEmptyRight = true;
				connectingTiles--;
			}
			if (!halls.Contains(hallTile + Vector2I.Left))
			{
				isEmptyLeft = true;
				connectingTiles--;
			}
			if (connectingTiles == 0)
			{

				// map.Remove(hallTile);
				// halls.Add(hallTile);
				doorMap.SetCell(0, hallTile, 0, doorTopBottom);
			}
			else if (connectingTiles == 1)
			{
				GD.Print("placing door");
				if (!isEmptyUp) doorMap.SetCell(0, hallTile, 0, doorBottom);
				if (!isEmptyDown) doorMap.SetCell(0, hallTile, 0, doorTop);
				if (!isEmptyLeft) doorMap.SetCell(0, hallTile, 0, doorRight);
				if (!isEmptyRight) doorMap.SetCell(0, hallTile, 0, doorLeft);

			}
		}
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
