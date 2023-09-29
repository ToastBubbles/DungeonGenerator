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
    public HashSet<Vector2I> tiles = new HashSet<Vector2I>();

    public int id { get; set; } = 0;

    public int size { get; set; } = 0;

    public RoomType roomType { get; set; } = RoomType.Normal;
}
