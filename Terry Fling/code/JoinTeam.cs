using Sandbox;
using System;

public sealed class JoinTeam : Component, Component.ITriggerListener
{
	[Property] private GameObject self {get; set;}
	[Property] public GameObject redSpawn {get;set;}
	[Property] public GameObject blueSpawn {get;set;}

[Broadcast]
public void OnTriggerEnter(Collider other )
	{
		var player = other.GameObject;
		CharacterController control = player.Components.Get<CharacterController>();
		Highlight highlight = player.Components.GetInChildrenOrSelf<Highlight>();
		var mr = other.Components.GetInChildrenOrSelf<ModelRenderer>();
		Log.Info(player);

		if(self.Tags.Has("red")) {
			player.Tags.Add("red");
			player.Transform.Position = redSpawn.Transform.Position;
			control.Velocity = Vector3.Zero;

		}
		else if (self.Tags.Has("blue")) {
			player.Tags.Add("blue");
		
			player.Transform.Position = blueSpawn.Transform.Position;
			control.Velocity = Vector3.Zero;


		}
	}

}