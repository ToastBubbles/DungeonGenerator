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
    private Vector2I startingPos;
    private Rect2 borders;
    readonly Random rand = new();
    private List<Room> rooms = new List<Room>();




    public LevelGenerator(Vector2I startingPos, Rect2 borders, LevelType level)
    {
        this.startingPos = startingPos;
        this.borders = borders;
        if (level == LevelType.Forest1)
        {
            GenForest();
        }

    }


    private void GenForest()
    {
        //Add additional properties here and to BuildRoom later
        BuildRooms(20, 8, 15, 1);
    }

    //minBuffer is the space around each room
    private void BuildRooms(int roomSteps, int minRooms, int maxRooms, int minBuffer = 1)
    {

        int placedRooms = 0;
        int roomsToPlace = rand.Next(minRooms, maxRooms + 1);

        while (placedRooms < roomsToPlace)
        {
            RoomWalker walker = new(startingPos, borders);
            Room room = new()
            {
                tiles = walker.Walk(roomSteps)
            };

            rooms.Add(room);

            placedRooms++;
        }

        SpreadRooms(minBuffer);
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
    private void SpreadRooms(int minBuffer)
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
            Vector2I offest = FindRoomSpace(rtp, PlacedRooms, minBuffer);
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




    private Vector2I FindRoomSpace(Room roomToPlace, List<Room> placedRooms, int minBuffer, int maxBuffer = 6)
    {
        Vector2I directionToMove = new(rand.Next(0, 7) - 3, rand.Next(0, 7) - 3);
        while (directionToMove == Vector2I.Zero)
        {
            directionToMove = new(rand.Next(0, 7) - 3, rand.Next(0, 7) - 3);
        }

        List<Vector2I> DIRECTIONS = new List<Vector2I>
    {
    Vector2I.Right,
    Vector2I.Up,
    Vector2I.Left,
    Vector2I.Down
    };
        List<Vector2I> usedPositions = new();
        foreach (Room room in placedRooms)
        {
            foreach (Vector2I cell in room.tiles)
            {
                usedPositions.Add(cell);
                foreach (Vector2I dir in DIRECTIONS)
                {
                    usedPositions.Add(cell + dir);
                }
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
}