using Sandbox;

public sealed class HoopTrigger : Component, Component.ITriggerListener
{
	[Property] private GameObject self {get; set;}
	[Property] public Score scoreKeep {get;set;}
public void OnTriggerEnter(Collider other) 
{
	if (IsProxy) return;
	if (scoreKeep.redPoints >= scoreKeep.maxPoints || scoreKeep.bluePoints >= scoreKeep.maxPoints) {
		return;
	}
	var player = other.Components.Get<PlayerControl>();

	if (player is null) {
		return;
	}
	if (self.Tags.Has("top") && player.botCheck == false) 
	{
	if (IsProxy) return;
		Log.Info(player);
		player.topCheck = true;
		Log.Info(player.topCheck);
	}
		if (self.Tags.Has("bot") && player.botCheck == false) 
	{
			Log.Info("BOT");
		player.botCheck = true;
		if (player.topCheck == true)
		{
			Score();
		}
		player.topCheck = false;
	}
	
}

public void OnTriggerExit(Collider other)
{
	if (IsProxy) return;
	var player = other.Components.Get<PlayerControl>();
	
	if (player is null) {
		return;
	}
	Log.Info("Resetting triggers");
	player.topCheck = false;
	player.botCheck = false;
}

public void Score()
{
	if (IsProxy) return;
	if (self.Tags.Has("blue")) {
		scoreKeep.RedScores();
	}
	if (self.Tags.Has("red")) {
		scoreKeep.BlueScores();
	}
}

}