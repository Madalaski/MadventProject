using Godot;
using System;
using System.Collections;

public partial class DamageZone : Node3D
{
	public double Lifetime = 0.1;
	public int Damage = 30;

	public float ScaleSpeed = 0f;

	public float PushForce = 20f;
	private double startTime = 0;

	private Area3D area;
	private GpuParticles3D particles;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		area = GetNode<Area3D>("Area3D");
		area.BodyEntered += _on_area_3d_body_entered;
		particles = GetNode<GpuParticles3D>("Particles");
		particles.Emitting = true;
		startTime = Time.GetUnixTimeFromSystem();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Basis = Basis.Scaled(Vector3.One + (Vector3.One * (float)delta * ScaleSpeed));

		//DebugDraw3D.DrawSphere(GlobalPosition, GlobalBasis.Scale.X);

		if (Time.GetUnixTimeFromSystem() > startTime + Lifetime)
		{
			area.DisableMode = CollisionObject3D.DisableModeEnum.Remove;
			area.ProcessMode = ProcessModeEnum.Disabled;
		}

		if (Time.GetUnixTimeFromSystem() > startTime + particles.Lifetime + 3.0)
		{
			QueueFree();
		}
	}

	public void _on_area_3d_body_entered(Node3D body)
	{
		if (body.HasNode("Health"))
		{
			Health health = body.GetNode<Health>("Health");
			health.Damage(Damage);

			if (body is RigidBody3D rigidBody)
			{
				Vector3 direction = ((rigidBody.GlobalPosition - GlobalPosition) + (Vector3.Up * 3f)).Normalized();
				rigidBody.ApplyImpulse(direction * PushForce);
			}
		}
	}
}
