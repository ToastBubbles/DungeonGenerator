using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

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

	List<Hall> halls = new List<Hall>();

	HashSet<Vector2I> hallTiles = new HashSet<Vector2I>();

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
		GD.Print("Found this many halls: ", halls.Count);
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
		foreach (Hall hall in halls)
		{
			Godot.Collections.Array<Vector2I> hallArray = new Godot.Collections.Array<Vector2I>(hall.tiles);
			tileMap.SetCellsTerrainConnect(1, hallArray, 0, 0);
		}

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
		HashSet<Vector2I> visitedHalls = new HashSet<Vector2I>();
		IterateCells(map);


		foreach (Vector2I tile in map)
		{
			if (!visited.Contains(tile))
			{
				Room workingRoom = new Room
				{
					id = GenRoomId()
				};
				if (rooms.Count == 0)
				{
					workingRoom.roomType = RoomType.Starting;
				}


				CrawlNeighbors(tile, workingRoom.tiles, visited, map);

				rooms.Add(workingRoom);

			}
		}


		if (rooms.Count < minimumRooms)
		{
			GD.Print("NOT ENOUGH ROOMS", rooms.Count);
			RegenLevel();
		}

		IterateCells(hallTiles, true);

		foreach (Vector2I tile in hallTiles)
		{
			if (!visitedHalls.Contains(tile))
			{
				Hall workingHall = new Hall
				{
					id = GenHallId()
				};



				CrawlNeighbors(tile, workingHall.tiles, visitedHalls, hallTiles);

				halls.Add(workingHall);

			}
		}

		foreach (Hall hall in halls)
		{
			SetHallNeighbors(hall);
		}

		CleanHalls();

	}

	private void CleanHalls()
	{
		//This method runs through all the halls to remove redundant halls
		//any halls with the same connecting rooms are removed
		//any deadend halls are also removed

		List<Hall> hallsToRemove = new List<Hall>();
		List<Vector2I> connectionIDS = new List<Vector2I>();

		foreach (Hall hall in halls)
		{
			if (hall.connectingRoom1 == null || hall.connectingRoom2 == null || hall.connectingRoom1 == hall.connectingRoom2)
			{
				hallsToRemove.Add(hall);
			}
			else
			{
				Vector2I connectionID = new Vector2I(hall.connectingRoom1.id, hall.connectingRoom2.id);
				if (connectionIDS.Contains(connectionID))
				{
					hallsToRemove.Add(hall);
				}
				else
				{
					connectionIDS.Add(connectionID);
				}
			}

		}

		if (hallsToRemove.Count > 0)
		{
			GD.Print("Need to remove this many dead-beat halls: ", hallsToRemove.Count);
			foreach (Hall hall in hallsToRemove)
			{
				halls.Remove(hall);

			}

		}



	}

	private void SetHallNeighbors(Hall hall)
	{
		//This Method crawls the hallway and finds the endpoints
		//side1 will always be either the farthest left or farthest up, vise versa for side2
		//side1/2 are then repurpoused by adding one additional offset of each to get a coord outside the bounds of the hallway
		//these coords are then used to identify which rooms the hall connects to.
		Vector2I side1 = hall.tiles.First();
		Vector2I side2 = hall.tiles.First();
		if (hall.tiles.Count == 1)
		{
			// Vector2I checker = side1 + Vector2I.Up;
			foreach (Room room in rooms)
			{
				if (room.tiles.Contains(side1 + Vector2I.Up) || room.tiles.Contains(side1 + Vector2I.Left))
				{
					hall.connectingRoom1 = room;
				}
				if (room.tiles.Contains(side2 + Vector2I.Down) || room.tiles.Contains(side2 + Vector2I.Right))
				{
					hall.connectingRoom2 = room;
				}

			}
		}
		else
		{
			foreach (Vector2I tile in hall.tiles)
			{
				if (tile.X < side1.X)
				{
					side1.X = tile.X;
				}
				if (tile.Y < side1.Y)
				{
					side1.Y = tile.Y;
				}

				if (tile.X > side2.X)
				{
					side2.X = tile.X;
				}
				if (tile.Y > side2.Y)
				{
					side2.Y = tile.Y;
				}
			}

			bool isHorizontalHall = side1.X != side2.X;


			if (isHorizontalHall)
			{
				side1 += Vector2I.Left;
				side2 += Vector2I.Right;
			}
			else
			{
				side1 += Vector2I.Up;
				side2 += Vector2I.Down;

			}

			foreach (Room room in rooms)
			{
				if (room.tiles.Contains(side1))
				{
					hall.connectingRoom1 = room;
				}
				if (room.tiles.Contains(side2))
				{
					hall.connectingRoom2 = room;
				}
			}
		}


		// 	Vector2I side1 = hall.tiles.First();
		// 	Vector2I side2 = hall.tiles.First();
		// 	Vector2I[] neighbors = new Vector2I[]
		// {
		// 		side1 + Vector2I.Up,
		// 		side1 + Vector2I.Down,
		// 		side1 + Vector2I.Left,
		// 		side1 + Vector2I.Right
		// };

		// 	int visitTiles = 0;
		// 	while (visitTiles < hall.tiles.Count)
		// 	{
		// 		foreach (Vector2I neighbor in neighbors)
		// 		{
		// 			if (hall.tiles.Contains(neighbor))
		// 			{

		// 				visitTiles++;
		// 			}
		// 		}
		// 	}

	}

	private int GenRoomId()
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
	private int GenHallId()
	{
		if (halls.Count == 0) return 0;
		int highestID = 0;
		foreach (Hall hall in halls)
		{
			if (hall.id > highestID)
			{
				highestID = hall.id;
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
					hallTiles.Add(tile);
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

	private void CrawlNeighbors(Vector2I tile, HashSet<Vector2I> tiles, HashSet<Vector2I> visited, HashSet<Vector2I> set)
	{
		visited.Add(tile);
		tiles.Add(tile);

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
			if (set.Contains(neighbor) && !visited.Contains(neighbor))
			{
				CrawlNeighbors(neighbor, tiles, visited, set);

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
