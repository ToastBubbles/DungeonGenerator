using Godot;
using System;
using System.Collections.Generic;


public partial class Hall : Node
{
    public HashSet<Vector2I> tiles = new HashSet<Vector2I>();

    public int id { get; set; } = 0;

    public int size { get; set; } = 0;

    public Room connectingRoom1 { get; set; }

    public Room connectingRoom2 { get; set; }

    public Hall(Room room1, Room room2, int id)
    {
        connectingRoom1 = room1;

        connectingRoom2 = room2;

        this.id = id;

    }
}
