using DelaunatorSharp;
using Godot;
using System;

using System.Collections.Generic;


public partial class Utils : Node
{
    public Vector2 IPointToV2(IPoint point)
    {
        Vector2 output = new((float)point.X, (float)point.Y);
        return output;
    }

}
