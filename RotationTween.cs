using Godot;
using System;

public partial class RotationTween : Node3D
{
	[Export]
	Vector3 firstRot;

	[Export]
	Vector3 secondRot;

	[Export]
	float totalTime = 3f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//firstRot.Origin = Transform.Origin;
		//secondRot.Origin = Transform.Origin;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float weight = Mathf.PosMod((float)Time.GetTicksMsec() / 1000f, totalTime * 2f) / (totalTime * 2f);
		weight -= 0.5f;
		weight = 0.5f + Mathf.Abs(weight);

		Transform3D first = Transform.LookingAt(Transform.Origin + firstRot);
		Transform3D second = Transform.LookingAt(Transform.Origin + secondRot);

		Transform = first.InterpolateWith(second, weight);
	}
}
