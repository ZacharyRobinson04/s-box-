using Sandbox;

public sealed class FollowCamRotation : Component
{
	[Property] public GameObject cam {get; set;}
	[Property] public GameObject self {get; set;}

	protected override void OnUpdate()
	{
		self.Transform.Rotation = cam.Transform.Rotation;
	}
}