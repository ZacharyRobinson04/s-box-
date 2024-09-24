using System;
using Sandbox;
using Sandbox.Citizen;

public sealed class PlayerControl : Component, Punchable
{
	//Movement properities
	[Property, Group ("Movement Properties")] public float Friction { get; set; } = 4.0f;
	[Property, Group ("Movement Properties")] public float AirControl { get; set; } = 0.2f; 
	[Property, Group ("Movement Properties")] public float MaxForce { get; set; } = 50f; 
	[Property, Group ("Movement Properties")] public float Speed { get; set; } = 290f;
	[Property, Group ("Movement Properties")] public float WalkSpeed { get; set; } = 100f;
	[Property, Group ("Movement Properties")] public float CrouchSpeed { get; set; } = 90f;
	[Property, Group ("Movement Properties")] public float JumpForce { get; set; } = 400f;
	[Property, Group ("Fling Control"), Range( 0, 1000 )] public float FlingSpeedMultiplier { get; set; } = 800f;
	[Property, Group ("Fling Control"), Range( 0, 1000 )] public float MaxCharge { get; set; } = 1f;
	[Property, Group ("Fling Control"), Range( 0, 0.01f )] public float ChargeSpeed { get; set; } = 0.02f;
	[Property, Group ("Punching")] public float punchHeight { get; set; } = 20f;
	[Property, Group ("Punching")] public float punchStrength { get; set; } = 300f;



	//Object References
	[Property, Group ("References")] private GameObject Head { get; set; }
		[Property, Group ("References")] private GameObject Player { get; set; }

	[Property, Group ("References")] private GameObject Body { get; set; }
	[Property, Group ("References")] private GameObject cam { get; set; }
	//Member Variables
	public bool topCheck = false;
	public bool botCheck = false;
	private float chargeAmount = 0f;
	private CharacterController control;
	private CitizenAnimationHelper CitizenAnimationHelper;
	private bool refling = true;

	//Synced variables
	[Sync]private Vector3 WishVelocity {get; set;}
	[Sync] private Angles targetAngle {get; set;}
	[Sync] private Rotation eyeAngle {get; set;}
	[Sync] private float rotationDifference {get; set;}
	[Sync] private bool isCrouching {get; set;}
	[Sync] private bool isAiming {get; set;}
	[Sync] private  int punchCooldown {get; set;} = 0;





	protected override void OnAwake()
	{
		CitizenAnimationHelper = Components.Get<CitizenAnimationHelper>();
		control = Components.Get<CharacterController>();

	}
	protected override void OnUpdate()
	{
		RotateBody();
		UpdateAnimations();	
		SendPunch();
		//Set sprinting and crouching states
		isCrouching = Input.Down( "Duck" );
		isAiming = Input.Down( "Attack2" );
		}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy ) return;
		BuildWishVelocity();
		Move();
		ChargeFling();

		if (punchCooldown > 0) {
			punchCooldown -= 1;
		}

	}

	void BuildWishVelocity()
	{
		WishVelocity = 0;

		var rot = Head.Transform.Rotation;
		if ( Input.Down( "Forward" ) ) WishVelocity += rot.Forward;
		if ( Input.Down( "Backward" ) ) WishVelocity += rot.Backward;
		if ( Input.Down( "Left" ) ) WishVelocity += rot.Left;
		if (Input.Down("Right")) WishVelocity += rot.Right;
		if(Input.Pressed("Jump") && control.IsOnGround) Jump();

		WishVelocity = WishVelocity.WithZ( 0 );
		if ( !WishVelocity.IsNearZeroLength ) 
		{
			WishVelocity = WishVelocity.Normal;
		}

		if ( isCrouching )
		{
			WishVelocity *= CrouchSpeed;
		}
		else if (isAiming){
			WishVelocity *= WalkSpeed;
		}
		else WishVelocity *= Speed;
	}

[Broadcast]
	void SendPunch()
	{
		if (cam is null) {
			return;
		}
		
		if (IsProxy) {
			return;
		}
		if (!Input.Pressed("Attack1") || punchCooldown > 0) {
			return;
		}
		punchCooldown = 15;
	Punchable punchable = null;
	var camPos = cam.Transform.Position;
	var eyeAngles = Head.Transform.Rotation.Angles();

	CitizenAnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Swing;
	CitizenAnimationHelper.TriggerDeploy();

	//Set Raytrace for whether you can punch a player
	var trace = Scene.Trace.Ray(Player.Transform.Position + (0), camPos + (cam.Transform.Rotation.Forward * 230f))
		.IgnoreGameObjectHierarchy( GameObject.Root )
		.UsePhysicsWorld()
		.UseHitboxes()
		.Run();

		if(trace.Hit) {
			punchable = trace.Component.Components.GetInAncestorsOrSelf<Punchable>();
			} 
		if(punchable is not null) {
		float yDir = (float)((float)Math.Sin(eyeAngles.yaw * Math.PI/180) * (float)Math.Cos(eyeAngles.pitch * Math.PI/180));
		float xDir = (float)((float)Math.Cos(eyeAngles.pitch * Math.PI/180) * (float)Math.Cos(eyeAngles.yaw * Math.PI/180));
		float zDir = (float)Math.Sin(eyeAngles.pitch * Math.PI/180);
		Vector3 punchDir = new Vector3(xDir,yDir,0);
			punchable.PunchMe(punchDir, punchStrength, punchHeight);
		}
		
	}

