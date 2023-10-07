using DelaunatorSharp;
using Godot;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

public partial class line : Line2D
{
	public List<Vector2> points { get; set; } = new();
	private List<Room> rooms;

	List<IEdge> edges = new();

	main mainScene;
	int maxLength = 23;
	private List<IEdge> goodEdges = new();
	private List<IEdge> badEdges = new();
	private List<IEdge> longEdges = new();
	private List<IEdge> optionalEdges = new();
	int count = 0;
	public override void _Ready()
	{
		mainScene = GetParent().GetParent<main>();
		rooms = mainScene.rooms;
		mainScene.DebugTris += SetPoints;
	}
	public override void _Draw()
	{

		if (points != null && points.Count > 1)
		{

			Vector2[] pointsArr = new Vector2[points.Count];

			for (int i = 0; i < points.Count; i++)
			{
				pointsArr[i] = points[i] * 32;
			}

			Vector2[] badEdgeArr = IEdgeListToV2Array(badEdges);
			Vector2[] goodEdgeArr = IEdgeListToV2Array(goodEdges);
			Vector2[] longEdgeArr = IEdgeListToV2Array(longEdges);
			Vector2[] optEdgeArr = IEdgeListToV2Array(optionalEdges);
			Vector2[] normalEdgeArr = IEdgeListToV2Array(NormalizeEdges(goodEdges));

			if (badEdgeArr.Length > 0) DrawMultiline(badEdgeArr, new Color(1, 0, 0, 0.1f), 10);
			if (longEdgeArr.Length > 0) DrawMultiline(longEdgeArr, new Color(0, 0, 1, 0.1f), 10);
			if (optEdgeArr.Length > 0) DrawMultiline(optEdgeArr, new Color(1, 1, 0, 0.1f), 10);
			if (goodEdgeArr.Length > 0) DrawMultiline(goodEdgeArr, new Color(0, 1, 1, 0.1f), 10);
			if (normalEdgeArr.Length > 0) DrawMultiline(normalEdgeArr, new Color(0, 1, 1, 1), 10);
		}
		else
		{
			GD.Print("No points found");
		}

	}
	public void SetPoints(Vector2 point1, Vector2 point2, int Index, int total, Vector2 strtRmCntr, Vector2 endRmCntr)
	{
		count++;
		points.Add(point1);
		points.Add(point2);
		edges.Add(new Edge(Index, new Point(point1.X, point1.Y), new Point(point2.X, point2.Y)));

		if (count == total)
		{
			rooms = mainScene.rooms;
			ValidateEdges();
			QueueRedraw();
		}
	}


