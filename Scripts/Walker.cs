using Godot;
using System;

using System.Collections.Generic;


public partial class Walker : Node
{
	private List<Vector2I> DIRECTIONS = new List<Vector2I>
	{
	Vector2I.Right,
	Vector2I.Up,
	Vector2I.Left,
	Vector2I.Down
	};
	Rect2 borders;
	Random rand = new Random();

	[Export]
	public int dirChangeChance = 30;

	[Export]
	public float maxLinearSteps = 4;
	private float stepsSinceTurn = 0;

	private List<Vector2I> StepHistory = new List<Vector2I>();

	private Vector2I pos;

	public Walker(Vector2I startingPos, Rect2 borders)
	{
		pos = startingPos;
		this.borders = borders;

	}
	public override void _Ready()
	{

	}




	public override void _Process(double delta)
	{


	}

	private bool Step(Vector2I direction)
	{
		if (!borders.HasPoint(pos + direction)) { return false; }

		StepHistory.Add(pos);
		pos += direction;
		return true;
	}
	private Vector2I ChangeDirection(Vector2I currentDirection)
	{
		// GD.Print("Changing Direction");
		List<Vector2I> directions = new List<Vector2I>(DIRECTIONS);

		directions.Remove(currentDirection);

		Vector2I nextDirection = directions[rand.Next(directions.Count)];
		bool foundSuitableDirection = false;
		while (directions.Count > 0 && !foundSuitableDirection)
		{
			if (borders.HasPoint(pos + nextDirection))
			{
				foundSuitableDirection = true;
			}
			else
			{
				directions.Remove(nextDirection);
				nextDirection = directions[rand.Next(directions.Count)];
			}
		}
		stepsSinceTurn = 0;
		return nextDirection;
	}

	public List<Vector2I> Walk(float steps)
	{
		StepHistory.Add(pos);

		float stepsTaken = 0;
		Vector2I direction = DIRECTIONS[rand.Next(DIRECTIONS.Count)];


		while (stepsTaken < steps)
		{
			

			if (stepsSinceTurn >= 4 || rand.Next(100) < dirChangeChance)
			{
				direction = ChangeDirection(direction);

			}


			bool didStep = Step(direction);

			if (didStep)
			{
				stepsTaken++;
				stepsSinceTurn++;
			}
			else
			{
				direction = ChangeDirection(direction);

			}


		}

		return StepHistory;

	}
}
