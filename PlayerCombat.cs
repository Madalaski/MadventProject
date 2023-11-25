using Godot;
using System;

public partial class PlayerCombat : Node
{
	[Export]
	public BigController bigController;
	public WeakPoint currentWeakPoint;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			switch (mouseEvent.ButtonIndex)
			{
				case MouseButton.Left:
					if (currentWeakPoint != null)
					{
						bigController.Rpc("NotifyPlayerDealtDamage", currentWeakPoint.Damage);
						currentWeakPoint.QueueFree();
					}
					break;
			}
		}
	}
}
