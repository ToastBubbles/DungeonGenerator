using Godot;
using System;

using System.Collections.Generic;
using System.Linq;


public partial class RoomWalker : Node
{

    readonly Random rand = new();

    private readonly HashSet<Vector2I> roomCells = new();

    private Vector2I pos;

    public RoomWalker(Vector2I startingPos)
    {
        pos = startingPos;
    }


    //Main method, takes in how many steps should be taken and returns a vector set representing a room
    //What this does, is generate a random rectangle of vectors, add those to a collection, then for each step, it will choose a random point from it's collection and gen a new ranmdom section
    public HashSet<Vector2I> Walk(float steps)
    {
        GenSection(pos);
        float stepsTaken = 0;

        while (stepsTaken < steps)
        {
            //this looks dumb but I couldn't pull randomly from a hashset sooo
            Vector2I[] tileArray = roomCells.ToArray();
            GenSection(tileArray[rand.Next(tileArray.Length)]);
            stepsTaken++;
        }

        return roomCells;

    }

    private void GenSection(Vector2I position)
    {
        Vector2I size = new Vector2I(rand.Next(5) + 2, rand.Next(5) + 2);

        Vector2I topLeftCorner = position - new Vector2I((int)Mathf.Ceil(size[0] / 2), (int)Mathf.Ceil(size[1] / 2));

        for (int iy = 0; iy < size.Y; iy++)
        {
            for (int ix = 0; ix < size.X; ix++)
            {
                Vector2I newStep = topLeftCorner + new Vector2I(ix, iy);

                roomCells.Add(newStep);

            }
        }

    }
}
