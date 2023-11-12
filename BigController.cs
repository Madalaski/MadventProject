using Godot;
using System;

public partial class BigController : Node
{
	[Export]
	float legHeight = 10f;

	[Export]
	float legSpeed = 4f;

	[Export]
	Node3D colossus;

	[Export]
	Node3D leftLegT;

	[Export]
	Node3D rightLegT;

	[Export]
	Node3D leftArmT;

	[Export]
	Node3D rightArmT;

	private Vector3 oldLeftLegPosition;
	private Vector3 oldRightLeftPosition;

	private Vector3 leftLegPosition;
	private Vector3 rightLegPosition;

	private bool leftLegMoving = false;
	private bool rightLegMoving = false;

	private float leftLegMoveStart = 0f;
	private float rightLegMoveStart = 0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		leftLegPosition = leftLegT.GlobalPosition;
		rightLegPosition = rightLegT.GlobalPosition;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (leftLegMoving)
		{
			float totalTime = legSpeed / leftLegPosition.DistanceTo(oldLeftLegPosition);
			float weight = (((float)Time.GetTicksMsec() * 1000f) - leftLegMoveStart) / totalTime;

			if (weight < 1f)
			{
				Vector3 legPosition = leftLegT.GlobalPosition;
				legPosition = oldLeftLegPosition.Lerp(leftLegPosition, weight);
				legPosition.Y = Mathf.Sin(Mathf.Pi * weight) * legHeight;

				leftLegT.GlobalPosition = legPosition;
			}
			else
			{
				leftLegT.GlobalPosition = leftLegPosition;
				leftLegMoving = false;
			}
		}

		if (leftLegMoving || rightLegMoving)
		{
			Vector3 cPos = colossus.GlobalPosition;
			Vector3 xzPos = leftLegT.GlobalPosition.Lerp(rightLegT.GlobalPosition, 0.5f);
			cPos.X = xzPos.X; cPos.Z = xzPos.Z;
			colossus.GlobalPosition = cPos;
		}
	}

	public void MoveLeftLeg(Vector3 newPosition)
	{
		leftLegPosition = newPosition;
		leftLegMoving = true;
		leftLegMoveStart = (float)Time.GetTicksMsec() * 1000f;
	}
}
