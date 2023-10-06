using DelaunatorSharp;
using Godot;
using System;
using System.Collections.Generic;
using System.Data;
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
	Random rand = new();

	int maxLength = 22;
	private List<IEdge> goodEdges = new();

	private List<IEdge> badEdges = new();
	private List<IEdge> longEdges = new();
	private List<IEdge> optionalEdges = new();
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
			Vector2[] optEdgeArr = IEdgeListToV2Array(optionalEdges);

			if (badEdgeArr.Length > 0) DrawMultiline(badEdgeArr, new Color(1, 0, 0, 0.1f), 10);
			if (longEdgeArr.Length > 0) DrawMultiline(longEdgeArr, new Color(0, 0, 1, 0.1f), 10);
			if (optEdgeArr.Length > 0) DrawMultiline(optEdgeArr, new Color(1, 1, 0, 0.1f), 10);
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
				// roomConnections.TryAdd(room.Id, 1);
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
					if (isPAtCenter)
					{
						pRoom = room;
					}
					else
					{
						qRoom = room;
					}

					// GD.Print($"Edge {edge.Q}, {edge.P} touches this room ({room.Center})");
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
		foreach (KeyValuePair<int, int> item in roomConnections)
		{
			if (item.Value > 3)
			{
				GD.Print($"Room {item.Key} has {item.Value} connections. Too many!");
				CheckForRedundantEdges(item.Key);
			}
			else
			{
				GD.Print($"Room {item.Key} has {item.Value} connections");
			}


		}

	}
	// Dictionary<IPoint, List<IEdge>> edgeGroups = new Dictionary<IPoint, List<IEdge>>();

	// 		foreach (IEdge edge in goodEdges)
	// 		{
	// 			// Group by P
	// 			IPoint keyP = new Point(edge.P.X, edge.P.Y);
	// 			if (!edgeGroups.ContainsKey(keyP))
	// 				edgeGroups[keyP] = new List<IEdge>();
	// 			edgeGroups[keyP].Add(edge);

	// 			// Group by Q
	// 			IPoint keyQ = new Point(edge.Q.X, edge.Q.Y);
	// 			if (!edgeGroups.ContainsKey(keyQ))
	// 				edgeGroups[keyQ] = new List<IEdge>();
	// 			edgeGroups[keyQ].Add(edge);
	// 		}

	// 		// Now, you can iterate through the groups to find edges connecting neighboring points
	// 		foreach (var edgeGroup in edgeGroups.Values)
	// 		{
	// 			if (edgeGroup.Count > 1)
	// 			{
	// 				// This group contains edges that connect neighboring points
	// 				// You can add them to your myLatEdges list or perform other actions
	// 				foreach (var edge in edgeGroup)
	// 				{
	// 					// Add to myLatEdges or process as needed
	// 				}
	// 			}
	// 		}
	private void CheckForRedundantEdges(int roomId)
	{
		// Find the room with the specified ID
		Room room = rooms.FirstOrDefault(x => x.Id == roomId);

		if (room == null)
		{
			// Handle the case where the room with the specified ID is not found
			return;
		}

		Vector2I roomCenter = room.Center;
		float cx = roomCenter.X;
		float cy = roomCenter.Y;

		// Get the edges that are connected to the room center
		List<IEdge> myEdges = goodEdges
			.Where(edge => (edge.P.X == cx && edge.P.Y == cy) || (edge.Q.X == cx && edge.Q.Y == cy))
			.ToList();

		HashSet<IEdge> myLatEdges = new HashSet<IEdge>(); // Use HashSet to store unique edges

		foreach (IEdge edge in myEdges)
		{
			GD.PrintRich($"[color=#FF00FF]{roomId} has edge: {GetRoomIdFromV2(IPointToV2(edge.Q))}, {GetRoomIdFromV2(IPointToV2(edge.P))}[/color]");
		}

		foreach (IEdge edge in goodEdges)
		{
			// GD.PrintRich($"[color=#6600FF]Checking edge: {GetRoomIdFromV2(IPointToV2(edge.Q))}, {GetRoomIdFromV2(IPointToV2(edge.P))}[/color]");
			// bool qMatch = false; //= edge.Q.X == myEdge.Q.X && edge.Q.Y == myEdge.Q.Y;
			// bool pMatch = false;//edge.P.X == myEdge.P.X && edge.P.Y == myEdge.P.Y;
			int sharedPoints = 0;
			foreach (IEdge myEdge in myEdges)
			{

				if (edge == myEdge)
				{
					// Skip comparing an edge with itself
					GD.PrintRich($"[color=#FF0000]Continuing...[/color]");
					continue;
				}

				// if (qMatch == false) qMatch = edge.Q.X == myEdge.Q.X && edge.Q.Y == myEdge.Q.Y;
				if (edge.Q.X == myEdge.Q.X && edge.Q.Y == myEdge.Q.Y)
				{
					// qMatch = true;
					sharedPoints++;
				}
				if (edge.P.X == myEdge.P.X && edge.P.Y == myEdge.P.Y)
				{
					// pMatch = true;
					sharedPoints++;
				}
				// if (pMatch == false) pMatch = edge.P.X == myEdge.P.X && edge.P.Y == myEdge.P.Y;
				GD.PrintRich($"[color=#0088FF]Edge: {GetRoomIdFromV2(IPointToV2(edge.Q))}, {GetRoomIdFromV2(IPointToV2(edge.P))} Checked against myEdge: {GetRoomIdFromV2(IPointToV2(myEdge.Q))}, {GetRoomIdFromV2(IPointToV2(myEdge.P))}[/color]");

				if (sharedPoints > 1)
				{
					// Add the redundant edge to myLatEdges HashSet
					if (!myEdges.Contains(edge) && CanRemoveEdge(edge))// && rand.Next(0, 100) < 50)
					{
						GD.PrintRich($"[color=#FFFF00]{roomId} is removing edge: {GetRoomIdFromV2(IPointToV2(edge.Q))}, {GetRoomIdFromV2(IPointToV2(edge.P))}[/color]");
						myLatEdges.Add(edge);

						optionalEdges.Add(edge);
					}
				}


			}
		}

		foreach (IEdge edge in optionalEdges)
		{
			goodEdges.Remove(edge);
		}

		GD.Print("myLatEdges ", myLatEdges.Count);
		// foreach (IEdge edge in myLatEdges)
		// {
		// 	GD.Print($"{edge.Q},{edge.P}");

		// }
		// ValidateRoomConnections();
	}

	private int GetRoomIdFromV2(Vector2 pt)
	{

		return rooms.FirstOrDefault(r => r.Center.X == pt.X && r.Center.Y == pt.Y).Id;
	}

	// private void ValidateRoomConnections()
	// {

	// 	List<IEdge> remainingEdges = new(goodEdges);
	// 	List<IEdge> usedEdges = new();
	// 	IEdge currentEdge = remainingEdges.First();
	// 	// IEdge nextEdge;
	// 	List<int> visitedRooms = new();
	// 	int i = 0;
	// 	while (i < goodEdges.Count)
	// 	{
	// 		Room rmQ = rooms.FirstOrDefault(x => x.Center.X == currentEdge.Q.X && x.Center.Y == currentEdge.Q.Y);
	// 		Room rmP = rooms.FirstOrDefault(x => x.Center.X == currentEdge.P.X && x.Center.Y == currentEdge.P.Y);
	// 		visitedRooms.Add(rmQ.Id);
	// 		visitedRooms.Add(rmP.Id);
	// 		usedEdges.Add(currentEdge);
	// 		remainingEdges.Remove(currentEdge);
	// 		currentEdge = remainingEdges.FirstOrDefault(e => e.Q.X == currentEdge.Q.X && e.Q.Y == currentEdge.Q.Y || e.P.X == currentEdge.P.X && e.P.Y == currentEdge.P.Y);

	// 		i++;

	// 	}
	// }

	private bool CanRemoveEdge(IEdge edge)
	{
		bool canRemove = false;



		Room rmQ = rooms.FirstOrDefault(x => x.Center.X == edge.Q.X && x.Center.Y == edge.Q.Y);

		Room rmP = rooms.FirstOrDefault(x => x.Center.X == edge.P.X && x.Center.Y == edge.P.Y);

		if (GetEdgeCount(rmP) > 1 && GetEdgeCount(rmQ) > 1) { canRemove = true; }

		if (!canRemove)
		{
			GD.Print("Saved this edge: ", edge.Q, edge.P);
		}

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

			if (qMatch || pMatch)
			{
				output++;
			}

		}


		return output;
	}
	// private void CheckForRedundantEdges(int roomId)
	// {

	// 	Vector2I roomCenter = rooms.FirstOrDefault(x => x.Id == roomId).Center;
	// 	float cx = roomCenter.X;
	// 	float cy = roomCenter.Y;
	// 	List<Room> neighbors = new();

	// 	List<IEdge> myEdges = goodEdges.Where(edge => edge.P.X == cx && edge.P.Y == cy || edge.Q.X == cx && edge.Q.Y == cy).ToList();
	// 	List<IEdge> myLatEdges = new();
	// 	GD.Print("myEdges ", myEdges.Count);
	// 	foreach (IEdge edge in goodEdges)
	// 	{
	// 		foreach (IEdge myEdge in myEdges)
	// 		{

	// 			bool qMatch = edge.Q.X == myEdge.Q.X && edge.Q.Y == myEdge.Q.Y;
	// 			bool pMatch = edge.P.X == myEdge.P.X && edge.P.Y == myEdge.P.Y;
	// 			if (qMatch)
	// 			{

	// 				foreach (IEdge myEdge2 in myEdges)
	// 				{
	// 					if (myEdge2 != myEdge)
	// 					{
	// 						bool pMatch2 = edge.P.X == myEdge2.P.X && edge.P.Y == myEdge2.P.Y;
	// 						if (pMatch2)
	// 						{
	// 							myLatEdges.Add(edge);
	// 						}
	// 					}
	// 				}
	// 			}
	// 			else if (pMatch)
	// 			{
	// 				foreach (IEdge myEdge2 in myEdges)
	// 				{
	// 					if (myEdge2 != myEdge)
	// 					{
	// 						bool qMatch2 = edge.Q.X == myEdge2.Q.X && edge.Q.Y == myEdge2.Q.Y;
	// 						if (qMatch2)
	// 						{
	// 							myLatEdges.Add(edge);
	// 						}
	// 					}
	// 				}
	// 			}
	// 		}

	// 	}
	// 	GD.Print("myLatEdges ", myLatEdges.Count);


	// }


	private void AddOrUpdateCount(Dictionary<int, int> dictionary, int id)
	{
		if (dictionary.ContainsKey(id))
		{
			// If the id is already in the dictionary, increment the count
			dictionary[id]++;
		}
		else
		{
			// If the id is not in the dictionary, add it with a count of 1
			dictionary[id] = 1;
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
