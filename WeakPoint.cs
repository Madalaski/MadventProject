using Godot;
using System;

public partial class WeakPoint : Node3D
{
	public int Damage = 30;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Area3D area = GetNode<Area3D>("Area3D");
		area.BodyEntered += _on_area_3d_body_entered;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		DebugDraw3D.DrawSphere(GlobalPosition, GlobalBasis.Scale.X);
	}

	public void _on_area_3d_body_entered(Node3D body)
	{
		if (body.HasNode("PlayerCombat"))
		{
			PlayerCombat combat = body.GetNode<PlayerCombat>("PlayerCombat");
			combat.currentWeakPoint = this;
		}
	}

	public void DealDamage()
	{

	}
}
