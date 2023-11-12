using Godot;
using System;
using System.ComponentModel.DataAnnotations;

public partial class CameraFollow : Node
{
	[Export]
	BigController colossus;

	[Export]
	Camera3D Camera;

	[Export]
	Node3D CameraParent;

	[Export]
	Node3D FollowTarget;

	[Export]
	StaticBody3D floor;

	[Export]
	float BaseCameraDistance = 10f;

	private Vector2 mouseMotion = Vector2.Zero;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float CameraDistance = BaseCameraDistance;

		if (floor != null)
		{
			var spaceState = floor.GetWorld3D().DirectSpaceState;
			var query = PhysicsRayQueryParameters3D.Create(CameraParent.GlobalPosition, CameraParent.ToGlobal(new Vector3(0f, 0f, CameraDistance)));
			var result = spaceState.IntersectRay(query);

			if (result.Count > 0)
			{
				if (result["collider"].As<Node3D>() == floor)
				{
					Vector3 hitPos = result["position"].As<Vector3>();
					float newLength = hitPos.DistanceTo(CameraParent.GlobalPosition);
					newLength -= 1f;
					CameraDistance = Mathf.Min(CameraDistance, newLength);
				}
			}
		}

		if (Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			CameraParent.Rotate(Vector3.Up, -mouseMotion.X * 0.001f);
			CameraParent.Rotate(CameraParent.Transform.Basis.X, -mouseMotion.Y * 0.001f);
			CameraParent.Transform = CameraParent.Transform.Orthonormalized();
		}
		
		mouseMotion = Vector2.Zero;

		if (Camera != null && CameraParent != null && FollowTarget != null)
		{
			Vector3 CamPos = CameraParent.GlobalPosition;
			Vector3 TargetPos = FollowTarget.GlobalPosition;
			Transform3D CamPT = CameraParent.GlobalTransform;
			CamPT.Origin = CamPos.Lerp(TargetPos, (float)delta * 5f);

			CameraParent.GlobalTransform = CamPT;

			Transform3D CamT = Camera.Transform;
			CamT.Origin = CamT.Origin.Lerp(new Vector3(0f, 0f, CameraDistance), (float)delta * 5f);
			Camera.Transform = CamT;
		}
	}

	public override void _Input(InputEvent @event)
	{
		
		if (@event is InputEventMouseMotion)
		{
			mouseMotion += ((InputEventMouseMotion)@event).Relative;
		}

		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			switch (mouseEvent.ButtonIndex)
			{
				case MouseButton.WheelUp:
					BaseCameraDistance -= 0.5f;
					break;
				case MouseButton.WheelDown:
					BaseCameraDistance += 0.5f;
					break;
				case MouseButton.Left:
				{
					if (floor != null && colossus != null && Input.MouseMode != Input.MouseModeEnum.Captured)
					{
						var spaceState = floor.GetWorld3D().DirectSpaceState;
						var from = Camera.ProjectRayOrigin(mouseEvent.Position);
        				var to = from + Camera.ProjectRayNormal(mouseEvent.Position) * 1000f;
						var query = PhysicsRayQueryParameters3D.Create(from, to);
						var result = spaceState.IntersectRay(query);

						if (result.Count > 0)
						{
							if (result["collider"].As<Node3D>() == floor)
							{
								colossus.MoveLeftLeg(result["position"].As<Vector3>());
							}
						}
					}
				} break;
			}
		}

		if (@event.IsActionPressed("Escape"))
		{
			Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
			mouseMotion = Vector2.Zero;
		}
	}
}
