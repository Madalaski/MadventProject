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
	Area3D leftArmDetector;

	[Export]
	Area3D rightArmDetector;

	[Export]
	float BaseCameraDistance = 10f;

	private Node3D movingNode = null;
	private Node3D DummyNode = null;

	private float movingDistance = 0f;

	private Vector2 mouseMotion = Vector2.Zero;
	private Vector2 mousePosition = Vector2.Zero;

	private bool leftClickDown = false;
	private bool rightClickDown = false;

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

		if (movingNode != null)
		{
			Vector3 origin = Camera.ProjectRayOrigin(mousePosition);
			Vector3 newPosition = origin + (Camera.ProjectRayNormal(mousePosition) * movingDistance);
			DummyNode.GlobalPosition = newPosition;

			if (leftClickDown)
			{
				if (colossus != null)
				{
					colossus.Rpc("MoveArmNode", movingNode.GetPath(), DummyNode.GlobalPosition, 1f);
				}

				DummyNode.QueueFree();
				DummyNode = null;
				movingNode = null;
				leftClickDown = false;
			}
		}

		// Handle Input
		if (movingNode == null && floor != null && colossus != null && Input.MouseMode != Input.MouseModeEnum.Captured)
		{
			var spaceState = floor.GetWorld3D().DirectSpaceState;
			var from = Camera.ProjectRayOrigin(mousePosition);
			var to = from + Camera.ProjectRayNormal(mousePosition) * 1000f;
			var query = PhysicsRayQueryParameters3D.Create(from, to);
			query.CollideWithAreas = true;
			var result = spaceState.IntersectRay(query);

			if (result.Count > 0)
			{
				Node3D collider = result["collider"].As<Node3D>();
				if (collider == floor)
				{
					if (leftClickDown)
					{
						colossus.Rpc("MoveLeftLeg", result["position"].As<Vector3>(), 1f);
					}
					
					if (rightClickDown)
					{
						colossus.Rpc("MoveRightLeg", result["position"].As<Vector3>(), 1f);
					}
				}
				else if (leftArmDetector != null && rightArmDetector != null)
				{
					if (collider == leftArmDetector)
					{
						leftArmDetector.GetParent().GetChild<Node3D>(0).Show();
						if (leftClickDown)
						{
							movingNode = leftArmDetector.GetParent<Node3D>();
							CreateDummyNode(movingNode.GlobalPosition);
							movingDistance = from.DistanceTo(movingNode.GlobalPosition);
						}
					}
					else
					{
						leftArmDetector.GetParent().GetChild<Node3D>(0).Hide();
					}

					if (collider == rightArmDetector)
					{
						rightArmDetector.GetParent().GetChild<Node3D>(0).Show();
						if (leftClickDown)
						{
							movingNode = rightArmDetector.GetParent<Node3D>();
							CreateDummyNode(movingNode.GlobalPosition);
							movingDistance = from.DistanceTo(movingNode.GlobalPosition);
						}
					}
					else
					{
						rightArmDetector.GetParent().GetChild<Node3D>(0).Hide();
					}
				}

			}
		}

		leftClickDown = false;
		rightClickDown = false;
	}

	public override void _Input(InputEvent @event)
	{
		
		if (@event is InputEventMouseMotion mouseMotionEvent)
		{
			mousePosition = mouseMotionEvent.Position;
			mouseMotion += mouseMotionEvent.Relative;
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
					leftClickDown = true;
					break;
				case MouseButton.Right:
					rightClickDown = true;
					break;
			}
		}

		if (@event.IsActionPressed("Escape"))
		{
			Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
			mouseMotion = Vector2.Zero;
		}
	}

	private void CreateDummyNode(Vector3 position)
	{
		MeshInstance3D dummyNode = new MeshInstance3D();
		SphereMesh mesh = new SphereMesh();
		mesh.Radius = 5f;
		mesh.Height = 10f;
		dummyNode.Mesh = mesh;
		DummyNode = dummyNode;
		AddChild(dummyNode);
	}
}
