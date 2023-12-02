using Godot;
using System;

public partial class ColossusMovement : Node
{
	private BigController colossus;
	private LimbIKTarget movingNode = null;
	private Node3D DummyNode = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		colossus = GetParent<BigController>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public bool IsMovingNode()
	{
		return movingNode != null;
	}

	public void StartMovingNode(LimbIKTarget node)
	{
		movingNode = node;
		CreateDummyNode(node.GlobalPosition);
	}

	public void UpdateMovingNode(Vector3 globalPosition)
	{
		Vector3 finalPosition = globalPosition;

		if (movingNode.freezeX)
		{
			finalPosition.X = DummyNode.GlobalPosition.X;
		}

		if (movingNode.freezeY)
		{
			finalPosition.Y = DummyNode.GlobalPosition.Y;
		}

		if (movingNode.freezeZ)
		{
			finalPosition.Z = DummyNode.GlobalPosition.Z;
		}

		if (movingNode.DistanceRestraint > 0f && movingNode.DistanceRestraintNode != null)
		{
			if (movingNode.DistanceRestraintNode.GlobalPosition.DistanceTo(finalPosition) > movingNode.DistanceRestraint)
			{
				Vector3 offset = (finalPosition - movingNode.DistanceRestraintNode.GlobalPosition).Normalized();
				finalPosition = movingNode.DistanceRestraintNode.GlobalPosition + offset * movingNode.DistanceRestraint;
			}
		}

		DummyNode.GlobalPosition = finalPosition;
	}

	public void ReleaseMovingNode(bool cancel)
	{
		if (!cancel)
		{
			colossus.Rpc("MoveLimbNode", movingNode.GetPath(), DummyNode.GlobalPosition);
		}
		
		DummyNode.QueueFree();
		DummyNode = null;
		movingNode = null;
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

	public void InitiateBoost()
	{
		colossus.InitiateBoost();
	}
}
