using Godot;
using System;

public partial class CameraFollow : Node
{
	[Export]
	Camera3D Camera;

	[Export]
	Node3D CameraParent;

	[Export]
	Node3D FollowTarget;

	[Export]
	float CameraDistance = 10f;

	private Vector2 mouseMotion = Vector2.Zero;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		CameraParent.Rotate(Vector3.Up, -mouseMotion.X * 0.001f);
		CameraParent.Rotate(CameraParent.Transform.Basis.X, -mouseMotion.Y * 0.001f);
		CameraParent.Transform = CameraParent.Transform.Orthonormalized();
		mouseMotion = Vector2.Zero;

		if (Camera != null && CameraParent != null && FollowTarget != null)
		{
			Vector3 CamPos = CameraParent.Transform.Origin;
			Vector3 TargetPos = FollowTarget.Transform.Origin;
			Transform3D CamPT = CameraParent.Transform;
			CamPT.Origin = CamPos.Lerp(TargetPos, (float)delta * 5f);

			CameraParent.Transform = CamPT;

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

		if (@event is InputEventMouseButton){
			InputEventMouseButton emb = (InputEventMouseButton)@event;
			if (emb.IsPressed()){
				if (emb.ButtonIndex == MouseButton.WheelUp){
					CameraDistance -= 0.1f;
				}
				if (emb.ButtonIndex == MouseButton.WheelUp){
					CameraDistance += 0.1f;;
				}
			}
		}
	}
}