	//This takes our triangulation and cleans it up, removing edges that are too long, or overlapping other rooms
	private void ValidateEdges()
	{
		//This dictionary just counts how many edges are connected to each room, the left value is the room ID
		Dictionary<int, int> roomConnections = new Dictionary<int, int>();
		foreach (IEdge edge in edges)
		{
			List<Vector2I> tilesToCheck = new();
			bool isTooLong = false;
			double length = CalculateDistance(edge.P, edge.Q);
			if (length > maxLength)
			{
				isTooLong = true;
			}

			Room qRoom = null;

			Room pRoom = null;

			foreach (Room room in rooms)
			{
				float cx = room.Center.X;
				float cy = room.Center.Y;
				bool isQAtCenter = cx == (float)edge.Q.X && cy == (float)edge.Q.Y;
				bool isPAtCenter = cx == (float)edge.P.X && cy == (float)edge.P.Y;
				if (!isQAtCenter && !isPAtCenter)
				{
					foreach (Vector2I tile in room.tiles)
					{
						tilesToCheck.Add(tile);
					}
				}
				else
				{
					if (isPAtCenter)
					{
						pRoom = room;
					}
					else
					{
						qRoom = room;
					}
				}
			}

			Vector2 start = IPointToV2(edge.Q);
			Vector2 end = IPointToV2(edge.P);
			bool isBadEdge = DoesLineIntersectRoom(start, end, tilesToCheck);
			if (isBadEdge)
			{
				badEdges.Add(edge);
			}
			else
			{
				if (isTooLong)
				{
					longEdges.Add(edge);
				}
				else
				{
					goodEdges.Add(edge);
					if (qRoom != null)
					{
						AddOrUpdateCount(roomConnections, qRoom.Id);
					}
					if (pRoom != null)
					{
						AddOrUpdateCount(roomConnections, pRoom.Id);
					}
				}
			}

		}
		//remove excess edges
		foreach (KeyValuePair<int, int> item in roomConnections)
		{
			if (item.Value > 3)
			{ CheckForRedundantEdges(item.Key); }
		}

	}
	//Converts out angular edge connections to 'normalized' edges, this just turns each edge into two, where one edge is only allowed to traverse the X axis, while the other is locked to Y. This is done to help make normalish halways.
	private List<IEdge> NormalizeEdges(List<IEdge> edgesToNormalize)
	{
		List<IEdge> normalizedEdges = new List<IEdge>();
		foreach (IEdge edge in edgesToNormalize)
		{
			// Calculate the absolute difference between X and Y coordinates
			double dx = Math.Abs(edge.Q.X - edge.P.X);
			double dy = Math.Abs(edge.Q.Y - edge.P.Y);

			// Determine if the edge is vertical or horizontal based on the greater difference
			if (dx > dy)
			{
				// Create a vertical edge
				IEdge verticalEdge = new Edge
				{
					P = edge.P,
					Q = new Point(edge.P.X, edge.Q.Y)
				};

				// Create a horizontal edge
				IEdge horizontalEdge = new Edge
				{
					P = new Point(edge.P.X, edge.Q.Y),
					Q = edge.Q
				};

				normalizedEdges.Add(verticalEdge);
				normalizedEdges.Add(horizontalEdge);
			}
			else
			{
				// Create a horizontal edge
				IEdge horizontalEdge = new Edge
				{
					P = edge.P,
					Q = new Point(edge.Q.X, edge.P.Y)
				};

				// Create a vertical edge
				IEdge verticalEdge = new Edge
				{
					P = new Point(edge.Q.X, edge.P.Y),
					Q = edge.Q
				};

				normalizedEdges.Add(horizontalEdge);
				normalizedEdges.Add(verticalEdge);
			}
		}
		return normalizedEdges;
	}

	//Takes in a roomID and attempts to remove redundant edges if possible, for example, if Room A has an edge to Room B and another to Room C, it will check if there is an edge from Room B to C, if so, it will remove it.
	private void CheckForRedundantEdges(int roomId)
	{
		// Find the room with the specified ID
		Room room = rooms.FirstOrDefault(x => x.Id == roomId);
		// Handle the case where the room with the specified ID is not found
		if (room == null) { return; }

		float cx = room.Center.X;
		float cy = room.Center.Y;

		// Get the edges that are connected to the room center
		List<IEdge> myEdges = goodEdges
			.Where(edge => (edge.P.X == cx && edge.P.Y == cy) || (edge.Q.X == cx && edge.Q.Y == cy))
			.ToList();

		HashSet<IEdge> myLatEdges = new HashSet<IEdge>(); // Use HashSet to store unique edges

		foreach (IEdge edge in goodEdges)
		{
			int sharedPoints = 0;
			foreach (IEdge myEdge in myEdges)
			{
				// Skip comparing an edge with itself
				if (edge == myEdge) { continue; }

				if ((edge.Q.X == myEdge.Q.X && edge.Q.Y == myEdge.Q.Y) || (edge.P.X == myEdge.P.X && edge.P.Y == myEdge.P.Y)) { sharedPoints++; }

				if (sharedPoints > 1)
				{
					// Add the redundant edge to myLatEdges HashSet
					if (!myEdges.Contains(edge) && CanRemoveEdge(edge))// && rand.Next(0, 100) < 50)
					{
						myLatEdges.Add(edge);
						optionalEdges.Add(edge);
					}
				}
			}
		}
		foreach (IEdge edge in optionalEdges) { goodEdges.Remove(edge); }
	}


