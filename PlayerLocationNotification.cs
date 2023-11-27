using Godot;
using System;

public partial class PlayerLocationNotification : Node3D
{
	public double Lifetime = 1f;

	public float ScaleSpeed = 0.001f;
	private double startTime = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		startTime = Time.GetUnixTimeFromSystem();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Basis = Basis.Scaled(Vector3.One + (Vector3.One * (float)delta * ScaleSpeed));

		//DebugDraw3D.DrawSphere(GlobalPosition, GlobalBasis.Scale.X, new Color(0.8f, 0f, 0f));

		if (Time.GetUnixTimeFromSystem() > startTime + Lifetime)
		{
			QueueFree();
		}
	}
}
