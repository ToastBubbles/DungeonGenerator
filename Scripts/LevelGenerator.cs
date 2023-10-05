using DelaunatorSharp;
using Godot;
using GodotPlugins.Game;
using System;
using System.Collections.Generic;
using System.Linq;


public enum LevelType
{
    Forest1
}
public partial class LevelGenerator : Node
{

    public List<DebugDataSet> debugData = new();
    public List<Vector2I> debugPoints = new();
    public List<Vector3I> debugRoomPoints = new();
    private Vector2I startingPos;


    readonly Random rand = new();
    public List<Room> rooms = new List<Room>();




    public LevelGenerator(Vector2I startingPos, LevelType level)
    {

        this.startingPos = startingPos;

        if (level == LevelType.Forest1)
        {
            GenForest();
        }

    }


    private void GenForest()
    {
        //Add additional properties here and to BuildRoom later
        BuildRooms(20, 8, 15, 1, 4);
    }

    //minBuffer is the space around each room
    private void BuildRooms(int roomSteps, int minRooms, int maxRooms, int minBuffer, int maxBuffer)
    {

        int placedRooms = 0;
        int roomsToPlace = rand.Next(minRooms, maxRooms + 1);

        while (placedRooms < roomsToPlace)
        {
            RoomWalker walker = new(startingPos);
            Room room = new()
            {
                tiles = walker.Walk(roomSteps)
            };

            rooms.Add(room);

            placedRooms++;
        }

        SpreadRooms(minBuffer, maxBuffer);
        GetRoomOrdering();
    }

    public HashSet<Vector2I> GetMap(RoomType rt)
    {
        // GD.Print(rooms.Count);
        HashSet<Vector2I> output = new();
        if (rooms.Count > 0)
        {
            foreach (Room room in rooms)
            {
                if (room.RoomType == rt)
                    foreach (Vector2I tile in room.tiles)
                    {
                        output.Add(tile);
                    }

            }
        }


        return output;


    }
    private void SpreadRooms(int minBuffer, int maxBuffer)
    {
        List<Room> CopyOfRooms = new(rooms);
        List<Room> PlacedRooms = new();
        Room smallestRoom = rooms.OrderBy(r => r.Size).FirstOrDefault();
        smallestRoom.RoomType = RoomType.Starting;
        PlacedRooms.Add(smallestRoom);
        CopyOfRooms.Remove(smallestRoom);



        while (CopyOfRooms.Count > 0)
        {
            Room rtp = CopyOfRooms[0];
            Vector2I offest = FindRoomSpace(rtp, PlacedRooms, minBuffer, maxBuffer);
            HashSet<Vector2I> offsetTiles = new();
            foreach (Vector2I tile in rtp.tiles)
            {
                offsetTiles.Add(tile + offest);
            }
            rtp.tiles = offsetTiles;
            PlacedRooms.Add(rtp);
            CopyOfRooms.Remove(rtp);


        }


    }


    private HashSet<Vector2I> BufferRoom(HashSet<Vector2I> tiles, int buffers)
    {
        HashSet<Vector2I> output = new();

        List<Vector2I> DIRECTIONS = new List<Vector2I>
    {
    Vector2I.Right,
    Vector2I.Up,
    Vector2I.Left,
    Vector2I.Down
    };
        int timesBuffered = 0;
        while (timesBuffered < buffers)
        {
            foreach (Vector2I tile in tiles)
            {
                output.Add(tile);
                foreach (Vector2I dir in DIRECTIONS)
                {
                    output.Add(tile + dir);
                }

            }
            tiles = new(output);
            timesBuffered++;
        }

        return output;

    }

