using DelaunatorSharp;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class main : Node2D
{
	[Signal]
	public delegate void DebugRoomsEventHandler(Vector3[] data);
	[Signal]
	public delegate void DebugTrisEventHandler(Vector2 p1, Vector2 p2, int index, int total, Vector2 strtrmcntr, Vector2 endRmCntr);
	TileMap tileMap;

	// TileMap hallTileMap;
	TileMap doorMap;
	Node2D player;

	readonly Random rand = new();

	public List<Room> rooms { get; set; } = new();

	Camera2D cam;
	[Export]
	public int minimumRooms = 10;

	[Export]
	Vector2I startingPos = new(0, 0);
	[Export]
	float steps = 20;
	HashSet<Vector2I> map;

	List<Hall> halls = new();

	HashSet<Vector2I> hallMap = new HashSet<Vector2I>();

	public override void _Ready()
	{
		//initial setters
		cam = GetNode<Node2D>("Player").GetNode<Camera2D>("Camera");
		player = GetNode<Node2D>("Player");
		tileMap = GetNode<TileMap>("Map");

		doorMap = GetNode<TileMap>("MapAdd");


		LevelGenerator levelGen = new(startingPos, LevelType.Forest1);

		rooms = new(levelGen.rooms);

		DrawMap(levelGen, RoomType.Normal, 1);
		DrawMap(levelGen, RoomType.Starting, 2);
		DrawMap(levelGen, RoomType.Boss, 3);



		SetDebugInfo(levelGen);

		SendTriData(levelGen);
		DrawHallMap(levelGen);
		player.Position = startingPos * tileMap.TileSet.TileSize;

		SetZoom();
	}



	//Draws map on tile map, this needs to be done multiple times for each type of room for the debug colors, later we can change this
	private void DrawMap(LevelGenerator levelGen, RoomType roomType, int layer)
	{
		map = levelGen.GetMap(roomType);
		Godot.Collections.Array<Vector2I> mapArray = new Godot.Collections.Array<Vector2I>(map);
		tileMap.SetCellsTerrainConnect(layer, mapArray, 0, 0);
	}

	//Draws halls
	private void DrawHallMap(LevelGenerator levelGen)
	{
		LineDraw lineDraw = GetNode<Node2D>("Debugger").GetNode<LineDraw>("LineDraw");
		hallMap = levelGen.GenHallways(lineDraw.GetHallEdges());

		Godot.Collections.Array<Vector2I> mapArray = new Godot.Collections.Array<Vector2I>(hallMap);
		tileMap.SetCellsTerrainConnect(0, mapArray, 0, 0);
	}


	//Sends info for debugger to print
	private void SetDebugInfo(LevelGenerator levelGen)
	{
		List<Vector2I> debugPoints = levelGen.debugPoints;
		List<Vector3I> debugRoomPoints = levelGen.debugRoomPoints;
		Vector2[] pointsArray = new Vector2[debugPoints.Count];
		Vector3[] debugRoomArray = new Vector3[debugRoomPoints.Count];
		for (int i = 0; i < debugPoints.Count; i++)
		{
			// Convert each Vector2I to Vector2
			pointsArray[i] = new Vector2(debugPoints[i].X, debugPoints[i].Y) * tileMap.TileSet.TileSize;
		}
		for (int i = 0; i < debugRoomPoints.Count; i++)
		{
			// Convert each Vector2I to Vector2
			debugRoomArray[i] = new Vector3(debugRoomPoints[i].X * tileMap.TileSet.TileSize[0], debugRoomPoints[i].Y * tileMap.TileSet.TileSize[0], debugRoomPoints[i].Z);
		}

		EmitSignal(SignalName.DebugRooms, debugRoomArray);
	}


	//triangulates positions and signals them for the line tool to draw
	private void SendTriData(LevelGenerator levelGen)
	{
		IEnumerable<IEdge> edges = levelGen.Delaunatate();
		Vector2I strtRmCntr = levelGen.rooms.FirstOrDefault(x => x.RoomType == RoomType.Starting).Center;
		Vector2I endRmCntr = levelGen.rooms.FirstOrDefault(x => x.RoomType == RoomType.Boss).Center;
		foreach (IEdge edge in edges)
		{
			EmitSignal(SignalName.DebugTris, new Vector2((float)edge.P.X, (float)edge.P.Y), new Vector2((float)edge.Q.X, (float)edge.Q.Y), edge.Index, edges.Count(), new Vector2(strtRmCntr.X, strtRmCntr.Y), new Vector2(endRmCntr.X, endRmCntr.Y));
		}
	}

	public override void _Process(double delta)
	{
		Zoom();
	}


	//Zoomout
	private void SetZoom()
	{
		cam.Zoom = new Vector2(0.25f, 0.25f);
	}


	//Control zoom with mousewheel
	private void Zoom()
	{
		Vector2 zoomLevel = new Vector2(0.25f, 0.25f);

		if (Input.IsActionJustReleased("wheelup"))
		{
			cam.Zoom += zoomLevel;
		}
		if (Input.IsActionJustReleased("wheeldown") && cam.Zoom > zoomLevel)
		{
			cam.Zoom -= zoomLevel;
		}
	}


	//Reload level/scene
	public void RegenLevel()
	{
		GD.Print("**REGEN LEVEL**");
		GetTree().ReloadCurrentScene();
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
