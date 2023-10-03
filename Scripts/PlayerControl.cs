using Godot;
using System;

public partial class PlayerControl : CharacterBody2D
{

	enum LegToMove
	{
		Left,
		Right,
		Neutral
	}



	private Node2D leftLeg;
	private Node2D targetLeftLeg;
	private Node2D rightLeg;
	private Node2D targetRightLeg;

	private Node2D leftLegHome;

	private Node2D rightLegHome;

	private Sprite2D body;

	private float maxLegDistance = 20f;

	private Vector2 direction = Vector2.Zero;
	private int speed = 200;
	private LegToMove ltm = LegToMove.Neutral;

	private bool ltmStateChange = false;
	private bool initialStateSet = false;

	//adjustments to walking to offset home positions to adjust for visual legs walking
	private Vector2 neutralHomePosL = new Vector2(-50, 260);
	private Vector2 neutralHomePosR = new Vector2(50, 260);
	private Vector2 rightHomePosL = new Vector2(0, 260);
	private Vector2 rightHomePosR = new Vector2(100, 260);
	private Vector2 leftHomePosL = new Vector2(-100, 260);
	private Vector2 leftHomePosR = new Vector2(0, 260);

	public override void _Ready()
	{



		//get refs
		leftLeg = GetParent<Node2D>().GetNode<Node2D>("LegLeft");
		targetLeftLeg = GetParent<Node2D>().GetNode<Node2D>("TargetLegLeft");
		leftLegHome = GetChild<Node2D>(0).GetNode<Node2D>("LeftLegHome");

		rightLeg = GetParent<Node2D>().GetNode<Node2D>("LegRight");
		targetRightLeg = GetParent<Node2D>().GetNode<Node2D>("TargetLegRight");
		rightLegHome = GetChild<Node2D>(0).GetNode<Node2D>("RightLegHome");

		body = GetNode<Sprite2D>("Body");

		//set home positions for legs -14, 52

		leftLegHome.Position = neutralHomePosL;
		rightLegHome.Position = neutralHomePosR;

		//set target positions initial val
		targetLeftLeg.Position = leftLegHome.GlobalPosition;
		targetRightLeg.Position = rightLegHome.GlobalPosition;


	}



	public override void _PhysicsProcess(double delta)
	{
		direction = Vector2.Zero;

		if (Input.IsKeyPressed(Key.W))
		{
			direction += new Vector2(0, -1);
		}
		if (Input.IsKeyPressed(Key.A))
		{
			direction += new Vector2(-1, 0);
			body.FlipH = true;
		}
		if (Input.IsKeyPressed(Key.S))
		{
			direction += new Vector2(0, 1);
		}
		if (Input.IsKeyPressed(Key.D))
		{
			direction += new Vector2(1, 0);
			body.FlipH = false;
		}

		// Normalize the direction vector to maintain consistent speed when moving diagonally
		if (direction.Length() > 0)
		{
			direction = direction.Normalized();
		}

		// Move the character based on the direction and speed
		Position += direction * speed * (float)delta;

		HandleLegs(delta);
	}


	float arcHeight = -20f;
	private void HandleLegs(double delta)
	{

		float leftStepDist = targetLeftLeg.Position.DistanceTo(leftLegHome.GlobalPosition);
		float rightStepDist = targetRightLeg.Position.DistanceTo(rightLegHome.GlobalPosition);

		float leftDist = leftLeg.Position.DistanceTo(targetLeftLeg.Position);
		float rightDist = rightLeg.Position.DistanceTo(targetRightLeg.Position);
		// GD.Print(leftStepDist, rightStepDist);
		// GD.Print(ltm);

		if (direction[0] > 0.1)
		{
			leftLegHome.Position = rightHomePosL;
			rightLegHome.Position = rightHomePosR;
		}
		else if (direction[0] < -.1)
		{
			leftLegHome.Position = leftHomePosL;
			rightLegHome.Position = leftHomePosR;
		}
		else
		{
			leftLegHome.Position = neutralHomePosL;
			rightLegHome.Position = neutralHomePosR;
		}

		if ((direction[0] > 0.1 || direction[1] > 0.1) && !initialStateSet)
		{
			ltm = LegToMove.Left;
			initialStateSet = true;
		}
		else if ((direction[0] < -0.1 || direction[1] < -0.1) && !initialStateSet)
		{
			ltm = LegToMove.Right;
			initialStateSet = true;
		}
		else if (direction[0] == 0 && direction[1] == 0)
		{
			ltm = LegToMove.Neutral;
			initialStateSet = false;
		}

		// Calculate the current position of the legs along the parabolic trajectory
		float tL = (float)Mathf.Clamp(leftDist / maxLegDistance, 0.0, 1.0); // Use leftDist or rightDist depending on the leg
		float yOffsetL = Mathf.Sin(tL * Mathf.Pi) * arcHeight;
		Vector2 parabolicOffsetL = new Vector2(0, yOffsetL);

		float tR = (float)Mathf.Clamp(rightDist / maxLegDistance, 0.0, 1.0); // Use leftDist or rightDist depending on the leg
		float yOffsetR = Mathf.Sin(tR * Mathf.Pi) * arcHeight;
		Vector2 parabolicOffsetR = new Vector2(0, yOffsetR);

		// GD.Print(parabolicOffsetL, " ", yOffsetL, " ", tL);



		if (ltm == LegToMove.Left && !ltmStateChange)
		{
			targetLeftLeg.Position = leftLegHome.GlobalPosition + direction * (maxLegDistance - 0.1f);
			ltmStateChange = true;

		}
		else if (ltm == LegToMove.Right && !ltmStateChange)
		{
			targetRightLeg.Position = rightLegHome.GlobalPosition + direction * (maxLegDistance - 0.1f);
			ltmStateChange = true;

		}
		else if (ltm == LegToMove.Neutral)
		{

			ltmStateChange = false;
			initialStateSet = false;

			targetLeftLeg.Position = leftLegHome.GlobalPosition;
			targetRightLeg.Position = rightLegHome.GlobalPosition;
		}

		if (rightStepDist > maxLegDistance)
		{
			ltm = LegToMove.Right;
			ltmStateChange = false;

		}
		if (leftStepDist > maxLegDistance)
		{
			ltm = LegToMove.Left;
			ltmStateChange = false;

		}

		Vector2 leftLerper = new Vector2(Mathf.Lerp(leftLeg.Position.X, targetLeftLeg.Position.X, (float)delta * Mathf.Clamp(leftDist, 0, 10)), Mathf.Lerp(leftLeg.Position.Y, targetLeftLeg.Position.Y, (float)delta * Mathf.Clamp(leftDist, 0, 10)));
		Vector2 rightLerper = new Vector2(Mathf.Lerp(rightLeg.Position.X, targetRightLeg.Position.X, (float)delta * Mathf.Clamp(rightDist, 0, 10)), Mathf.Lerp(rightLeg.Position.Y, targetRightLeg.Position.Y, (float)delta * Mathf.Clamp(rightDist, 0, 10)));


		leftLeg.Position = leftLerper;// - parabolicOffsetL;
		rightLeg.Position = rightLerper;//- parabolicOffsetR;
		Sprite2D s2d = (Sprite2D)leftLeg;
		s2d.Offset = -parabolicOffsetL;

		Sprite2D s2dR = (Sprite2D)rightLeg;
		s2dR.Offset = -parabolicOffsetR;
	}
}
