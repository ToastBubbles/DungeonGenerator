using Godot;
using System;
using System.Collections.Generic;

public enum DoorDirection
{
    Up,
    Down,
    Left,
    Right,
    UpDown,
    LeftRight,
}
public partial class Door : Node
{
    public Vector2I coords;

    public DoorDirection direction;

    public Hall parentHall;

    public int id { get; set; } = 0;

}
