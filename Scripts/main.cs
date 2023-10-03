using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class main : Node2D
{
	TileMap tileMap;
	TileMap doorMap;
	Node2D player;



	[Export]
	public int minimumRooms = 10;
	[Export]
	Rect2 borders = new(0, 0, 100, 40);
	[Export]
	Vector2I startingPos = new(50, 20);
	[Export]
	float steps = 20;
	HashSet<Vector2I> map;

	List<Hall> halls = new();

	HashSet<Vector2I> hallTiles = new HashSet<Vector2I>();

	List<Room> rooms = new();

	public override void _Ready()
	{
		GD.Print("Start");
		player = GetNode<Node2D>("Player");

		tileMap = GetNode<TileMap>("Map");
		doorMap = GetNode<TileMap>("MapAdd");

		LevelGenerator levelGen = new(startingPos, borders, LevelType.Forest1);

		map = levelGen.GetMap(RoomType.Normal);
		Godot.Collections.Array<Vector2I> mapArray = new Godot.Collections.Array<Vector2I>(map);
		tileMap.SetCellsTerrainConnect(0, mapArray, 0, 0);


		map = levelGen.GetMap(RoomType.Starting);
		Godot.Collections.Array<Vector2I> mapArray2 = new Godot.Collections.Array<Vector2I>(map);
		tileMap.SetCellsTerrainConnect(2, mapArray2, 0, 0);


		// Walker walker = new Walker(startingPos, borders);
		// RoomWalker walker = new RoomWalker(startingPos, borders);
		// genMap(walker);
		// GD.Print("Found this many rooms: ", rooms.Count);
		// GD.Print("Found this many halls: ", halls.Count);
		player.Position = startingPos * tileMap.TileSet.TileSize;
	}

	public override void _Process(double delta)
	{
		Zoom();

	}

	private void Zoom()
	{
		if (Input.IsActionJustReleased("wheelup"))
		{
			GetNode<Node2D>("Player").GetNode<Camera2D>("Camera").Zoom += new Vector2(0.25f, 0.25f);
		}
		if (Input.IsActionJustReleased("wheeldown"))
		{
			GetNode<Node2D>("Player").GetNode<Camera2D>("Camera").Zoom += new Vector2(-0.25f, -0.25f);
		}
	}
	public void RegenLevel()
	{
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

	// public void genMap(RoomWalker walker)
	// {
	// map = new HashSet<Vector2I>(walker.Walk(steps));

	// FindMapData();

	// Godot.Collections.Array<Vector2I> mapArray = new Godot.Collections.Array<Vector2I>(map);
	// tileMap.SetCellsTerrainConnect(0, mapArray, 0, 0);
	// SetRoomTypes();
	// foreach (Room room in rooms)
	// {
	// 	int layer = 0;
	// 	if (room.roomType == RoomType.Starting) layer = 2;
	// 	if (room.roomType == RoomType.Boss) layer = 3;
	// 	Godot.Collections.Array<Vector2I> roomArray = new Godot.Collections.Array<Vector2I>(room.tiles);
	// 	tileMap.SetCellsTerrainConnect(layer, roomArray, 0, 0);

	// }
	// foreach (Hall hall in halls)
	// {
	// 	Godot.Collections.Array<Vector2I> hallArray = new Godot.Collections.Array<Vector2I>(hall.tiles);
	// 	tileMap.SetCellsTerrainConnect(1, hallArray, 0, 0);
	// 	if (hall.connectingDoor1 != null) doorMap.SetCell(0, hall.connectingDoor1.coords, 0, ConvertDoorDirection(hall.connectingDoor1));
	// 	if (hall.connectingDoor2 != null) doorMap.SetCell(0, hall.connectingDoor2.coords, 0, ConvertDoorDirection(hall.connectingDoor2));
	// }

	// }
	// private Vector2I ConvertDoorDirection(Door door)
	// {

	// 	switch (door.direction)
	// 	{
	// 		case DoorDirection.Up:
	// 			return new Vector2I(2, 0);
	// 		case DoorDirection.Down:
	// 			return new Vector2I(3, 0);
	// 		case DoorDirection.Left:
	// 			return new Vector2I(0, 0);
	// 		case DoorDirection.Right:
	// 			return new Vector2I(1, 0);
	// 		case DoorDirection.UpDown:
	// 			return new Vector2I(4, 0);
	// 		case DoorDirection.LeftRight:
	// 			return new Vector2I(5, 0);
	// 		default:
	// 			return new Vector2I(0, 0);
	// 	}
	// }
	// private void SetRoomTypes()
	// {
	// 	//Starting Room is already set

	// 	FindFarthestRoom(startingPos, rooms).roomType = RoomType.Boss;

	// }

	// public Room FindFarthestRoom(Vector2 startingPoint, List<Room> roomList)
	// {
	// 	// Vector2 farthestPoint = startingPoint;
	// 	float maxDistance = float.MinValue;
	// 	Room farthestRoom = roomList[0];

	// 	foreach (Room room in roomList)
	// 	{
	// 		foreach (Vector2I point in room.tiles)
	// 		{
	// 			float distance = startingPoint.DistanceTo(point);

	// 			if (distance > maxDistance)
	// 			{
	// 				maxDistance = distance;
	// 				// farthestPoint = point;
	// 				farthestRoom = room;
	// 			}
	// 		}
	// 	}


	// 	return farthestRoom;
	// }



	// private void FindMapData()
	// {

	// 	HashSet<Vector2I> visited = new HashSet<Vector2I>();
	// 	HashSet<Vector2I> visitedHalls = new HashSet<Vector2I>();
	// 	IterateCells(map);


	// 	foreach (Vector2I tile in map)
	// 	{
	// 		if (!visited.Contains(tile))
	// 		{
	// 			Room workingRoom = new Room
	// 			{
	// 				id = GenRoomId()
	// 			};
	// 			if (rooms.Count == 0)
	// 			{
	// 				workingRoom.roomType = RoomType.Starting;
	// 			}


	// 			CrawlNeighbors(tile, workingRoom.tiles, visited, map);

	// 			rooms.Add(workingRoom);

	// 		}
	// 	}


	// 	if (rooms.Count < minimumRooms)
	// 	{
	// 		GD.Print("NOT ENOUGH ROOMS", rooms.Count);
	// 		RegenLevel();
	// 	}

	// 	IterateCells(hallTiles);

	// 	foreach (Vector2I tile in hallTiles)
	// 	{
	// 		if (!visitedHalls.Contains(tile))
	// 		{
	// 			Hall workingHall = new Hall
	// 			{
	// 				id = GenHallId()
	// 			};



	// 			CrawlNeighbors(tile, workingHall.tiles, visitedHalls, hallTiles);

	// 			halls.Add(workingHall);

	// 		}
	// 	}

	// 	foreach (Hall hall in halls)
	// 	{
	// 		SetHallNeighbors(hall);
	// 	}

	// 	CleanHalls();

	// }

	// private void CleanHalls()
	// {
	// 	//This method runs through all the halls to remove redundant halls
	// 	//any halls with the same connecting rooms are removed
	// 	//any deadend halls are also removed

	// 	List<Hall> hallsToRemove = new List<Hall>();
	// 	List<Vector2I> connectionIDS = new List<Vector2I>();

	// 	foreach (Hall hall in halls)
	// 	{
	// 		if (hall.connectingRoom1 == null || hall.connectingRoom2 == null || hall.connectingRoom1 == hall.connectingRoom2)
	// 		{
	// 			hallsToRemove.Add(hall);
	// 		}
	// 		else
	// 		{
	// 			Vector2I connectionID = new Vector2I(hall.connectingRoom1.id, hall.connectingRoom2.id);
	// 			if (connectionIDS.Contains(connectionID))
	// 			{
	// 				hallsToRemove.Add(hall);
	// 			}
	// 			else
	// 			{
	// 				connectionIDS.Add(connectionID);
	// 			}
	// 		}

	// 	}

	// 	if (hallsToRemove.Count > 0)
	// 	{
	// 		GD.Print("Need to remove this many dead-beat halls: ", hallsToRemove.Count);
	// 		foreach (Hall hall in hallsToRemove)
	// 		{
	// 			halls.Remove(hall);

	// 		}

	// 	}



	// }

	// private void SetHallNeighbors(Hall hall)
	// {
	// 	//This Method crawls the hallway and finds the endpoints
	// 	//side1 will always be either the farthest left or farthest up, vise versa for side2
	// 	//side1/2 are then repurpoused by adding one additional offset of each to get a coord outside the bounds of the hallway
	// 	//these coords are then used to identify which rooms the hall connects to.
	// 	Vector2I side1 = hall.tiles.First();
	// 	Vector2I side2 = hall.tiles.First();
	// 	Door door1 = new Door
	// 	{
	// 		id = 0, //fix this
	// 		parentHall = hall,
	// 		coords = side1,
	// 	};
	// 	if (hall.tiles.Count == 1)
	// 	{
	// 		foreach (Room room in rooms)
	// 		{
	// 			if (room.tiles.Contains(side1 + Vector2I.Up) || room.tiles.Contains(side1 + Vector2I.Left))
	// 			{
	// 				if (room.tiles.Contains(side1 + Vector2I.Up))
	// 				{
	// 					door1.direction = DoorDirection.UpDown;
	// 				}
	// 				else
	// 				{
	// 					door1.direction = DoorDirection.LeftRight;
	// 				}
	// 				hall.connectingRoom1 = room;
	// 			}
	// 			if (room.tiles.Contains(side2 + Vector2I.Down) || room.tiles.Contains(side2 + Vector2I.Right))
	// 			{
	// 				hall.connectingRoom2 = room;
	// 			}
	// 			hall.connectingDoor1 = door1;

	// 			if (hall.connectingRoom1 != null && hall.connectingRoom2 != null) break;

	// 		}
	// 	}
	// 	else
	// 	{
	// 		foreach (Vector2I tile in hall.tiles)
	// 		{
	// 			if (tile.X < side1.X)
	// 			{
	// 				side1.X = tile.X;
	// 			}
	// 			if (tile.Y < side1.Y)
	// 			{
	// 				side1.Y = tile.Y;
	// 			}

	// 			if (tile.X > side2.X)
	// 			{
	// 				side2.X = tile.X;
	// 			}
	// 			if (tile.Y > side2.Y)
	// 			{
	// 				side2.Y = tile.Y;
	// 			}
	// 		}

	// 		bool isHorizontalHall = side1.X != side2.X;

	// 		door1.coords = side1;
	// 		Door door2 = new Door
	// 		{
	// 			id = 0, //fix this
	// 			parentHall = hall,
	// 			coords = side2,
	// 		};

	// 		if (isHorizontalHall)
	// 		{
	// 			door1.direction = DoorDirection.Left;
	// 			door2.direction = DoorDirection.Right;
	// 			side1 += Vector2I.Left;
	// 			side2 += Vector2I.Right;
	// 		}
	// 		else
	// 		{
	// 			door1.direction = DoorDirection.Up;
	// 			door2.direction = DoorDirection.Down;
	// 			side1 += Vector2I.Up;
	// 			side2 += Vector2I.Down;

	// 		}
	// 		hall.connectingDoor1 = door1;
	// 		hall.connectingDoor2 = door2;

	// 		foreach (Room room in rooms)
	// 		{
	// 			if (room.tiles.Contains(side1))
	// 			{
	// 				hall.connectingRoom1 = room;
	// 			}
	// 			if (room.tiles.Contains(side2))
	// 			{
	// 				hall.connectingRoom2 = room;
	// 			}
	// 		}
	// 	}
	// }

	// private int GenRoomId()
	// {
	// 	if (rooms.Count == 0) return 0;
	// 	int highestID = 0;
	// 	foreach (Room room in rooms)
	// 	{
	// 		if (room.id > highestID)
	// 		{
	// 			highestID = room.id;
	// 		}

	// 	}
	// 	return highestID + 1;
	// }
	// private int GenHallId()
	// {
	// 	if (halls.Count == 0) return 0;
	// 	int highestID = 0;
	// 	foreach (Hall hall in halls)
	// 	{
	// 		if (hall.id > highestID)
	// 		{
	// 			highestID = hall.id;
	// 		}

	// 	}
	// 	return highestID + 1;
	// }




	// private void IterateCells(HashSet<Vector2I> set)
	// {


	// 	foreach (Vector2I tile in set)
	// 	{
	// 		bool isEmptyUp = false;
	// 		bool isEmptyDown = false;
	// 		bool isEmptyLeft = false;
	// 		bool isEmptyRight = false;

	// 		if (!set.Contains(tile + Vector2I.Up)) isEmptyUp = true;

	// 		if (!set.Contains(tile + Vector2I.Down)) isEmptyDown = true;

	// 		if (!set.Contains(tile + Vector2I.Right)) isEmptyRight = true;

	// 		if (!set.Contains(tile + Vector2I.Left)) isEmptyLeft = true;


	// 		if (isEmptyUp && isEmptyDown || isEmptyLeft && isEmptyRight)
	// 		{
	// 			map.Remove(tile);
	// 			hallTiles.Add(tile);
	// 		}



	// 	}
	// }

	// private void CrawlNeighbors(Vector2I tile, HashSet<Vector2I> tiles, HashSet<Vector2I> visited, HashSet<Vector2I> set)
	// {
	// 	visited.Add(tile);
	// 	tiles.Add(tile);

	// 	// Define the possible neighboring tiles (e.g., up, down, left, right)
	// 	Vector2I[] neighbors = new Vector2I[]
	// 	{
	// 		tile + Vector2I.Up,
	// 		tile + Vector2I.Down,
	// 		tile + Vector2I.Left,
	// 		tile + Vector2I.Right
	// 	};

	// 	foreach (Vector2I neighbor in neighbors)
	// 	{
	// 		if (set.Contains(neighbor) && !visited.Contains(neighbor))
	// 		{
	// 			CrawlNeighbors(neighbor, tiles, visited, set);

	// 		}
	// 	}
	// }






	/***************************************************************************************************************************************/


	// 	TileMap tileMap;
	// TileMap doorMap;
	// Node2D player;



	// [Export]
	// public int minimumRooms = 10;
	// [Export]
	// Rect2 borders = new Rect2(0, 0, 100, 40);
	// [Export]
	// Vector2I startingPos = new Vector2I(50, 20);
	// [Export]
	// float steps = 200;
	// HashSet<Vector2I> map;

	// List<Hall> halls = new List<Hall>();

	// HashSet<Vector2I> hallTiles = new HashSet<Vector2I>();

	// List<Room> rooms = new List<Room>();

	// public override void _Ready()
	// {

	// 	player = GetNode<Node2D>("Player");

	// 	tileMap = GetNode<TileMap>("Map");
	// 	doorMap = GetNode<TileMap>("MapAdd");

	// 	Walker walker = new Walker(startingPos, borders);
	// 	genMap(walker);
	// 	GD.Print("Found this many rooms: ", rooms.Count);
	// 	GD.Print("Found this many halls: ", halls.Count);
	// 	player.Position = startingPos * tileMap.TileSet.TileSize;
	// }

	// public void genMap(Walker walker)
	// {
	// 	map = new HashSet<Vector2I>(walker.Walk(steps));
	// 	FindMapData();

	// 	// Godot.Collections.Array<Vector2I> mapArray = new Godot.Collections.Array<Vector2I>(map);
	// 	// tileMap.SetCellsTerrainConnect(0, mapArray, 0, 0);
	// 	SetRoomTypes();
	// 	foreach (Room room in rooms)
	// 	{
	// 		int layer = 0;
	// 		if (room.roomType == RoomType.Starting) layer = 2;
	// 		if (room.roomType == RoomType.Boss) layer = 3;
	// 		Godot.Collections.Array<Vector2I> roomArray = new Godot.Collections.Array<Vector2I>(room.tiles);
	// 		tileMap.SetCellsTerrainConnect(layer, roomArray, 0, 0);

	// 	}
	// 	foreach (Hall hall in halls)
	// 	{
	// 		Godot.Collections.Array<Vector2I> hallArray = new Godot.Collections.Array<Vector2I>(hall.tiles);
	// 		tileMap.SetCellsTerrainConnect(1, hallArray, 0, 0);
	// 		if (hall.connectingDoor1 != null) doorMap.SetCell(0, hall.connectingDoor1.coords, 0, ConvertDoorDirection(hall.connectingDoor1));
	// 		if (hall.connectingDoor2 != null) doorMap.SetCell(0, hall.connectingDoor2.coords, 0, ConvertDoorDirection(hall.connectingDoor2));
	// 	}

	// }
	// private Vector2I ConvertDoorDirection(Door door)
	// {

	// 	switch (door.direction)
	// 	{
	// 		case DoorDirection.Up:
	// 			return new Vector2I(2, 0);
	// 		case DoorDirection.Down:
	// 			return new Vector2I(3, 0);
	// 		case DoorDirection.Left:
	// 			return new Vector2I(0, 0);
	// 		case DoorDirection.Right:
	// 			return new Vector2I(1, 0);
	// 		case DoorDirection.UpDown:
	// 			return new Vector2I(4, 0);
	// 		case DoorDirection.LeftRight:
	// 			return new Vector2I(5, 0);
	// 		default:
	// 			return new Vector2I(0, 0);
	// 	}
	// }
	// private void SetRoomTypes()
	// {
	// 	//Starting Room is already set

	// 	FindFarthestRoom(startingPos, rooms).roomType = RoomType.Boss;

	// }

	// public Room FindFarthestRoom(Vector2 startingPoint, List<Room> roomList)
	// {
	// 	// Vector2 farthestPoint = startingPoint;
	// 	float maxDistance = float.MinValue;
	// 	Room farthestRoom = roomList[0];

	// 	foreach (Room room in roomList)
	// 	{
	// 		foreach (Vector2I point in room.tiles)
	// 		{
	// 			float distance = startingPoint.DistanceTo(point);

	// 			if (distance > maxDistance)
	// 			{
	// 				maxDistance = distance;
	// 				// farthestPoint = point;
	// 				farthestRoom = room;
	// 			}
	// 		}
	// 	}


	// 	return farthestRoom;
	// }

	// public void RegenLevel()
	// {
	// 	GetTree().ReloadCurrentScene();
	// }


	// private void FindMapData()
	// {

	// 	HashSet<Vector2I> visited = new HashSet<Vector2I>();
	// 	HashSet<Vector2I> visitedHalls = new HashSet<Vector2I>();
	// 	IterateCells(map);


	// 	foreach (Vector2I tile in map)
	// 	{
	// 		if (!visited.Contains(tile))
	// 		{
	// 			Room workingRoom = new Room
	// 			{
	// 				id = GenRoomId()
	// 			};
	// 			if (rooms.Count == 0)
	// 			{
	// 				workingRoom.roomType = RoomType.Starting;
	// 			}


	// 			CrawlNeighbors(tile, workingRoom.tiles, visited, map);

	// 			rooms.Add(workingRoom);

	// 		}
	// 	}


	// 	if (rooms.Count < minimumRooms)
	// 	{
	// 		GD.Print("NOT ENOUGH ROOMS", rooms.Count);
	// 		RegenLevel();
	// 	}

	// 	IterateCells(hallTiles);

	// 	foreach (Vector2I tile in hallTiles)
	// 	{
	// 		if (!visitedHalls.Contains(tile))
	// 		{
	// 			Hall workingHall = new Hall
	// 			{
	// 				id = GenHallId()
	// 			};



	// 			CrawlNeighbors(tile, workingHall.tiles, visitedHalls, hallTiles);

	// 			halls.Add(workingHall);

	// 		}
	// 	}

	// 	foreach (Hall hall in halls)
	// 	{
	// 		SetHallNeighbors(hall);
	// 	}

	// 	CleanHalls();

	// }

	// private void CleanHalls()
	// {
	// 	//This method runs through all the halls to remove redundant halls
	// 	//any halls with the same connecting rooms are removed
	// 	//any deadend halls are also removed

	// 	List<Hall> hallsToRemove = new List<Hall>();
	// 	List<Vector2I> connectionIDS = new List<Vector2I>();

	// 	foreach (Hall hall in halls)
	// 	{
	// 		if (hall.connectingRoom1 == null || hall.connectingRoom2 == null || hall.connectingRoom1 == hall.connectingRoom2)
	// 		{
	// 			hallsToRemove.Add(hall);
	// 		}
	// 		else
	// 		{
	// 			Vector2I connectionID = new Vector2I(hall.connectingRoom1.id, hall.connectingRoom2.id);
	// 			if (connectionIDS.Contains(connectionID))
	// 			{
	// 				hallsToRemove.Add(hall);
	// 			}
	// 			else
	// 			{
	// 				connectionIDS.Add(connectionID);
	// 			}
	// 		}

	// 	}

	// 	if (hallsToRemove.Count > 0)
	// 	{
	// 		GD.Print("Need to remove this many dead-beat halls: ", hallsToRemove.Count);
	// 		foreach (Hall hall in hallsToRemove)
	// 		{
	// 			halls.Remove(hall);

	// 		}

	// 	}



	// }

	// private void SetHallNeighbors(Hall hall)
	// {
	// 	//This Method crawls the hallway and finds the endpoints
	// 	//side1 will always be either the farthest left or farthest up, vise versa for side2
	// 	//side1/2 are then repurpoused by adding one additional offset of each to get a coord outside the bounds of the hallway
	// 	//these coords are then used to identify which rooms the hall connects to.
	// 	Vector2I side1 = hall.tiles.First();
	// 	Vector2I side2 = hall.tiles.First();
	// 	Door door1 = new Door
	// 	{
	// 		id = 0, //fix this
	// 		parentHall = hall,
	// 		coords = side1,
	// 	};
	// 	if (hall.tiles.Count == 1)
	// 	{
	// 		foreach (Room room in rooms)
	// 		{
	// 			if (room.tiles.Contains(side1 + Vector2I.Up) || room.tiles.Contains(side1 + Vector2I.Left))
	// 			{
	// 				if (room.tiles.Contains(side1 + Vector2I.Up))
	// 				{
	// 					door1.direction = DoorDirection.UpDown;
	// 				}
	// 				else
	// 				{
	// 					door1.direction = DoorDirection.LeftRight;
	// 				}
	// 				hall.connectingRoom1 = room;
	// 			}
	// 			if (room.tiles.Contains(side2 + Vector2I.Down) || room.tiles.Contains(side2 + Vector2I.Right))
	// 			{
	// 				hall.connectingRoom2 = room;
	// 			}
	// 			hall.connectingDoor1 = door1;

	// 			if (hall.connectingRoom1 != null && hall.connectingRoom2 != null) break;

	// 		}
	// 	}
	// 	else
	// 	{
	// 		foreach (Vector2I tile in hall.tiles)
	// 		{
	// 			if (tile.X < side1.X)
	// 			{
	// 				side1.X = tile.X;
	// 			}
	// 			if (tile.Y < side1.Y)
	// 			{
	// 				side1.Y = tile.Y;
	// 			}

	// 			if (tile.X > side2.X)
	// 			{
	// 				side2.X = tile.X;
	// 			}
	// 			if (tile.Y > side2.Y)
	// 			{
	// 				side2.Y = tile.Y;
	// 			}
	// 		}

	// 		bool isHorizontalHall = side1.X != side2.X;

	// 		door1.coords = side1;
	// 		Door door2 = new Door
	// 		{
	// 			id = 0, //fix this
	// 			parentHall = hall,
	// 			coords = side2,
	// 		};

	// 		if (isHorizontalHall)
	// 		{
	// 			door1.direction = DoorDirection.Left;
	// 			door2.direction = DoorDirection.Right;
	// 			side1 += Vector2I.Left;
	// 			side2 += Vector2I.Right;
	// 		}
	// 		else
	// 		{
	// 			door1.direction = DoorDirection.Up;
	// 			door2.direction = DoorDirection.Down;
	// 			side1 += Vector2I.Up;
	// 			side2 += Vector2I.Down;

	// 		}
	// 		hall.connectingDoor1 = door1;
	// 		hall.connectingDoor2 = door2;

	// 		foreach (Room room in rooms)
	// 		{
	// 			if (room.tiles.Contains(side1))
	// 			{
	// 				hall.connectingRoom1 = room;
	// 			}
	// 			if (room.tiles.Contains(side2))
	// 			{
	// 				hall.connectingRoom2 = room;
	// 			}
	// 		}
	// 	}
	// }

	// private int GenRoomId()
	// {
	// 	if (rooms.Count == 0) return 0;
	// 	int highestID = 0;
	// 	foreach (Room room in rooms)
	// 	{
	// 		if (room.id > highestID)
	// 		{
	// 			highestID = room.id;
	// 		}

	// 	}
	// 	return highestID + 1;
	// }
	// private int GenHallId()
	// {
	// 	if (halls.Count == 0) return 0;
	// 	int highestID = 0;
	// 	foreach (Hall hall in halls)
	// 	{
	// 		if (hall.id > highestID)
	// 		{
	// 			highestID = hall.id;
	// 		}

	// 	}
	// 	return highestID + 1;
	// }




	// private void IterateCells(HashSet<Vector2I> set)
	// {


	// 	foreach (Vector2I tile in set)
	// 	{
	// 		bool isEmptyUp = false;
	// 		bool isEmptyDown = false;
	// 		bool isEmptyLeft = false;
	// 		bool isEmptyRight = false;

	// 		if (!set.Contains(tile + Vector2I.Up)) isEmptyUp = true;

	// 		if (!set.Contains(tile + Vector2I.Down)) isEmptyDown = true;

	// 		if (!set.Contains(tile + Vector2I.Right)) isEmptyRight = true;

	// 		if (!set.Contains(tile + Vector2I.Left)) isEmptyLeft = true;


	// 		if (isEmptyUp && isEmptyDown || isEmptyLeft && isEmptyRight)
	// 		{
	// 			map.Remove(tile);
	// 			hallTiles.Add(tile);
	// 		}



	// 	}
	// }

	// private void CrawlNeighbors(Vector2I tile, HashSet<Vector2I> tiles, HashSet<Vector2I> visited, HashSet<Vector2I> set)
	// {
	// 	visited.Add(tile);
	// 	tiles.Add(tile);

	// 	// Define the possible neighboring tiles (e.g., up, down, left, right)
	// 	Vector2I[] neighbors = new Vector2I[]
	// 	{
	// 		tile + Vector2I.Up,
	// 		tile + Vector2I.Down,
	// 		tile + Vector2I.Left,
	// 		tile + Vector2I.Right
	// 	};

	// 	foreach (Vector2I neighbor in neighbors)
	// 	{
	// 		if (set.Contains(neighbor) && !visited.Contains(neighbor))
	// 		{
	// 			CrawlNeighbors(neighbor, tiles, visited, set);

	// 		}
	// 	}
	// }


	// public override void _Input(InputEvent @event)
	// {
	// 	base._Input(@event);

	// 	if (@event.IsActionPressed("ui_accept"))
	// 	{
	// 		RegenLevel();
	// 	}
	// }
}
