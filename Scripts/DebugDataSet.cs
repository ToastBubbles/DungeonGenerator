using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DebugDataSet : Node
{
    public Vector2 center;
    public int id;
    public bool essential = false;

    public DebugDataSet(Vector2 center, int id, bool essential)
    {
        this.center = center;
        this.id = id;
        this.essential = essential;
    }

}
