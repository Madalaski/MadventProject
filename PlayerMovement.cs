using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerMovement : Node
{

	[Export]
	public float MovementForce = 20.0f;

	[Export]
	public float DragCoefficient = 0.2f;

	[Export]
	public RigidBody3D player;

	[Export]
	public Camera3D camera;

	public List<Vector3> forcePositions;

	private Vector2 up = Vector2.Zero;
	private Vector2 down = Vector2.Zero;
	private Vector2 left = Vector2.Zero;
	private Vector2 right = Vector2.Zero;
	private bool jumpPressed = false;
	private bool climbing = false;

	public Vector2 NormalizeInput(Vector2 input)
	{
		if (input.LengthSquared() > 1.0f)
		{
			return input.Normalized();
		}

		return input;
	}

	public Vector3 ConvertInputToAxis(Vector2 input, Vector3 forward, Vector3 up)
	{
		Vector3 right = -forward.Cross(up);
		return (forward*input.Y) + (right*input.X);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		forcePositions = new List<Vector3>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (forcePositions.Count == 0 || !climbing)
		{
			player.GravityScale = 1f;
		}
		else
		{
			player.GravityScale = 0f;

			Vector3 averageForce = Vector3.Zero;

			foreach (Vector3 forcePosition in forcePositions)
			{
				Vector3 direction = (forcePosition - player.Transform.Origin).Normalized();
				averageForce += direction;
			}

			averageForce *= (1f / forcePositions.Count);

			player.ApplyForce(averageForce * 20f);

		}

		Vector2 input = up + down + left + right;
		input = NormalizeInput(input);

		if (input.LengthSquared() > 0.01)
		{
			Vector3 direction = camera.GlobalTransform.Basis.Z;
			direction.Y = 0;
			direction = direction.Normalized();

			player.ApplyForce(ConvertInputToAxis(input * MovementForce, direction, camera.GlobalTransform.Basis.Y));
		}
		else
		{
			Vector3 drag = -player.LinearVelocity;

			if (forcePositions.Count == 0 || !climbing)
				drag.Y = 0;

			player.ApplyForce(drag * DragCoefficient);
		}

		if (jumpPressed)
		{
			jumpPressed = false;
			player.SetAxisVelocity(Vector3.Up * 5.0f);
		}
		
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("Up"))
		{
			up = Vector2.Up;
		}
		else if (@event.IsActionReleased("Up"))
		{
			up = Vector2.Zero;
		}

		if (@event.IsActionPressed("Down"))
		{
			down = Vector2.Down;
		}
		else if (@event.IsActionReleased("Down"))
		{
			down = Vector2.Zero;
		}

		if (@event.IsActionPressed("Left"))
		{
			left = Vector2.Left;
		}
		else if (@event.IsActionReleased("Left"))
		{
			left = Vector2.Zero;
		}

		if (@event.IsActionPressed("Right"))
		{
			right = Vector2.Right;
		}
		else if (@event.IsActionReleased("Right"))
		{
			right = Vector2.Zero;
		}

		if (@event.IsActionPressed("Jump"))
		{
			jumpPressed = true;
		}

		if (@event.IsAction("Climbing"))
		{
			climbing = @event.IsActionPressed("Climbing");
		}
	}
}
