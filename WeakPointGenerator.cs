using Godot;
using System;

public partial class WeakPointGenerator : Node
{
	[Export]
	PackedScene weakPointScene;

	[Export]
	Node3D colossus;

	[Export]
	float minHeight = 60f;
	
	[Export]
	float maxHeight = 20f;

	[Export]
	float range = 70f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CallDeferred("GenerateWeakPoints");
	}

	public void GenerateWeakPoints()
	{
		var spaceState = colossus.GetWorld3D().DirectSpaceState;
		Random rand = new Random();

		for (int i = 0; i < 10; i++)
		{
			float height = minHeight + ((maxHeight - minHeight) * (float)rand.NextDouble());
			float angle = (float)rand.NextDouble() * Mathf.Pi * 2f;

			Vector3 from = colossus.GlobalPosition + new Vector3(Mathf.Cos(angle) * range, height, Mathf.Sin(angle) * range);
			Vector3 to = colossus.GlobalPosition + new Vector3(0, height, 0);

			DebugDraw3D.DrawLine(from, to, new Color(0f, 0f, 0f), 500000f);
			GD.Print(height);
			GD.Print(angle);

			var query = PhysicsRayQueryParameters3D.Create(from, to);
			var result = spaceState.IntersectRay(query);

			if (result.Count > 0)
			{
				Vector3 position = result["position"].As<Vector3>();
				Node3D body = result["collider"].As<Node3D>();
				WeakPoint newWeakPoint = weakPointScene.Instantiate<WeakPoint>();
				newWeakPoint.Basis = newWeakPoint.Basis.Scaled(Vector3.One * 1f);
				body.AddChild(newWeakPoint);
				newWeakPoint.GlobalPosition = position;

				GD.Print("Hit! Making Weakpoint");
			}
			else
			{
				--i;
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
