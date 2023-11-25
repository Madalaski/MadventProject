using Godot;
using System;

public partial class ColossusMovement : Node
{
	private BigController colossus;
	private Node3D movingNode = null;
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

	public void StartMovingNode(Node3D node)
	{
		movingNode = node;
		CreateDummyNode(node.GlobalPosition);
	}

	public void UpdateMovingNode(Vector3 globalPosition)
	{
		DummyNode.GlobalPosition = globalPosition;
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
