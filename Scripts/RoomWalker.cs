using Godot;
using System;

using System.Collections.Generic;
using System.Linq;


public partial class RoomWalker : Node
{

    Rect2 borders;
    readonly Random rand = new();

    private readonly HashSet<Vector2I> roomCells = new();

    private Vector2I pos;

    public RoomWalker(Vector2I startingPos, Rect2 borders)
    {
        pos = startingPos;
        this.borders = borders;

    }




    public HashSet<Vector2I> Walk(float steps)
    {
        GenRoom(pos);
        float stepsTaken = 0;



        while (stepsTaken < steps)
        {
            Vector2I[] tileArray = roomCells.ToArray();
            GenRoom(tileArray[rand.Next(tileArray.Length)]);
            stepsTaken++;

        }


        return roomCells;

    }

    private void GenRoom(Vector2I position)
    {
        Vector2I size = new Vector2I(rand.Next(5) + 2, rand.Next(5) + 2);

        Vector2I topLeftCorner = position - new Vector2I((int)Mathf.Ceil(size[0] / 2), (int)Mathf.Ceil(size[1] / 2));

        for (int iy = 0; iy < size.Y; iy++)
        {
            for (int ix = 0; ix < size.X; ix++)
            {
                Vector2I newStep = topLeftCorner + new Vector2I(ix, iy);
                if (borders.HasPoint(newStep))
                {
                    roomCells.Add(newStep);
                }
            }
        }

    }
}
