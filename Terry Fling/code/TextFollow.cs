using Sandbox;

public sealed class TextFollow : Component
{
	[Property] GameObject Head {get; set;}
	[Property] GameObject Self {get; set;}
	[Property] GameObject Player {get; set;}


	protected override void OnAwake()
	{
		TextRenderer render = Self.Components.Get<TextRenderer>();
	}
	protected override void OnUpdate()
	{
		Self.Transform.Rotation = Head.Transform.Rotation;
		TextRenderer render = Self.Components.Get<TextRenderer>();
		if (Player.Tags.Has("red")) {
			render.Color = new Color(999999,0,0,1f);
		} else if (Player.Tags.Has("blue")) {
			render.Color = new Color(0,0,999999,1f);
		} else {
			render.Color = new Color(999999,999999,999999,1f);
		}
	}
}