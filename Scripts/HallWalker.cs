using DelaunatorSharp;
using Godot;
using System;

using System.Collections.Generic;


public partial class HallWalker : Node
{
    List<IEdge> edges;
    public HallWalker(List<IEdge> edges)
    {
        this.edges = edges;
    }

    //converts edges to vectors representing hall tiles
    public HashSet<Vector2I> WalkHalls()
    {
        HashSet<Vector2I> tiles = new();
        if (edges != null)
        {
            foreach (IEdge edge in edges)
            {
                Vector2 p1 = IPointToV2(edge.Q);
                Vector2 p2 = IPointToV2(edge.P);
                bool isXAxis = p1.Y == p2.Y;
                if (isXAxis)
                {
                    int spread = (int)Mathf.Abs(p1.X - p2.X);
                    int dir = p1.X > p2.X ? -1 : 1;
                    int count = 0;
                    while (count < spread)
                    {
                        Vector2I point = new((int)p1.X + count * dir, (int)p1.Y);
                        tiles.Add(point);

                        count++;
                    }
                }
                else
                {
                    int spread = (int)Mathf.Abs(p1.Y - p2.Y);
                    int dir = p1.Y > p2.Y ? -1 : 1;
                    int count = 0;
                    while (count < spread)
                    {
                        Vector2I point = new((int)p1.X, (int)p1.Y + count * dir);
                        tiles.Add(point);

                        count++;
                    }
                }
            }


        }
        return tiles;
    }
    public Vector2 IPointToV2(IPoint point)
    {
        Vector2 output = new((float)point.X, (float)point.Y);
        return output;
    }
}
