using Godot;
using Godot.Collections;
using System;

public partial class BigController : Node
{
	[Export]
	public bool isServer = false;

	[Export]
	float legStaminaCost = 0.3f;

	[Export]
	float armStaminaCost = 0.4f;

	[Export]
	float legHeight = 4f;

	[Export]
	float legSpeed = 5f;

	[Export]
	float armSpeed = 7f;

	[Export]
	PackedScene damageZone;

	[Export]
	Node3D colossus;

	[Export]
	Node3D leftLegT;

	[Export]
	Node3D rightLegT;

	[Export]
	Node3D leftArmT;

	[Export]
	Node3D rightArmT;

	Stamina stamina;

	private Vector3 oldLeftLegPosition;
	private Vector3 oldRightLegPosition;
	private Vector3 oldArmPosition;

	private Vector3 leftLegPosition;
	private Vector3 rightLegPosition;
	private Vector3 armPosition;

	private bool leftLegMoving = false;
	private bool rightLegMoving = false;
	private bool armMoving = false;

	private double leftLegMoveStart = 0.0;
	private double rightLegMoveStart = 0.0;
	private double armMoveStart = 0.0;

	private float leftLegSpeedMod = 1f;
	private float rightLegSpeedMod = 1f;
	private float armSpeedMod = 1f;

	private Node3D armNode = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (isServer)
		{
			stamina = GetNode<Stamina>("Stamina");
		}

		CallDeferred("MakeMultiplayer");

