using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public List<Room> neighbors = new();

    public int Id { get; } = 0;

    public int Size => tiles.Count;

    public Vector2I Center => FindCenter();

    public RoomType RoomType { get; set; } = RoomType.Normal;
    public Room()
    {
        Id = ++lastAssignedId;
    }

    private Vector2I FindCenter()
    {
        int centerX = tiles.Sum(v => v.X) / tiles.Count;
        int centerY = tiles.Sum(v => v.Y) / tiles.Count;
        Vector2I center = new(centerX, centerY);
        if (tiles.Contains(center))
        {
            return center;
        }
        if (tiles.Contains(center + Vector2I.Up))
        {
            return center + Vector2I.Up;
        }
        if (tiles.Contains(center + Vector2I.Down))
        {
            return center + Vector2I.Down;
        }
        if (tiles.Contains(center + Vector2I.Left))
        {
            return center + Vector2I.Left;
        }
        if (tiles.Contains(center + Vector2I.Right))
        {
            return center + Vector2I.Right;
        }
        return tiles.ToList()[0];
    }
}
