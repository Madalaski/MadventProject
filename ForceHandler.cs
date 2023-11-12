using Godot;
using System;
using System.Runtime.CompilerServices;

public enum AreaType
{
	SPHERE,
	CAPSULE
}

public struct ForceArea
{
	public ForceArea(AreaType _type, Node3D _node, float _radius, float _height)
	{
		type = _type;
		node = _node;
		radius = _radius;
		height = _height;
	}

	public AreaType type;
	public Node3D node;

	public float radius;
	public float height;
}

public partial class ForceHandler : Node
{
	[Export]
	public float Thickness = 2f;

	private Node3D mainNode;

	private ForceArea selfArea;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		mainNode = GetParent<Node3D>();

		if (mainNode == null)
			return;

		Node bodyNode = mainNode.FindChild("Body", false);
		if (bodyNode == null)
			return;

		StaticBody3D body = (StaticBody3D)bodyNode;

		CollisionShape3D collisionShape = body.GetChild<CollisionShape3D>(0);
		Shape3D shape = collisionShape.Shape;

		// Define Self Area
		AreaType areaType = AreaType.SPHERE;
		float radius = 1f;
		float height = 1f;

		if (shape is SphereShape3D sphereShape)
		{
			areaType = AreaType.SPHERE;
			radius = sphereShape.Radius;
		}
		else if (shape is CapsuleShape3D capsuleShape)
		{
			areaType = AreaType.CAPSULE;
			radius = capsuleShape.Radius;
			height = capsuleShape.Height / 2f;
		}

		selfArea = new ForceArea(areaType, collisionShape, radius, height);

		// Create Trigger (Area)
		Area3D triggerArea = new Area3D();
		mainNode.CallDeferred("add_child", triggerArea);
		triggerArea.Transform = body.Transform;

		CollisionShape3D triggerShape = new CollisionShape3D();
		triggerArea.CallDeferred("add_child", triggerShape);
		triggerShape.Transform = collisionShape.Transform;
		
		if (areaType == AreaType.SPHERE)
		{
			SphereShape3D newShape = new SphereShape3D();
			newShape.Radius = radius + Thickness;
			triggerShape.Shape = newShape;
		}
		else
		{
			CapsuleShape3D newShape = new CapsuleShape3D();
			newShape.Radius = radius + Thickness;
			newShape.Height = height * 2f + Thickness;
			triggerShape.Shape = newShape;
		}

		triggerArea.BodyEntered += _on_area_3d_body_entered;
		triggerArea.BodyExited += _on_area_3d_body_exited;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_area_3d_body_entered(Node3D body)
	{
		if (body.GetChildCount() < 4)
			return;

		PlayerClimbing pc = body.GetChild<PlayerClimbing>(3);
		if (pc != null)
		{
			pc.forceAreas.Add(selfArea);
		}
	}

	public void _on_area_3d_body_exited(Node3D body)
	{
		if (body.GetChildCount() < 4)
			return;

		PlayerClimbing pc = body.GetChild<PlayerClimbing>(3);
		if (pc != null)
		{
			pc.forceAreas.Remove(selfArea);
		}
	}
}
