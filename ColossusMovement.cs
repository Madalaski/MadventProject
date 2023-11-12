using Godot;
using System;

public partial class ColossusMovement : Node
{
	[Export]
	Node3D leftLegT;

	[Export]
	Node3D rightLegT;

	[Export]
	Node3D leftArmT;

	[Export]
	Node3D rightArmT;

	private Vector3 leftLegPosition;
	private Vector3 rightLeftPosition;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
