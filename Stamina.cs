using Godot;
using System;

public partial class Stamina : Node
{
	[Export]
	public float CurrentStamina = 1f;

	[Export]
	public float StaminaRegen = 0.3f;
	[Export]
	public double RegenDelay = 3;

	private double lastStaminaUse = 0f;

	[Export]
	TextureProgressBar progressBar;

	[Export]
	TextureRect textureRect;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//DebugDraw2D.SetText("Stamina", CurrentStamina * 100f);

		if (progressBar != null)
		{
			progressBar.Value = CurrentStamina * 100f;
		}

		if (textureRect != null)
		{
			textureRect.Scale = Vector2.One * CurrentStamina;
		}

		if (CurrentStamina < 1f)
		{
			double currentTime = Time.GetUnixTimeFromSystem();
			if (currentTime > lastStaminaUse + RegenDelay)
			{
				CurrentStamina += StaminaRegen * (float)delta;
			}
		}
		else
		{
			CurrentStamina = 1f;
		}
	}

	public bool TryUseStamina(float amount)
	{
		if (CurrentStamina <= amount)
		{
			return false;
		}
		CurrentStamina -= amount;
		lastStaminaUse = Time.GetUnixTimeFromSystem();
		return true;
	}

	public void UseStamina(float amount)
	{
		if (CurrentStamina <= amount)
		{
			return;
		}

		CurrentStamina -= amount;
		lastStaminaUse = Time.GetUnixTimeFromSystem();
	}
}
