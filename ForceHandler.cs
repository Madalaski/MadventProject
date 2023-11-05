using Godot;
using System;

public partial class ForceHandler : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_area_3d_body_entered(Node3D body)
	{
		if (body.GetChildCount() < 3)
			return;

		PlayerMovement pm = body.GetChild<PlayerMovement>(2);
		if (pm != null)
		{
			pm.forcePositions.Add(GetParent<Node3D>().Transform.Origin);
		}
	}

	public void _on_area_3d_body_exited(Node3D body)
	{
		if (body.GetChildCount() < 3)
			return;

		PlayerMovement pm = body.GetChild<PlayerMovement>(2);
		if (pm != null)
		{
			pm.forcePositions.Remove(GetParent<Node3D>().Transform.Origin);
		}
	}
}
