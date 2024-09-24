using System.Threading;
using System;
using Sandbox;

public class Score : Component
{
//Properties n' Parameters
[Property] public int maxPoints {get; set;} = 7;
public int endTime{get; set;} = 20;
public int endTimeDef = 20;
//Sync'd variables
[Sync] public int redPoints {get; set;} = 0;
[Sync] public int bluePoints {get; set;} = 0;
//Events
public event EventHandler OnScore;
public event EventHandler OnGameEnd;


protected override void OnAwake()
{
	OnGameEnd += EndGame;
}

[Broadcast]
public void BlueScores()
{
	OnScore?.Invoke(this, EventArgs.Empty);
	if (!IsProxy) bluePoints++;
	if (bluePoints >= maxPoints) {
		OnGameEnd?.Invoke(this, EventArgs.Empty);
		Log.Info("RED WINS");

	} 
}
[Broadcast]

public void RedScores()
{
	OnScore?.Invoke(this, EventArgs.Empty);	
	if (!IsProxy) redPoints++;
	if (redPoints >= maxPoints) {
		OnGameEnd?.Invoke(this, EventArgs.Empty);
		Log.Info("RED WINS");
	}
}
[Broadcast]
public void EndGame(object o, EventArgs e) {
	Log.Info("ENDING GAME PLEASE WORK");
	redPoints = 0;
	bluePoints = 0;

	
}

}