	//If a room has at least two connecting edges, returns true.
	private bool CanRemoveEdge(IEdge edge)
	{
		bool canRemove = false;
		Room rmQ = rooms.FirstOrDefault(x => x.Center.X == edge.Q.X && x.Center.Y == edge.Q.Y);
		Room rmP = rooms.FirstOrDefault(x => x.Center.X == edge.P.X && x.Center.Y == edge.P.Y);

		if (GetEdgeCount(rmP) > 1 && GetEdgeCount(rmQ) > 1) { canRemove = true; }

		return canRemove;
	}
	private int GetEdgeCount(Room room)
	{
		float cx = room.Center.X;
		float cy = room.Center.Y;
		int output = 0;

		foreach (IEdge edge in goodEdges)
		{
			bool qMatch = edge.Q.X == cx && edge.Q.Y == cy;
			bool pMatch = edge.P.X == cx && edge.P.Y == cy;

			if (qMatch || pMatch) { output++; }
		}

		return output;
	}

	private void AddOrUpdateCount(Dictionary<int, int> dictionary, int id)
	{
		// If the id is already in the dictionary, increment the count
		// Or if the id is not in the dictionary, add it with a count of 1
		if (dictionary.ContainsKey(id)) { dictionary[id]++; }
		else { dictionary[id] = 1; }
	}
	public bool DoesLineIntersectRoom(Vector2 start, Vector2 end, List<Vector2I> tilesToCheck)
	{
		foreach (Vector2I tilePos in tilesToCheck)
		{
			Rect2 tileRect = new Rect2(tilePos.X, tilePos.Y, 1, 1);

			// Check if the line intersects the tile's bounding rectangle
			if (DoLinesIntersect(start, end, tileRect.Position, tileRect.End)) { return true; }
		}

		return false;
	}

	// Function to check if two line segments intersect
	private bool DoLinesIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
	{
		float A1 = p2.Y - p1.Y;
		float B1 = p1.X - p2.X;
		float C1 = A1 * p1.X + B1 * p1.Y;

		float A2 = q2.Y - q1.Y;
		float B2 = q1.X - q2.X;
		float C2 = A2 * q1.X + B2 * q1.Y;

		float det = A1 * B2 - A2 * B1;

		if (Math.Abs(det) < 0.001) // Lines are parallel
		{
			return false;
		}
		else
		{
			float intersectX = (B2 * C1 - B1 * C2) / det;
			float intersectY = (A1 * C2 - A2 * C1) / det;

			// Check if the intersection point lies within the line segments
			return IsPointInsideLineSegment(new Vector2(intersectX, intersectY), p1, p2) &&
				   IsPointInsideLineSegment(new Vector2(intersectX, intersectY), q1, q2);
		}
	}

	// Function to check if a point is inside a line segment
	private bool IsPointInsideLineSegment(Vector2 point, Vector2 start, Vector2 end)
	{
		float minX = Mathf.Min(start.X, end.X);
		float maxX = Mathf.Max(start.X, end.X);
		float minY = Mathf.Min(start.Y, end.Y);
		float maxY = Mathf.Max(start.Y, end.Y);

		return point.X >= minX && point.X <= maxX && point.Y >= minY && point.Y <= maxY;
	}



	private int GetRoomIdFromV2(Vector2 pt)
	{
		return rooms.FirstOrDefault(r => r.Center.X == pt.X && r.Center.Y == pt.Y).Id;
	}
	private double CalculateDistance(IPoint p1, IPoint p2)
	{
		double dx = p1.X - p2.X;
		double dy = p1.Y - p2.Y;
		return Math.Sqrt(dx * dx + dy * dy);
	}

	private Vector2 IPointToV2(IPoint point)
	{
		Vector2 output = new((float)point.X, (float)point.Y);
		return output;
	}

	private Vector2[] IEdgeListToV2Array(List<IEdge> edgeList)
	{
		Vector2[] output = new Vector2[edgeList.Count * 2];

		for (int i = 0; i < edgeList.Count; i++)
		{
			output[i * 2] = IPointToV2(edgeList[i].P) * 32;
			output[i * 2 + 1] = IPointToV2(edgeList[i].Q) * 32;
		}

		return output;
	}
}
