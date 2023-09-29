using Godot;
using System;
using System.Collections.Generic;

public partial class main : Node2D
{
	TileMap tileMap;
	TileMap doorMap;

	[Export]
	public int minimumRooms = 6;
	[Export]
	Rect2 borders = new Rect2(0, 0, 100, 40);
	[Export]
	Vector2I startingPos = new Vector2I(50, 20);
	[Export]
	float steps = 200;
	HashSet<Vector2I> map;
	HashSet<Vector2I> halls = new HashSet<Vector2I>();

	List<Room> rooms = new List<Room>();

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
		GD.Print("Found this many rooms: ", rooms.Count);
	}

	public void genMap(Walker walker)
	{
		map = new HashSet<Vector2I>(walker.Walk(steps));
		FindMapData();

		// Godot.Collections.Array<Vector2I> mapArray = new Godot.Collections.Array<Vector2I>(map);
		// tileMap.SetCellsTerrainConnect(0, mapArray, 0, 0);
		SetRoomTypes();
		foreach (Room room in rooms)
		{
			int layer = 0;
			if (room.roomType == RoomType.Starting) layer = 2;
			if (room.roomType == RoomType.Boss) layer = 3;
			Godot.Collections.Array<Vector2I> roomArray = new Godot.Collections.Array<Vector2I>(room.tiles);
			tileMap.SetCellsTerrainConnect(layer, roomArray, 0, 0);

		}
		Godot.Collections.Array<Vector2I> hallArray = new Godot.Collections.Array<Vector2I>(halls);
		tileMap.SetCellsTerrainConnect(1, hallArray, 0, 0);
	}

	private void SetRoomTypes()
	{
		//Starting Room is already set

		FindFarthestRoom(startingPos, rooms).roomType = RoomType.Boss;




	}

	public Room FindFarthestRoom(Vector2 startingPoint, List<Room> roomList)
	{
		// Vector2 farthestPoint = startingPoint;
		float maxDistance = float.MinValue;
		Room farthestRoom = roomList[0];

		foreach (Room room in roomList)
		{
			foreach (Vector2I point in room.tiles)
			{
				float distance = startingPoint.DistanceTo(point);

				if (distance > maxDistance)
				{
					maxDistance = distance;
					// farthestPoint = point;
					farthestRoom = room;
				}
			}
		}


		return farthestRoom;
	}

	public void RegenLevel()
	{
		GetTree().ReloadCurrentScene();
	}


	private void FindMapData()
	{

		HashSet<Vector2I> visited = new HashSet<Vector2I>();
		IterateCells(map);


		foreach (Vector2I tile in map)
		{
			if (!visited.Contains(tile))
			{
				Room workingRoom = new Room();
				if (rooms.Count == 0)
				{
					workingRoom.roomType = RoomType.Starting;
				}


				CrawlRoomNeighbors(tile, workingRoom, visited);

				rooms.Add(workingRoom);

			}
		}
		if (rooms.Count < minimumRooms)
		{
			GD.Print("NOT ENOUGH ROOMS", rooms.Count);
			RegenLevel();
		}

		IterateCells(halls, true);


	}

	private int GenId()
	{
		if (rooms.Count == 0) return 0;
		int highestID = 0;
		foreach (Room room in rooms)
		{
			if (room.id > highestID)
			{
				highestID = room.id;
			}

		}
		return highestID + 1;
	}




	private void IterateCells(HashSet<Vector2I> set, bool placingDoors = false)
	{


		foreach (Vector2I tile in set)
		{
			bool isEmptyUp = false;
			bool isEmptyDown = false;
			bool isEmptyLeft = false;
			bool isEmptyRight = false;
			int connectingTiles = 4;
			if (!set.Contains(tile + Vector2I.Up))
			{
				isEmptyUp = true;
				connectingTiles--;
			}
			if (!set.Contains(tile + Vector2I.Down))
			{
				isEmptyDown = true;
				connectingTiles--;
			}
			if (!set.Contains(tile + Vector2I.Right))
			{
				isEmptyRight = true;
				connectingTiles--;
			}
			if (!set.Contains(tile + Vector2I.Left))
			{
				isEmptyLeft = true;
				connectingTiles--;
			}
			if (!placingDoors)
			{

				if (isEmptyUp && isEmptyDown || isEmptyLeft && isEmptyRight)
				{
					map.Remove(tile);
					halls.Add(tile);
				}

			}
			else
			{
				if (connectingTiles == 0)
				{
					if (!map.Contains(tile + Vector2I.Up))
					{
						doorMap.SetCell(0, tile, 0, doorLeftRight);
					}
					else
					{
						doorMap.SetCell(0, tile, 0, doorTopBottom);
					}
				}
				else if (connectingTiles == 1)
				{

					if (!isEmptyUp) doorMap.SetCell(0, tile, 0, doorBottom);
					if (!isEmptyDown) doorMap.SetCell(0, tile, 0, doorTop);
					if (!isEmptyLeft) doorMap.SetCell(0, tile, 0, doorRight);
					if (!isEmptyRight) doorMap.SetCell(0, tile, 0, doorLeft);

				}
			}
		}
	}


	private void CrawlRoomNeighbors(Vector2I tile, Room room, HashSet<Vector2I> visited)
	{
		visited.Add(tile);
		room.tiles.Add(tile);

		// Define the possible neighboring tiles (e.g., up, down, left, right)
		Vector2I[] neighbors = new Vector2I[]
		{
			tile + Vector2I.Up,
			tile + Vector2I.Down,
			tile + Vector2I.Left,
			tile + Vector2I.Right
		};

		foreach (Vector2I neighbor in neighbors)
		{
			if (map.Contains(neighbor) && !visited.Contains(neighbor))
			{
				CrawlRoomNeighbors(neighbor, room, visited);

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
