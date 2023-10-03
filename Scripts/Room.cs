using Godot;
using System;
using System.Collections.Generic;

public enum RoomType
{
    Normal,
    Starting,
    Treasure,
    Boss
}
public partial class Room : Node
{
    private static int lastAssignedId = 0;
    public HashSet<Vector2I> tiles = new();

    public int Id { get; } = 0;

    public int Size => tiles.Count;

    public RoomType RoomType { get; set; } = RoomType.Normal;
    public Room()
    {
        Id = ++lastAssignedId;
    }
}
