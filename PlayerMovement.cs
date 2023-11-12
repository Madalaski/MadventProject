using Godot;
using System;
using System.Collections.Generic;

public enum PlayerState : int
{
	GROUNDED,
	AIRBORNE,
	CLIMBING
}

public partial class PlayerMovement : Node
{

	[Export]
	public float[] MovementForces = new float[3]{20f, 15f, 5f};

	[Export]
	public float[] DragCoefficients = new float[3]{5f, 5f, 5f};

	[Export]
	public Camera3D camera;

	private PlayerState state = PlayerState.AIRBORNE;

	private RigidBody3D player;

	private PlayerClimbing climbing;

	private Vector2 up = Vector2.Zero;
	private Vector2 down = Vector2.Zero;
	private Vector2 left = Vector2.Zero;
	private Vector2 right = Vector2.Zero;
	private bool jumpPressed = false;
	private bool climbingInput = false;

	[Export]
	public bool grounded = false;

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
		Vector3 right = up.Cross(forward);
		Vector3 newForward = right.Cross(up);
		return (newForward*input.Y) + (right*input.X);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		player = GetParent<RigidBody3D>();
		climbing = (PlayerClimbing)GetParent().FindChild("PlayerClimbing");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var spaceState = player.GetWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create(player.GlobalTransform.Origin, player.GlobalTransform.Origin - new Vector3(0f, 1.5f, 0f));
		query.Exclude.Add(player.GetRid());
		var result = spaceState.IntersectRay(query);

		grounded = result.Count > 0;

		if (grounded)
		{
			state = PlayerState.GROUNDED;
		}
		else
		{
			state = PlayerState.AIRBORNE;
		}

		if (climbingInput && climbing.climbAvailable)
		{
			state = PlayerState.CLIMBING;
		}

		if (state != PlayerState.CLIMBING)
		{
			player.GravityScale = 1f;
		}
		else
		{
			player.GravityScale = 0f;

			player.ApplyForce(climbing.forceDirection * 30f);

		}

		Vector2 input = up + down + left + right;
		input = NormalizeInput(input);

		if (input.LengthSquared() > 0.01)
		{
			Vector3 direction = camera.GlobalTransform.Basis.Z;
			direction.Y = 0;
			direction = direction.Normalized();

			if (state != PlayerState.CLIMBING)
			{
				Vector3 playerMovementOnGround = ConvertInputToAxis(input * MovementForces[(int)state], direction, Vector3.Up);
				player.ApplyForce(playerMovementOnGround);
			}
			else
			{
				Vector3 normalToClimbingPlane = -climbing.forceDirection;

				normalToClimbingPlane.Y = Mathf.Abs(normalToClimbingPlane.Y);

				Vector3 playerMovementOnSurface = ConvertInputToAxis(input * MovementForces[(int)state], camera.GlobalTransform.Basis.Z, normalToClimbingPlane);

				player.ApplyForce(playerMovementOnSurface);
			}
			
		}
		
		Vector3 drag = -player.LinearVelocity;

		if (state != PlayerState.CLIMBING)
			drag.Y = 0;

		player.ApplyForce(drag * DragCoefficients[(int)state]);

		

		if (jumpPressed)
		{
			if (state != PlayerState.AIRBORNE)
			{
				Vector3 jumpDirection = Vector3.Up;

				if (state == PlayerState.CLIMBING)
					jumpDirection = -climbing.forceDirection * 2.5f;

				player.SetAxisVelocity(jumpDirection * 8.0f);
			}

			jumpPressed = false;
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

		if (@event.IsActionPressed("Climbing")) climbingInput = true;
		else if (@event.IsActionReleased("Climbing")) climbingInput = false;
	}
}