[Broadcast]
		public void PunchMe(Vector3 punchDir, float punchStrength, float punchHeight) {
		control.Punch(punchDir  * punchStrength);
		control.Punch(Vector3.Up * punchHeight);
		punchCooldown = 15;
	}

	void Move()
	{
		//Get Gravity from scene
		var gravity = Scene.PhysicsWorld.Gravity;

		if(control.IsOnGround)
		{

			control.Velocity = control.Velocity.WithZ( 0 );
			control.Accelerate( WishVelocity );
			control.ApplyFriction( Friction );

		} else
		{
			//Apply Gravity
			control.Velocity += gravity * Time.Delta * 0.5f;
			control.Accelerate( WishVelocity * AirControl );
		}

		//Move the character controller
		control.Move();

		if (!control.IsOnGround)
		{
			control.Velocity += gravity * Time.Delta * 0.5f;
		} else
		{
			control.Velocity = control.Velocity.WithZ( 0 );
			refling = true;
		}
	}

	void RotateBody()
	{
		if (Body is null) {
			return;
		}
		targetAngle = new Angles(0,Head.Transform.Rotation.Yaw(), 0).ToRotation();
		rotationDifference = Body.Transform.Rotation.Distance( targetAngle );
			if (rotationDifference > 90f ||control.Velocity.Length > 10f)
			{
				Body.Transform.Rotation = Rotation.Lerp(Body.Transform.Rotation, targetAngle, Time.Delta *10f);
			}
	}

	void UpdateAnimations()
	{
		eyeAngle = Head.Transform.Rotation;
		CitizenAnimationHelper.WithWishVelocity( WishVelocity );
		CitizenAnimationHelper.WithVelocity (control.Velocity); 
		CitizenAnimationHelper.AimAngle = eyeAngle;
		CitizenAnimationHelper.IsGrounded = control.IsOnGround;
		CitizenAnimationHelper.WithLook(eyeAngle.Forward, 1f, 1f, 1f);
		 if(isAiming) 
		 {
			CitizenAnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Walk;
		 } 
		 else 
		 {
			CitizenAnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Run;
		 }

		CitizenAnimationHelper.DuckLevel = isCrouching ? 1f : 0f;

		if (punchCooldown > 0) {
				CitizenAnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Punch;

		} else {
				CitizenAnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
		}
	}


	void Fling(float chargeAmount)
	{
		//Get angle of Aim
		var eyeAngles = Head.Transform.Rotation.Angles();
		float yDir = (float)((float)Math.Sin(eyeAngles.yaw * Math.PI/180) * (float)Math.Cos(eyeAngles.pitch * Math.PI/180));
		float xDir = (float)((float)Math.Cos(eyeAngles.pitch * Math.PI/180) * (float)Math.Cos(eyeAngles.yaw * Math.PI/180));
		float zDir = (float)Math.Sin(eyeAngles.pitch * Math.PI/180);
		Vector3 flingDir = new Vector3(xDir,yDir,-1*zDir);
		control.Punch(flingDir * FlingSpeedMultiplier * chargeAmount);
	}
	void Jump() 
	{
		control.Punch(Vector3.Up * JumpForce);
		CitizenAnimationHelper?.TriggerJump();

		
	}

	void ChargeFling() 
	{

		//Spaghetti Code.. Fix this later
		if (Input.Down("Attack2") && chargeAmount < MaxCharge) {
			chargeAmount += ChargeSpeed;
		}

		if (Input.Released("Attack2") && !isCrouching && refling) {
			Fling(chargeAmount);
			chargeAmount = 0f;
			refling = false;
			
		} 
		else if (Input.Released("Attack2") || (isCrouching && control.IsOnGround)) {
			chargeAmount = 0f;
		}
		else if (Input.Released("Attack2")) {
			chargeAmount = 0f;
		}
	}



	public float GetFlingAmount()
	{
		return chargeAmount;
	}
		public float GetMaxCharge()
	{
		return MaxCharge;
	}

}