		leftLegPosition = leftLegT.GlobalPosition;
		rightLegPosition = rightLegT.GlobalPosition;
	}

	public void MakeMultiplayer()
	{
		if (isServer)
		{
			ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
			peer.CreateServer(8910);
			Multiplayer.MultiplayerPeer = peer;
		}
		else
		{
			ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
			peer.CreateClient("127.0.0.1", 8910);
			Multiplayer.MultiplayerPeer = peer;
		}

		Dictionary dict = new Dictionary();
		dict["rpc_mode"] = (int)MultiplayerApi.RpcMode.Authority;
		dict["transfer_mode"] = (int)MultiplayerPeer.TransferModeEnum.Reliable;
		dict["call_local"] = true;

		RpcConfig("MoveLeftLeg", dict);
		RpcConfig("MoveRightLeg", dict);
		RpcConfig("MoveArmNode", dict);

		Dictionary playerSide = new Dictionary();
		playerSide["rpc_mode"] = (int)MultiplayerApi.RpcMode.AnyPeer;
		playerSide["transfer_mode"] = (int)MultiplayerPeer.TransferModeEnum.Unreliable;
		playerSide["call_local"] = false;
		RpcConfig("NotifyPlayerPosition", playerSide);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (leftLegMoving)
		{
			float totalTime = leftLegPosition.DistanceTo(oldLeftLegPosition) / (legSpeed * leftLegSpeedMod);
			float weight = (float)((Time.GetUnixTimeFromSystem() - leftLegMoveStart) / totalTime);

			if (weight <= 1f)
			{
				Vector3 legPosition = leftLegT.GlobalPosition;
				legPosition = oldLeftLegPosition.Lerp(leftLegPosition, weight);
				legPosition.Y = Mathf.Sin(Mathf.Pi * weight) * legHeight;

				leftLegT.GlobalPosition = legPosition;
			}
			else
			{
				leftLegT.GlobalPosition = leftLegPosition;
				leftLegMoving = false;
				leftLegSpeedMod = 1f;

				DamageZone newDamageZone = damageZone.Instantiate<DamageZone>();
				newDamageZone.Basis = newDamageZone.Basis.Scaled(Vector3.One * 10f);
				newDamageZone.Position = leftLegPosition;
				AddChild(newDamageZone);
			}
		}

		if (rightLegMoving)
		{
			float totalTime = rightLegPosition.DistanceTo(oldRightLegPosition) / (legSpeed * rightLegSpeedMod);
			float weight = (float)((Time.GetUnixTimeFromSystem() - rightLegMoveStart) / totalTime);

			if (weight < 1f)
			{
				Vector3 legPosition = rightLegT.GlobalPosition;
				legPosition = oldRightLegPosition.Lerp(rightLegPosition, weight);
				legPosition.Y = Mathf.Sin(Mathf.Pi * weight) * legHeight;

				rightLegT.GlobalPosition = legPosition;
			}
			else
			{
				rightLegT.GlobalPosition = rightLegPosition;
				rightLegMoving = false;
				rightLegSpeedMod = 1f;

				DamageZone newDamageZone = damageZone.Instantiate<DamageZone>();
				newDamageZone.Basis = newDamageZone.Basis.Scaled(Vector3.One * 10f);
				newDamageZone.Position = rightLegPosition;
				AddChild(newDamageZone);
			}
		}

		if (armMoving)
		{
			float totalTime = armPosition.DistanceTo(oldArmPosition) / (armSpeed * armSpeedMod);
			float weight = (float)((Time.GetUnixTimeFromSystem() - armMoveStart) / totalTime);

			if (weight < 1f)
			{
				armNode.GlobalPosition = oldArmPosition.Lerp(armPosition, weight);;
			}
			else
			{
				armNode.GlobalPosition = armPosition;
				armNode = null;
				armMoving = false;
				armSpeedMod = 1f;

				DamageZone newDamageZone = damageZone.Instantiate<DamageZone>();
				newDamageZone.Basis = newDamageZone.Basis.Scaled(Vector3.One * 10f);
				newDamageZone.Position = armPosition;
				AddChild(newDamageZone);
			}
		}

		Vector3 cPos = colossus.GlobalPosition;
		Vector3 rightToLeft = leftLegT.GlobalPosition - rightLegT.GlobalPosition;
		rightToLeft.Y = 0;
		Vector3 rightOrigin = rightLegT.GlobalPosition; rightOrigin.Y = colossus.GlobalPosition.Y;

		Vector3 newForward = rightToLeft.Normalized().Cross(Vector3.Up);
		Quaternion q = new Quaternion(colossus.Basis.Z, newForward);

		Vector3 localRightArm = colossus.ToLocal(rightArmT.GlobalPosition);
		Vector3 localLeftArm = colossus.ToLocal(leftArmT.GlobalPosition);

		colossus.Basis = new Basis(q) * colossus.Basis;
		colossus.GlobalPosition = rightOrigin + (0.5f * rightToLeft);

		rightArmT.GlobalPosition = colossus.ToGlobal(localRightArm);
		leftArmT.GlobalPosition = colossus.ToGlobal(localLeftArm);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("Jump"))
		{
			if (leftLegMoving && stamina.TryUseStamina(legStaminaCost))
			{
				Rpc("MoveLeftLeg", leftLegPosition, 3f);
			}

			if (rightLegMoving && stamina.TryUseStamina(legStaminaCost))
			{
				Rpc("MoveRightLeg", rightLegPosition, 3f);
			}

			if (armMoving && stamina.TryUseStamina(armStaminaCost))
			{
				Rpc("MoveArmNode", armPosition, 3f);
			}
		}
	}

	public void MoveLeftLeg(Vector3 newPosition, float speedModifier = 1f)
	{
		oldLeftLegPosition = leftLegT.GlobalPosition;
		leftLegPosition = newPosition;
		leftLegSpeedMod = speedModifier;
		leftLegMoving = true;
		leftLegMoveStart = Time.GetUnixTimeFromSystem();
	}

	public void MoveRightLeg(Vector3 newPosition, float speedModifier = 1f)
	{
		oldRightLegPosition = rightLegT.GlobalPosition;
		rightLegPosition = newPosition;
		rightLegSpeedMod = speedModifier;
		rightLegMoving = true;
		rightLegMoveStart = Time.GetUnixTimeFromSystem();
	}

	public void MoveArmNode(NodePath path, Vector3 newPosition, float speedModifier = 1f)
	{
		armNode = GetNode<Node3D>(path);
		oldArmPosition = armNode.GlobalPosition;
		armPosition = newPosition;
		armSpeedMod = speedModifier;
		armMoving = true;
		armMoveStart = Time.GetUnixTimeFromSystem();
	}

	public void NotifyPlayerPosition(Vector3 playerPosition)
	{
		if (isServer)
		{
			DamageZone newDamageZone = damageZone.Instantiate<DamageZone>();
			newDamageZone.Basis = newDamageZone.Basis.Scaled(Vector3.One * 4f);
			newDamageZone.Lifetime = 4.0;
			newDamageZone.ScaleSpeed = 2f;
			newDamageZone.Position = playerPosition;
			AddChild(newDamageZone);
		}
	}
}
