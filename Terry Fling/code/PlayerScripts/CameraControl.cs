//I GOT THIS CODE FROM A TUTORIAL BUT IT DROPS FRAMES A LOT.. THERE IS PROBABLY BETTER WAY TO DO ALL THIS
using System;
using System.Xml.Serialization;
using Sandbox;
public sealed class CameraControl : Component
{

	[Property] public PlayerControl Player {get; set;}
	[Property] public GameObject Body { get; set; }
	[Property] public GameObject Head { get; set; }
	[Property] public float camDistance { get; set; }
	[Property] public float defaultCameraFOV { get; set; } = 90f;
	[Property] public float sensitivity {get; set; } = 0.1f;

	//Vars
	private CameraComponent Camera;
	

protected override void OnAwake() 
	{
		Camera = Components.Get<CameraComponent>();
		if ( IsProxy ) Camera.Enabled = false;
	}
	protected override void OnUpdate()
	{
		if ( IsProxy ) Camera.Enabled = false;
		//Rotate Head
		var eyeAngles = Head.Transform.Rotation.Angles();
		eyeAngles.pitch += Input.MouseDelta.y  * sensitivity;
		eyeAngles.yaw -= Input.MouseDelta.x * sensitivity;
		eyeAngles.pitch = eyeAngles.pitch.Clamp(-89.9f, 89.9f);
		Head.Transform.Rotation = Rotation.From(eyeAngles);

		//Camera Position
		if (Camera is not null) {
			// Vector3 camPos = new Vector3(Head.Transform.Position.x -(float)Math.Cos(eyeAngles.yaw * Math.PI/180)*30 ,Head.Transform.Position.y + (float)Math.Cos(eyeAngles.yaw * Math.PI/180)*30,Head.Transform.Position.z);
			var camPos = Head.Transform.Position;
			var camForward = eyeAngles.ToRotation().Forward;
			var camTrace = Scene.Trace.Ray(camPos, camPos - (camForward * camDistance)).WithoutTags("player", "trigger").Run();

			if(camTrace.Hit) {
				camPos = camTrace.HitPosition + camTrace.Normal;
			} else {
				camPos = camTrace.EndPosition;
			}

			//Set cam position
			Camera.Transform.Position = camPos;
			Camera.Transform.Rotation = eyeAngles.ToRotation();
		}

		//Decrease FOV while charging fling
		Camera.FieldOfView = (defaultCameraFOV - (Player.GetFlingAmount()/Player.GetMaxCharge()) * 30);
	}
}