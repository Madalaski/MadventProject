using Godot;
using System;

public partial class Health : Node
{
	[Export]
	public bool showDebug = true;

	[Export]
	public string debugName = "Health";

	public int CurrentHealth = 100;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (showDebug)
		{
			DebugDraw2D.SetText(debugName, CurrentHealth);
		}
	}

	public void Damage(int amount)
	{
		CurrentHealth -= amount;
	}
}
