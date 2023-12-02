using Godot;
using System;

public enum LimbType : int
{
	INVALID = 0,
	LEFT_ARM = 1,
	RIGHT_ARM = 2,
	ARM = LEFT_ARM | RIGHT_ARM,
	LEFT_LEG = 4,
	RIGHT_LEG = 8,
	LEG = LEFT_LEG | RIGHT_LEG,
	CHEST = 16
}

public partial class LimbIKTarget : Node3D
{
	[Export]
	public LimbType type;

	[Export]
	public float DistanceRestraint = -1f;

	[Export]
	public Node3D DistanceRestraintNode;

	[Export]
	public bool freezeX;

	[Export]
	public bool freezeY;

	[Export]
	public bool freezeZ;

	public bool shouldShow = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (GetChildCount() > 0)
		{
			if (shouldShow)
			{
				GetChild<Node3D>(0).Show();
			}
			else 
			{
				GetChild<Node3D>(0).Hide();
			}
		}
		shouldShow = false;
	}

	public bool IsLeg()
	{
		return (type & LimbType.LEG) != 0;
	}

	public bool IsArm()
	{
		return (type & LimbType.ARM) != 0;
	}
}
