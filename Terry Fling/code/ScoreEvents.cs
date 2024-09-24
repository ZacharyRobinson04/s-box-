using Sandbox;
using System;

public sealed class ScoreEvents : Component, Component.INetworkListener
{
	[Property] Score Score {get; set;}
	[Property] GameObject Player {get; set;}
	[Property] CharacterController control {get; set;}
	[Property] float defaultBounciness {get; set;} =0.62f;
	[Property] HighlightOutline highlight {get; set;}
	public Vector3 spawnPoint;


	private int ResetBounciness = 0;
	protected override void OnAwake()
	{
		if (IsProxy) return;
		Score.OnScore += PunchAll;
		Score.OnGameEnd += EndGame;
		spawnPoint = Player.Transform.Position;
	}

	protected override void OnFixedUpdate()
	{
		UpdateTeams();
		if (IsProxy) return;
		BounceCheck();
	}

	public void BounceCheck()
	{
		if (ResetBounciness > 0) {
			if (IsProxy) return;
			ResetBounciness-= 1;
		} else {
			control.Bounciness = defaultBounciness;
		}
	}
	public void PunchAll(object sender, EventArgs e)
	{
		if (IsProxy) return;
		Log.Info("Recieving Punch");
			if (Player.Tags.Has("red")) {
				control.Bounciness = 0f;
				control.Velocity = new Vector3(0,0,0);
				control.Punch(new Vector3(00,-2000,2110));
				ResetBounciness = 120;
			}
			if (Player.Tags.Has("blue")) {
				control.Bounciness = 0f;
				control.Velocity = new Vector3(0,0,0);
				ResetBounciness = 120;
				control.Punch(new Vector3(00,2000,2110));
			}
	}

	public void EndGame(object sender, EventArgs e)
	{
		if (IsProxy) return;
			control.Velocity = Vector3.Zero;
			Player.Tags.RemoveAll();
			Player.Tags.Add("player");
			Player.Transform.Position = spawnPoint;
	}

	[Broadcast]
	public void UpdateTeams()
	{
		highlight.Enabled = false;
		if (Player.Tags.Has("red")){
			// Log.Info("I AM RED");
			highlight.Enabled = true;
			highlight.Color = new Color(1.00f, 0.00f, 0.00f, 1.00f);
		} else if (Player.Tags.Has("blue")){
			// Log.Info("I AM BLUE");
			highlight.Enabled = true;
			highlight.Color = new Color(0f, 0f, 1f, 1f);
		}
	}
}