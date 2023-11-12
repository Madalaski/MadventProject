using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerClimbing : Node
{
	public List<ForceArea> forceAreas;

	public Node3D body;

	public bool climbAvailable = false;
	public Vector3 forceDirection = Vector3.Zero;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		body = GetParent<Node3D>();
		forceAreas = new List<ForceArea>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector3 averageDirection = Vector3.Zero;
		climbAvailable = false;

		if (forceAreas.Count > 0)
		{
			foreach (ForceArea area in forceAreas)
			{
				if (area.type == AreaType.SPHERE)
				{
					averageDirection += (area.node.GlobalTransform.Origin - body.GlobalTransform.Origin).Normalized();
				}
				else if (area.type == AreaType.CAPSULE)
				{
					Vector3 playerLocalPos = area.node.ToLocal(body.GlobalTransform.Origin);
					Vector3 localDirection = Vector3.Zero;
					if (Mathf.Abs(playerLocalPos.Y) < area.height - area.radius)
					{
						localDirection = -new Vector3(playerLocalPos.X, 0, playerLocalPos.Z).Normalized();
					}
					else
					{
						localDirection = -new Vector3(playerLocalPos.X, playerLocalPos.Y - (Mathf.Sign(playerLocalPos.Y) * (area.height - area.radius)), playerLocalPos.Z).Normalized();
					}
					averageDirection += (area.node.GlobalBasis * localDirection).Normalized();
				}
			}

			averageDirection *= 1f / forceAreas.Count;
			climbAvailable = true;
		}
		forceDirection = averageDirection.Normalized();
	}
}
