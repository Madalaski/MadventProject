using Godot;
using System;
using System.Collections.Generic;

public partial class IKSolver : Node3D
{
	[Export]
	public int chainLength = 1;

	[Export]
	public int iterations = 1;

	[Export]
	public Node3D target;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (target == null)
			return;

		List<Node3D> nodes = new List<Node3D>();
		List<Vector3> positions = new List<Vector3>();
		List<float> lengths = new List<float>();
		float totalLength = 0f;

		Node3D currentNode = this;
		for (int i = 0; i < chainLength; i++)
		{
			Node3D parentNode = currentNode.GetParent<Node3D>();
			nodes.Add(currentNode);
			positions.Add(currentNode.GlobalPosition);
			float length = (parentNode.GlobalPosition - currentNode.GlobalPosition).Length();
			lengths.Add(length);
			totalLength += length;
			currentNode = parentNode;
		}
		nodes.Add(currentNode);
		positions.Add(currentNode.GlobalPosition);
		Vector3 startPosition = currentNode.GlobalPosition;
		Vector3 endPosition = target.GlobalPosition;

		if (startPosition.DistanceTo(endPosition) < totalLength)
		{
			for (int j = 0; j < iterations; j++)
			{
				positions[0] = endPosition;

				// Forward Pass
				for (int i = 0; i < chainLength; i++)
				{
					Vector3 n = (positions[i+1] - positions[i]).Normalized();
					positions[i+1] = positions[i] + n * lengths[i];
				}

				positions.Reverse();
				lengths.Reverse();
				positions[0] = startPosition;

				// Back Pass
				for (int i = 0; i < chainLength; i++)
				{
					Vector3 n = (positions[i+1] - positions[i]).Normalized();
					positions[i+1] = positions[i] + n * lengths[i];
				}

				positions.Reverse();
				lengths.Reverse();
			}
		}
		else
		{
			positions.Reverse();
			lengths.Reverse();
			Vector3 n = (endPosition - startPosition).Normalized();
			for (int i = 1; i < chainLength + 1; i++)
			{
				positions[i] = positions[i-1] + n * lengths[i-1];
			}
			positions.Reverse();
			lengths.Reverse();
		}

		nodes.Reverse();
		positions.Reverse();

		for (int i = 0; i < chainLength; i++)
		{
			Vector3 currentDir = (nodes[i+1].GlobalPosition - nodes[i].GlobalPosition).Normalized();
			Vector3 newDir = (positions[i+1] - positions[i]).Normalized();
			Quaternion q = new Quaternion(currentDir, newDir);
			Transform3D t = nodes[i].GlobalTransform;
			t.Basis = new Basis(q) * t.Basis;
			nodes[i].GlobalTransform = t;
		}
	}
}