    private Vector2I FindRoomSpace(Room roomToPlace, List<Room> placedRooms, int minBuffer, int maxBuffer = 6)
    {
        Vector2I directionToMove = new(rand.Next(0, 7) - 3, rand.Next(0, 7) - 3);
        while (directionToMove == Vector2I.Zero)
        {
            directionToMove = new(rand.Next(0, 7) - 3, rand.Next(0, 7) - 3);
        }


        List<Vector2I> usedPositions = new();
        foreach (Room room in placedRooms)
        {
            int buffer = rand.Next(minBuffer, maxBuffer + 1);
            // int bufferIterations = 0;

            // foreach (Vector2I cell in room.tiles)
            // {
            //     usedPositions.Add(cell);

            // }
            HashSet<Vector2I> bufferedRoom = BufferRoom(room.tiles, buffer);
            foreach (Vector2I tile in bufferedRoom)
            {
                usedPositions.Add(tile);
            }
        }

        Vector2I offset = usedPositions[rand.Next(0, usedPositions.Count - 1)];


        bool suitableArea = false;



        while (!suitableArea)
        {

            offset += directionToMove;
            //do boundary checks
            suitableArea = true;
            foreach (Vector2I tile in roomToPlace.tiles)
            {
                if (usedPositions.Contains(tile + offset - startingPos))
                {
                    suitableArea = false;
                    break;
                }

            }

        }

        return offset - startingPos;

    }

    private void GetRoomOrdering()
    {
        // List<Room> orderedRooms = rooms.OrderBy(room => room.Center.X).ThenBy(room => room.Center.Y).ToList();
        Room startingRoom = rooms.FirstOrDefault(room => room.RoomType == RoomType.Starting);
        Room farthestRoom = rooms.OrderByDescending(room => CalculateDistance(room.Center, startingRoom.Center)).First();
        farthestRoom.RoomType = RoomType.Boss;
        GenHallways(startingRoom, farthestRoom);

        List<Room> roomsCopy = new(rooms);
        roomsCopy.Remove(startingRoom);
        roomsCopy.Remove(farthestRoom);
        //lock a percentage of rooms/ make them leaf rooms
        int percentLocked = (int)Mathf.Floor(roomsCopy.Count * 0.2f);
        while (percentLocked > 0)
        {
            roomsCopy.Remove(roomsCopy[rand.Next(0, roomsCopy.Count - 1)]);
            percentLocked--;
        }

        List<Room> orderedRooms = new()
        {
            startingRoom
        };

        Room currentWorkingRoom = startingRoom;

        while (roomsCopy.Count > 0)
        {
            currentWorkingRoom = FindClosestRoom(currentWorkingRoom, roomsCopy);
            orderedRooms.Add(currentWorkingRoom);
            roomsCopy.Remove(currentWorkingRoom);
        }
        orderedRooms.Add(farthestRoom);

        // foreach (Room room in orderedRooms)
        // {
        //     GD.Print(room.Center);
        // }
        // if (debugLine != null)
        // {
        //     debugLine
        // }


        foreach (Room room in rooms)
        {
            debugPoints.Add(room.Center);
            debugRoomPoints.Add(new Vector3I(room.Center.X, room.Center.Y, room.Id));
        }
        SetNeighbors();

        List<int> order = new();
        List<int> leafHolder = new();
        foreach (Room room in rooms)
        {
            if (room.neighbors.Count <= 1)
            {
                leafHolder.Add(room.Id);
            }
        }
        List<int> leafs = new(leafHolder);
        // order.Add(startingRoom.Id);
        int lastAddedId = startingRoom.Id;
        int tries = 0;
        bool routeGenerated = false;


        int maxTries = 150;
        while (!routeGenerated && tries < maxTries)
        {
            bool leafed = false;
            bool badTry = false;
            if (order.Count == 0)
            {
                order.Add(startingRoom.Id);
                lastAddedId = startingRoom.Id;
            }
            Room lastRoom = rooms.FirstOrDefault(room => room.Id == lastAddedId);
            List<Room> tempNeighbors = new(lastRoom.neighbors);

            if (tempNeighbors.Count > 1)
            {
                ShuffleList(tempNeighbors);
            }
            else if (tempNeighbors.Count == 1 && tempNeighbors[0].neighbors.Count == 1)
            {
                leafs.Add(tempNeighbors[0].Id);
                leafed = true;
            }

            int i = 0;
            Room nextRoom = tempNeighbors[i];
            if (!leafed)
            {

                while (order.Contains(nextRoom.Id) || leafs.Contains(nextRoom.Id))
                {
                    if (i == tempNeighbors.Count - 1)
                    {
                        badTry = true;
                        break;
                    }
                    i++;
                    nextRoom = tempNeighbors[i];
                }
            }

            if (order.Count + leafs.Count == rooms.Count)
            {
                routeGenerated = true;
            }
            else if (badTry)
            {
                order.Clear();
                leafs = new(leafHolder);
            }
            else
            {
                if (!leafed)
                {
                    order.Add(nextRoom.Id);
                    lastRoom = nextRoom;
                    lastAddedId = nextRoom.Id;
                }
            }
            GD.Print("order:");
            foreach (var item in order)
            {
                GD.Print(item);
            }
            GD.Print("leafs:");
            foreach (var item in leafs)
            {
                GD.Print(item);
            }
            tries++;
        }


        GD.Print($"{tries} tries!");
        if (tries < maxTries)
        {
            if (!order.Contains(farthestRoom.Id))
            {
                order.Add(farthestRoom.Id);
                leafs.Remove(farthestRoom.Id);
            }
            foreach (int id in order)
            {
                Room rm = rooms.FirstOrDefault(room => room.Id == id);
                debugData.Add(new DebugDataSet((Vector2)rm.Center, id, true));

            }
            foreach (int id in leafs)
            {
                Room rm = rooms.FirstOrDefault(room => room.Id == id);
                debugData.Add(new DebugDataSet((Vector2)rm.Center, id, false));
            }
        }

    }



