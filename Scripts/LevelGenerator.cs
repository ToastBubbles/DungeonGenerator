using DelaunatorSharp;
using Godot;
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

    //Main control method
    //Generates all the rooms at the starting position based on level params using the Walker
    //Buffer is the space around each room
    private void BuildRooms(int roomSteps, int minRooms, int maxRooms, int minBuffer, int maxBuffer)
    {
        GenerateRooms(roomSteps, minRooms, maxRooms);
        SpreadRooms(minBuffer, maxBuffer);
        SetRoomData();
        SetNeighbors();
    }

    //Uses Walker to generate rooms
    private void GenerateRooms(int roomSteps, int minRooms, int maxRooms)
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
    }

    //Spreads out all the rooms (because they are all created at the starting position)
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

    //Sets room data.... like types and debug info
    private void SetRoomData()
    {
        Room startingRoom = rooms.FirstOrDefault(room => room.RoomType == RoomType.Starting);
        Room farthestRoom = rooms.OrderByDescending(room => CalculateDistance(room.Center, startingRoom.Center)).First();
        farthestRoom.RoomType = RoomType.Boss;

        //Allows for debugger to show positions and IDs
        foreach (Room room in rooms)
        {
            debugPoints.Add(room.Center);
            debugRoomPoints.Add(new Vector3I(room.Center.X, room.Center.Y, room.Id));
        }
    }

    //returns a set of vectors representing the passed room with additional vectors representing the buffer around the room
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


    //Returns a Vector that is represenative of a caculated and random offset where the passed Room can fit in respect to other rooms and buffers. This offset is then applied (in the SpreadRooms method)
    private Vector2I FindRoomSpace(Room roomToPlace, List<Room> placedRooms, int minBuffer, int maxBuffer = 4)
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

    //Sets the neighbor properties for all rooms
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
    }

    public HashSet<Vector2I> GenHallways(List<IEdge> hallEdges)
    {
        //todo
        HallWalker hallWalker = new(hallEdges);

        return hallWalker.WalkHalls();
    }

    //Returns a hashset of all map tile positions
    public HashSet<Vector2I> GetMap(RoomType rt)
    {
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

    //Get triangulation data currently called elsewhere
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

        return edges;
    }

    static double CalculateDistance(Vector2I point1, Vector2I point2)
    {
        int dx = point1.X - point2.X;
        int dy = point1.Y - point2.Y;
        double output = Math.Sqrt(dx * dx + dy * dy);

        return output;
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

}
