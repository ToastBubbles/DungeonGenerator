using DelaunatorSharp;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class line : Line2D
{
	public List<Vector2> points { get; set; } = new();
	public List<IPoint> essPoints { get; set; } = new();
	private List<Room> rooms;

	List<IEdge> edges = new();

	List<IPoint> route = new();
	Color color = new Color(0, 1, 0, 1);
	main mainboi;

	int maxLength = 22;
	private List<IEdge> goodEdges = new();

	private List<IEdge> badEdges = new();
	private List<IEdge> longEdges = new();
	public override void _Ready()
	{
		mainboi = GetParent().GetParent<main>();
		rooms = mainboi.rooms;
		// mainboi.DebugData += SetPoints;
		mainboi.DebugTris += SetPoints2;

	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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

			// Vector2[] routeArr = new Vector2[route.Count];
			// for (int i = 0; i < route.Count; i++)
			// {
			// 	routeArr[i] = new Vector2((float)route[i].X * 32, (float)route[i].Y * 32);
			// }
			Vector2[] badEdgeArr = IEdgeListToV2Array(badEdges);
			Vector2[] goodEdgeArr = IEdgeListToV2Array(goodEdges);
			Vector2[] longEdgeArr = IEdgeListToV2Array(longEdges);

			if (badEdgeArr.Length > 0) DrawMultiline(badEdgeArr, new Color(1, 0, 0, 1), 10);
			if (longEdgeArr.Length > 0) DrawMultiline(longEdgeArr, new Color(0, 0, 1, 1), 10);
			if (goodEdgeArr.Length > 0) DrawMultiline(goodEdgeArr, color, 10);

			// DrawMultiline(pointsArr, color, 10);
			// DrawPolyline(routeArr, new Color(1, 0, 0, 1), 10);
		}
		else
		{
			GD.Print("No points found");
		}

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

	// public void SetPoints(Vector2 point, int id, bool essential)
	// {
	// 	if (essential)
	// 	{
	// 		points.Add(point);
	// 		QueueRedraw();
	// 	}
	// }
	int count = 0;
	public void SetPoints2(Vector2 point1, Vector2 point2, int Index, int total, Vector2 strtRmCntr, Vector2 endRmCntr, Vector2 essPt)
	{

		count++;

		points.Add(point1);
		points.Add(point2);
		edges.Add(new Edge(Index, new Point(point1.X, point1.Y), new Point(point2.X, point2.Y)));

		if (essPt != new Vector2(99999, 99999))
		{
			essPoints.Add(ToIPoint(essPt));
		}


		if (count == total)
		{
			rooms = mainboi.rooms;
			// route = FindShortestRoute(ToIPoint(strtRmCntr), ToIPoint(endRmCntr), essPoints);
			IPoint start = new Point(strtRmCntr.X, strtRmCntr.Y);
			IPoint end = new Point(endRmCntr.X, endRmCntr.Y);
			// route = GenerateRandomPath(start, end);
			ValidateEdges();
			GD.Print("ROOMS", rooms.Count);
			GD.Print("Bad edges: ", badEdges.Count);
			GD.Print("Good edges: ", goodEdges.Count);
			GD.Print("!!!!!!", route.Count);
			GD.Print($"route, start: {strtRmCntr} end: {endRmCntr}, ess:");
			foreach (IPoint point in essPoints)
			{
				GD.Print(point);
			}
			GD.Print("Route:");
			foreach (IPoint item in route)
			{
				GD.Print(item);
			}
			QueueRedraw();
			GD.Print("Drawing");
		}
	}


	private void ValidateEdges()
	{
		foreach (IEdge edge in edges)
		{
			List<Vector2I> tilesToCheck = new();
			bool isTooLong = false;
			double length = CalculateDistance(edge.P, edge.Q);
			if (length > maxLength)
			{
				isTooLong = true;
			}
			foreach (Room room in rooms)
			{
				float cx = room.Center.X;
				float cy = room.Center.Y;
				bool isQAtCenter = cx == (float)edge.Q.X && cy == (float)edge.Q.Y;
				bool isPAtCenter = cx == (float)edge.P.X && cy == (float)edge.P.Y;
				// GD.Print($"cx: {cx}, cy: {cy}, edge {edge.Q}, {edge.P}, isQ {isQAtCenter}, isP {isPAtCenter}");
				if (!isQAtCenter && !isPAtCenter)
				{
					foreach (Vector2I tile in room.tiles)
					{
						tilesToCheck.Add(tile);
					}
					// GD.Print($"Edge {edge.Q}, {edge.P} DOES NOT TOUCH this room {room.Center}");
				}
				else
				{
					GD.Print($"Edge {edge.Q}, {edge.P} touches this room ({room.Center})");
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
				}
			}

		}
	}



	public bool DoesLineIntersectRoom(Vector2 start, Vector2 end, List<Vector2I> tilesToCheck)
	{
		foreach (Vector2I tilePos in tilesToCheck)
		{
			Rect2 tileRect = new Rect2(tilePos.X, tilePos.Y, 1, 1);

			// Check if the line intersects the tile's bounding rectangle
			if (DoLinesIntersect(start, end, tileRect.Position, tileRect.End))
			{
				return true;
			}
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

	// Helper method to calculate the distance between two points.
	private double CalculateDistance(IPoint p1, IPoint p2)
	{
		double dx = p1.X - p2.X;
		double dy = p1.Y - p2.Y;
		return Math.Sqrt(dx * dx + dy * dy);
	}
	// public List<IPoint> GenerateRandomPath(IPoint startPoint, IPoint endPoint)
	// {
	// 	// Initialize a list to store the path points
	// 	List<IPoint> path = new List<IPoint>();

	// 	// Add the starting point to the path
	// 	path.Add(startPoint);
	// 	HashSet<IPoint> allPoints = new HashSet<IPoint>();
	// 	foreach (IEdge edge in edges)
	// 	{
	// 		allPoints.Add(edge.Q);
	// 		allPoints.Add(edge.P);
	// 	}

	// 	foreach (IEdge e in edges)
	// 	{
	// 		GD.Print($"*****************************{e.Q}*********{e.P}*******************************");
	// 	}
	// 	// Create a HashSet to keep track of used points
	// 	HashSet<IPoint> usedPoints = new HashSet<IPoint>();

	// 	// Add the starting point to the used points
	// 	usedPoints.Add(startPoint);

	// 	// Set the desired number of points/edges to use (60% of the total)
	// 	int targetCount = (int)Math.Ceiling(0.6 * allPoints.Count);
	// 	GD.Print("ccc", targetCount);

	// 	Random rand = new Random();

	// 	while (path.Count < targetCount && !path.Contains(endPoint))
	// 	{
	// 		// Get the last point in the path
	// 		IPoint currentPoint = path.Last();

	// 		// Get all edges that connect to the current point and haven't been used
	// 		// List<IEdge> availableEdges = edges
	// 		// 	.Where(e => (e.P == currentPoint || e.Q == currentPoint) &&
	// 		// 				!usedPoints.Contains(e.P) && !usedPoints.Contains(e.Q))
	// 		// 	.ToList();
	// 		List<IEdge> connectedEdges = new List<IEdge>();
	// 		foreach (IEdge edge in edges)
	// 		{
	// 			GD.Print($"WHAT THE FUCK {edge.P} - {edge.Q} = {currentPoint}");
	// 			if (edge.P.X == currentPoint.X && edge.P.Y == currentPoint.Y || edge.Q.X == currentPoint.X && edge.Q.Y == currentPoint.Y)
	// 			{
	// 				GD.Print("THE SAME");
	// 				connectedEdges.Add(edge);
	// 			}
	// 		}

	// 		List<IEdge> availableEdges = new List<IEdge>();
	// 		foreach (var edge in connectedEdges)
	// 		{
	// 			if ((usedPoints.Contains(edge.P) && !usedPoints.Contains(edge.Q)) ||
	// 				(!usedPoints.Contains(edge.P) && usedPoints.Contains(edge.Q)))
	// 			{
	// 				availableEdges.Add(edge);
	// 			}
	// 		}

	// 		GD.Print("Con edges: ", connectedEdges.Count);
	// 		GD.Print("Avail edges: ", availableEdges.Count);
	// 		GD.Print("Curr Pnt: ", currentPoint);

	// 		if (availableEdges.Count == 0)
	// 		{
	// 			// No available edges, break the loop
	// 			break;
	// 		}

	// 		// Choose a random edge from the available edges
	// 		int randomIndex = rand.Next(availableEdges.Count);
	// 		IEdge selectedEdge = availableEdges[randomIndex];

	// 		// Add the unvisited endpoint to the path
	// 		if (!usedPoints.Contains(selectedEdge.P))
	// 		{
	// 			path.Add(selectedEdge.P);
	// 			usedPoints.Add(selectedEdge.P);
	// 		}
	// 		else
	// 		{
	// 			path.Add(selectedEdge.Q);
	// 			usedPoints.Add(selectedEdge.Q);
	// 		}
	// 	}

	// 	// Add the endpoint to the path if it's not already there
	// 	if (!path.Contains(endPoint))
	// 	{
	// 		path.Add(endPoint);
	// 	}

	// 	return path;
	// }


	private IPoint ToIPoint(Vector2 point)
	{
		IPoint output = new Point(point.X, point.Y);
		return output;
	}

	private Vector2 IPointToV2(IPoint point)
	{
		Vector2 output = new((float)point.X, (float)point.Y);
		return output;
	}
	// private void FindPath(Vector2 strtRmCntr)
	// {
	// 	IPoint lastPoint = ToIPoint(strtRmCntr);
	// 	List<IEdge> startingEdges = new();

	// 	foreach (Edge edge in edges)
	// 	{
	// 		if (edge.P == lastPoint || edge.Q == lastPoint)
	// 		{
	// 			startingEdges.Add(edge);
	// 		}
	// 	}

	// 	bool complete = false;

	// 	List<List<IEdge>> routes = new();

	// 	while (!complete)
	// 	{
	// 		List<IEdge> edgesCopy = new(edges);
	// 		List<IEdge> edgesToLoop = new(startingEdges);
	// 		List<IPoint> prospectPoints = new();
	// 		foreach (Edge edge in edgesToLoop)
	// 		{

	// 			if (edge.P != lastPoint)
	// 			{
	// 				prospectPoints.Add(edge.P);
	// 			}
	// 			else if (edge.Q != lastPoint)
	// 			{
	// 				prospectPoints.Add(edge.Q);
	// 			}
	// 		}
	// 	}


	// }




}