    public static void ShuffleList<T>(List<T> list)
    {
        Random rng = new Random();
        int n = list.Count;

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
    private void SetNeighbors()
    {
        int minDist = 20;
        foreach (Room room in rooms)
        {
            foreach (Room otherRoom in rooms)
            {
                if (room != otherRoom)
                {
                    if (CalculateDistance(room.Center, otherRoom.Center) < minDist)
                    {
                        room.neighbors.Add(otherRoom);
                    }
                }

            }
        }

        GD.Print("Neighbors:");
        foreach (Room room in rooms)
        {
            GD.Print($"Room {room.Id} has the following Neighbors:");
            foreach (Room neighbor in room.neighbors)
            {
                GD.Print(neighbor.Id);

            }
        }
    }



    private Room FindClosestRoom(Room currentRoom, List<Room> roomSelection)
    {
        double smallestDistance = 500;
        int minDist = 10;
        Room output = roomSelection[0];
        foreach (Room room in roomSelection)
        {
            double dist = CalculateDistance(currentRoom.Center, room.Center);
            if (dist < smallestDistance)
            {
                smallestDistance = dist;
                output = room;
            }


        }

        return output;
    }

    // public List<Vector2I> GetDebugPoints()
    // {
    //     return debugPoints;
    // }

    private void GenHallways(Room startingRoom, Room bossRoom)
    {

    }

    public IEnumerable<IEdge> Delaunatate()
    {
        IPoint[] points = new IPoint[rooms.Count];
        for (int i = 0; i < rooms.Count; i++)
        {
            IPoint point = new Point
            {
                X = rooms[i].Center.X,
                Y = rooms[i].Center.Y,
            };
            points[i] = point;
        }
        Delaunator delaunator = new(points);
        IEnumerable<IEdge> edges = delaunator.GetEdges();

        // foreach (IEdge edge in edges)
        // {
        //     GD.Print($"{edge.P}, {edge.Q}, {edge.Index}");
        // }
        return edges;

    }

    static double CalculateDistance(Vector2I point1, Vector2I point2)
    {
        int dx = point1.X - point2.X;
        int dy = point1.Y - point2.Y;
        double output = Math.Sqrt(dx * dx + dy * dy);

        return output;
    }


}
