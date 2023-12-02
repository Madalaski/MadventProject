using Godot;
using System;

public partial class VRController : Node
{
	XROrigin3D origin;

	XRController3D leftHand;
	XRController3D rightHand;

	private bool moving;

	private bool scaling;

	private Vector3 prevPos = Vector3.Zero;
	private float prevScale = 0f;

	private Vector3 prevHandOffset = Vector3.One;

	[Export]
	ColossusMovement movement;

	[Export]
	StaticBody3D floor;

	[Export]
	Node3D leftLegT;

	[Export]
	Node3D rightLegT;

	private Vector3 offset_ls = Vector3.Zero;

	private bool prevLeftTrigger = false;
	private bool prevRightTrigger = false;

	private bool prevLeftClick = false;
	private bool prevRightClick = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		origin = GetParent<XROrigin3D>();
		origin.WorldScale = 40f;
		leftHand = GetParent().GetNode<XRController3D>("LeftHand");
		rightHand = GetParent().GetNode<XRController3D>("RightHand");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		bool leftGrip = leftHand.GetFloat("grip") > 0.8f;
		bool rightGrip = rightHand.GetFloat("grip") > 0.8f;

		bool leftTrigger = leftHand.GetFloat("trigger") > 0.8f;
		bool rightTrigger = rightHand.GetFloat("trigger") > 0.8f;

		bool leftClick = leftHand.IsButtonPressed("primary_click");
		bool rightClick = rightHand.IsButtonPressed("primary_click");

		if (!movement.IsMovingNode())
		{
			if ((leftClick && !prevLeftClick) || (rightClick && !prevRightClick))
			{
				movement.InitiateBoost();
			}

			var spaceState = floor.GetWorld3D().DirectSpaceState;
			var from = leftHand.GlobalPosition;
			var to = from + -leftHand.GlobalBasis.Z * 1000f;

			DebugDraw3D.DrawLine(from, to);

			var query = PhysicsRayQueryParameters3D.Create(from, to);
			query.CollideWithAreas = true;
			var result = spaceState.IntersectRay(query);

			if (result.Count > 0)
			{
				Node3D collider = result["collider"].As<Node3D>();

				if (collider.GetParent() is LimbIKTarget limb)
				{
					limb.shouldShow = true;
					if (leftTrigger)
					{
						movement.StartMovingNode(limb);
						offset_ls = leftHand.ToLocal(limb.GlobalPosition);
					}
				}
			}

			from = rightHand.GlobalPosition;
			to = from + -rightHand.GlobalBasis.Z * 1000f;

			DebugDraw3D.DrawLine(from, to);

			query = PhysicsRayQueryParameters3D.Create(from, to);
			query.CollideWithAreas = true;
			var result2 = spaceState.IntersectRay(query);

			if (result2.Count > 0)
			{
				Node3D collider = result2["collider"].As<Node3D>();

				if (collider.GetParent() is LimbIKTarget limb)
				{
					limb.shouldShow = true;
					if (rightTrigger)
					{
						movement.StartMovingNode(limb);
						offset_ls = rightHand.ToLocal(limb.GlobalPosition);
					}
				}
			}
		}
		else
		{
			if (!leftTrigger && !rightTrigger)
			{
				movement.ReleaseMovingNode(false);
			}

			if (leftTrigger)
			{
				movement.UpdateMovingNode(leftHand.ToGlobal(offset_ls));
			}

			if (rightTrigger)
			{
				movement.UpdateMovingNode(rightHand.ToGlobal(offset_ls));
			}
		}

		if (leftGrip && rightGrip)
		{
			if (!scaling)
			{
				prevScale = leftHand.GlobalPosition.DistanceTo(rightHand.GlobalPosition);
				prevHandOffset = rightHand.GlobalPosition - leftHand.GlobalPosition;
			}
			else
			{
				float currentScale = leftHand.GlobalPosition.DistanceTo(rightHand.GlobalPosition);
				origin.WorldScale *= prevScale / currentScale;
				prevScale = currentScale;

				Vector3 handOffset = rightHand.GlobalPosition - leftHand.GlobalPosition;

				Quaternion q = new Quaternion(prevHandOffset, handOffset);
				//origin.GlobalBasis = new Basis(q) * origin.GlobalBasis;

				prevHandOffset = handOffset;
			}

			scaling = true;
			moving = false;
		}
		else if (leftGrip || rightGrip)
		{
			if (!moving)
			{
				if (leftGrip)
				{
					prevPos = leftHand.GlobalPosition;
				}
				else
				{
					prevPos = rightHand.GlobalPosition;
				}
			}
			else
			{
				Vector3 currentPos;
				if (leftGrip)
				{
					currentPos = leftHand.GlobalPosition;
				}
				else
				{
					currentPos = rightHand.GlobalPosition;
				}

				origin.GlobalPosition += prevPos - currentPos;

				if (leftGrip)
				{
					prevPos = leftHand.GlobalPosition;
				}
				else
				{
					prevPos = rightHand.GlobalPosition;
				}
			}

			moving = true;
			scaling = false;
		}
		else
		{
			moving = false;
			scaling = false;
		}

		prevLeftTrigger = leftTrigger;
		prevRightTrigger = rightTrigger;

		prevLeftClick = leftClick;
		prevRightClick = rightClick;
	}

	public void LeftHandInputFloatChanged(String name, float value)
	{
		//GD.Print(name);
	}
}
