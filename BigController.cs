using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public struct LimbMovement
{
	public LimbIKTarget nodeToMove;
	public Vector3 startPosition;
	public Vector3 endPosition;

	public double startTime;
	public double endTime;

	public float legHeight = 0f;

	public bool generateDamageZone = false;

	public LimbMovement(LimbIKTarget _nodeToMove, Vector3 _startPosition, Vector3 _endPosition, double _startTime, float _speed)
	{
		nodeToMove = _nodeToMove;
		startPosition = _startPosition;
		endPosition = _endPosition;
		startTime = _startTime;
		endTime = startTime + (double)(startPosition.DistanceTo(endPosition) / _speed);
	}

	public bool IsActive(double currentTime)
	{
		return currentTime >= startTime && currentTime <= endTime;
	}

	public bool HasEnded(double currentTime)
	{
		return currentTime > endTime;
	}

	public Vector3 GetCurrentPosition(double currentTime)
	{
		float weight = (float)((currentTime - startTime) / (endTime - startTime));

		if (legHeight <= 0f)
			return startPosition.Lerp(endPosition, weight);

		Vector3 initial = startPosition.Lerp(endPosition, weight);
		initial.Y += Mathf.Sin(Mathf.Pi * weight) * legHeight;
		return initial;
	}
}

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
	float boostMultiplier = 3f;

	[Export]
	PackedScene damageZone;

	[Export]
	PackedScene playerNotification;

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

	List<LimbMovement> limbMovements;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		limbMovements = new List<LimbMovement>();

		if (isServer)
		{
			stamina = GetNode<Stamina>("Stamina");
		}

		CallDeferred("MakeMultiplayer");
	}

	public void MakeMultiplayer()
	{
		if (isServer)
		{
			GetWindow().Title = "SERVER";

			ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
			peer.CreateServer(8910);
			Multiplayer.MultiplayerPeer = peer;
		}
		else
		{
			GetWindow().Title = "CLIENT";
			
			Script clientPolling = ResourceLoader.Load("res://Scenes/ClientPolling.gd") as Script;
			ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
			string host_ip_address = clientPolling.Get("host_ip_address").AsString();
			GD.Print("Getting static ip address: " + host_ip_address);
			peer.CreateClient(host_ip_address, 8910);
			Multiplayer.MultiplayerPeer = peer;
		}

		Dictionary dict = new Dictionary();
		dict["rpc_mode"] = (int)MultiplayerApi.RpcMode.Authority;
		dict["transfer_mode"] = (int)MultiplayerPeer.TransferModeEnum.Reliable;
		dict["call_local"] = true;
		RpcConfig("MoveLimbNode", dict);

		Dictionary playerSide = new Dictionary();
		playerSide["rpc_mode"] = (int)MultiplayerApi.RpcMode.AnyPeer;
		playerSide["transfer_mode"] = (int)MultiplayerPeer.TransferModeEnum.Unreliable;
		playerSide["call_local"] = false;
		RpcConfig("NotifyPlayerPosition", playerSide);

		Dictionary playerSideLocal = new Dictionary();
		playerSideLocal["rpc_mode"] = (int)MultiplayerApi.RpcMode.AnyPeer;
		playerSideLocal["transfer_mode"] = (int)MultiplayerPeer.TransferModeEnum.Reliable;
		playerSideLocal["call_local"] = true;
		RpcConfig("NotifyPlayerDealtDamage", playerSideLocal);
		RpcConfig("NotifyPlayerDeath", playerSideLocal);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		double currentTime = Time.GetUnixTimeFromSystem();

		for (int i = 0; i < limbMovements.Count; i++)
		{
			if(limbMovements[i].HasEnded(currentTime))
			{
				if (limbMovements[i].generateDamageZone)
				{
					DamageZone newDamageZone = damageZone.Instantiate<DamageZone>();
					//newDamageZone.Basis = newDamageZone.Basis.Scaled(Vector3.One * 10f);
					newDamageZone.Position = limbMovements[i].endPosition;
					AddChild(newDamageZone);
				}

				limbMovements.RemoveAt(i);
				--i;
				continue;
			}

			if (limbMovements[i].IsActive(currentTime))
			{
				limbMovements[i].nodeToMove.GlobalPosition = limbMovements[i].GetCurrentPosition(currentTime);
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
			if (isServer)
			{
				InitiateBoost();
			}
		}
	}

	public void MoveLimbNode(NodePath path, Vector3 newPosition)
	{
		LimbIKTarget nodeToMove = GetNode<LimbIKTarget>(path);

		int prevIndex = limbMovements.FindIndex(
		delegate(LimbMovement movement)
		{
			return movement.nodeToMove == nodeToMove;
		}
		);

		double startTime = Time.GetUnixTimeFromSystem();
		Vector3 startPosition = nodeToMove.GlobalPosition;

		if (prevIndex != -1)
		{
			startTime = limbMovements[prevIndex].endTime;
			startPosition = limbMovements[prevIndex].endPosition;
		}

		LimbMovement movement = new LimbMovement(nodeToMove, startPosition, newPosition, startTime, nodeToMove.IsLeg() ? legSpeed : armSpeed);
		if (nodeToMove.IsLeg())
		{
			movement.legHeight = legHeight;
		}
		
		movement.generateDamageZone = nodeToMove.IsLeg();
		limbMovements.Add(movement);
	}

	public void InitiateBoost()
	{
		List<int> movementsToSpeedUp = new List<int>();

		for (int i = 0; i < limbMovements.Count; i++)
		{
			bool limbFound = false;
			foreach (int index in movementsToSpeedUp)
			{
				if (limbMovements[index].nodeToMove.type == limbMovements[i].nodeToMove.type)
				{
					limbFound = true;
				}
			}

			if (!limbFound)
			{
				movementsToSpeedUp.Add(i);
			}
		}

		float staminaCost = 0f;
		int index_offset = 0;

		double startTime = Time.GetUnixTimeFromSystem();

		foreach (int index in movementsToSpeedUp)
		{
			int newIndex = index + index_offset;
			float speed = 0f;

			if (limbMovements[newIndex].nodeToMove.IsArm())
			{
				staminaCost += armStaminaCost;
				speed = armSpeed;
			}
			else if (limbMovements[newIndex].nodeToMove.IsLeg())
			{
				staminaCost += legStaminaCost;
				speed = legSpeed;
			}

			speed *= boostMultiplier;

			LimbMovement prevMovement = limbMovements[newIndex];
			LimbMovement newMovement = new LimbMovement(
				prevMovement.nodeToMove,
				prevMovement.nodeToMove.GlobalPosition,
				prevMovement.endPosition,
				startTime,
				speed);
			newMovement.generateDamageZone = true;

			prevMovement.endTime = startTime;
			prevMovement.endPosition = newMovement.startPosition;
			limbMovements[newIndex] = prevMovement;
			limbMovements.Insert(newIndex, newMovement);
			++index_offset;
		}

		stamina.UseStamina(staminaCost);
	}

	public void NotifyPlayerPosition(Vector3 playerPosition)
	{
		if (isServer)
		{
			PlayerLocationNotification notification = playerNotification.Instantiate<PlayerLocationNotification>();
			notification.Basis = notification.Basis.Scaled(Vector3.One * 1.2f);
			notification.Lifetime = 4.0;
			notification.ScaleSpeed = 0.5f;
			notification.Position = playerPosition;
			AddChild(notification);
		}
	}

	public void NotifyPlayerDealtDamage(int damage)
	{
		Health health = GetNode<Health>("Health");
		health.Damage(damage);
		if (health.CurrentHealth <= 0)
		{
			GetTree().ChangeSceneToFile("res://main_menu.tscn");
		}
	}

	public void NotifyPlayerDeath()
	{
		GetTree().ChangeSceneToFile("res://main_menu.tscn");
	}
}
