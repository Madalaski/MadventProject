using Godot;
using System;

public partial class PositionTween : Node3D
{
	[Export]
	public Vector3 relativePosition;

	[Export]
	public float timeTaken = 1f;

	private Vector3 start;
	private Vector3 end;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		start = Transform.Origin;
		end = Transform.Origin + relativePosition;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float time = Mathf.PosMod((float)Time.GetTicksMsec() / 1000f, timeTaken) / timeTaken;
		float weight = Mathf.Sin(2f * Mathf.Pi * time);
		weight += 1f;
		weight *= 0.5f;

		Transform3D transform = Transform;
		transform.Origin = start.Lerp(end, weight);
		Transform = transform;
	}
}
