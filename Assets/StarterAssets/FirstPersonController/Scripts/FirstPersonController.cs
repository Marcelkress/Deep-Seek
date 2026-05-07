using UnityEngine;
using System;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.Events;
#endif

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{
		public enum SurgeState
		{
			None,
			Charge,
			Ignition,
			Sustain,
			Cooldown
		}

		[Serializable]
		public struct StepImpactData
		{
			public float pulse;
			public float planarSpeed;
			public float downwardSpeed;
			public bool wasLanding;
		}

		public event Action<StepImpactData> StepLanded;
		public event Action<SurgeState> SurgeStateChanged;

		[Header("Movement Speeds")] [Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 2.2f;

		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 3.6f;

		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;

		[Header("Mech Inertia")] [Tooltip("Forward acceleration in m/s²")]
		public float ForwardAcceleration = 2.4f;

		[Tooltip("Reverse acceleration in m/s²")]
		public float ReverseAcceleration = 1.0f;

		[Tooltip("Lateral acceleration in m/s²")]
		public float LateralAcceleration = 1.5f;

		[Tooltip("Deceleration when no input is provided")]
		public float BrakeDeceleration = 0.9f;

		[Tooltip("Extra deceleration when changing to opposite direction")]
		public float DirectionChangeDragBoost = 1.6f;

		[Header("Underwater Resistance")] [Tooltip("Forward drag applied to planar velocity")]
		public float WaterDragForward = 1.2f;

		[Tooltip("Lateral drag applied to planar velocity")]
		public float WaterDragLateral = 2.0f;

		[Tooltip("Max world-space drift speed caused by ocean current")]
		public float CurrentDriftAmplitude = 0.2f;

		[Tooltip("How quickly drift direction changes over time")]
		public float CurrentDriftFrequency = 0.06f;

		[Tooltip("How quickly current drift blends to its target value")]
		public float CurrentDriftResponsiveness = 0.6f;

		[Tooltip("Multiplier applied to current drift while grounded")]
		public float GroundedDriftMultiplier = 0.1f;

		[Header("Ground Traction")] [Tooltip("How strongly the mech adheres to desired velocity while grounded")]
		public float GroundTraction = 16f;

		[Tooltip("Aggressive stop deceleration at low/medium speed")]
		public float GroundStopDeceleration = 12f;

		[Tooltip("Deceleration used when stopping from top speed to allow slight slide")]
		public float HighSpeedSlideDeceleration = 3.2f;

		[Tooltip("Speed above which stopping allows slight slide")]
		public float HighSpeedSlideThreshold = 3.8f;

		[Header("Step Feedback")] [Tooltip("Distance between step pulses while grounded")]
		public float StepDistance = 2.2f;

		[Tooltip("Minimum horizontal speed required to generate step pulses")]
		public float StepMinSpeed = 0.9f;

		[Tooltip("How much landing speed contributes to step pulse")]
		public float LandingPulseScale = 0.12f;

		[Header("Surge")] public float SurgeChargeDuration = 0.16f;
		public float SurgeIgnitionDuration = 0.10f;
		public float SurgeSustainDuration = 1.15f;
		public float SurgeCooldownDuration = 0.55f;

		[Tooltip("Speed multiplier while surge sustain is active")]
		public float SurgeSustainSpeedMultiplier = 1.35f;

		[Tooltip("Forward impulse applied during surge ignition")]
		public float SurgeIgnitionImpulse = 1.4f;

		[Space(10)] [Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;

		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;

		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;

		[Tooltip("Useful for rough ground")] public float GroundedOffset = -0.14f;

		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;

		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;

		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;

		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		public bool isMoving => _input.move.sqrMagnitude > 0.01f;
		// cinemachine
		private float _cinemachineTargetPitch;

		// player
		private float _rotationVelocity;
		private float _verticalVelocity;
		private readonly float _terminalVelocity = 53.0f;
		private Vector3 _planarVelocity;
		private Vector3 _currentDriftVelocity;
		private float _distanceSinceStep;
		private bool _wasGroundedLastFrame;
		private bool _wasSprintingLastFrame;
		private float _surgeTimer;
		private bool _surgeIgnitionImpulseApplied;
		private SurgeState _surgeState;
		private FootstepSystem footstepSystem;
		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

		[HideInInspector] public bool canMove;
		
		//Wwise
		[Header("Wwise")] 
		//public AK.Wwise.Event Footsteps;
		public AK.Wwise.Event LandingSound;
		public AK.Wwise.Event JumpVoice;
		public AK.Wwise.Event HitVoice;

#if ENABLE_INPUT_SYSTEM
		private PlayerInput _playerInput;
#endif
		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;

		private const float _threshold = 0.01f;

		private bool IsCurrentDeviceMouse
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
			}
		}

		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
			footstepSystem = GetComponent<FootstepSystem>();
#if ENABLE_INPUT_SYSTEM
			_playerInput = GetComponent<PlayerInput>();
			Isjumpping = false;
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
			_wasGroundedLastFrame = Grounded;
			SetSurgeState(SurgeState.None);
		}

		private void Update()
		{
			GroundedCheck();
			UpdateSurgeState(Time.deltaTime);
			UpdateCurrentDrift(Time.deltaTime);
			JumpAndGravity();
			
			
			if(canMove)
				Move();
			
			HandleLandingPulse();
			UpdateStepCycle(Time.deltaTime);

		}

		private void LateUpdate()
		{
			if(canMove)
				    CameraRotation();
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
				transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
				QueryTriggerInteraction.Ignore);
		}

		private void CameraRotation()
		{
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

				_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		private void Move()
		{
			float dt = Time.deltaTime;
			Vector2 input = _input.move;
			float inputMagnitude = _input.analogMovement
				? Mathf.Clamp01(input.magnitude)
				: (input == Vector2.zero ? 0f : 1f);

			Vector3 desiredDirection = Vector3.zero;
			if (inputMagnitude > _threshold)
			{

				desiredDirection = (transform.right * input.x + transform.forward * input.y).normalized;
			}

			float targetSpeed = GetTargetSpeed(inputMagnitude);
			Vector3 desiredPlanarVelocity = desiredDirection * targetSpeed;

			AccelerateTowardDesiredVelocity(desiredPlanarVelocity, dt);
			ApplyWaterDrag(dt);
			ApplyGroundTraction(desiredPlanarVelocity, inputMagnitude, dt);

			Vector3 finalPlanarVelocity = _planarVelocity + GetEffectiveCurrentDrift();
			Vector3 verticalMove = new Vector3(0.0f, _verticalVelocity, 0.0f);
			_controller.Move((finalPlanarVelocity + verticalMove) * dt);

		}

		private void ApplyGroundTraction(Vector3 desiredPlanarVelocity, float inputMagnitude, float dt)
		{
			if (!Grounded)
			{
				return;
			}

			if (inputMagnitude > _threshold)
			{
				_planarVelocity = Vector3.MoveTowards(_planarVelocity, desiredPlanarVelocity, GroundTraction * dt);
				return;
			}

			float currentSpeed = new Vector3(_planarVelocity.x, 0f, _planarVelocity.z).magnitude;
			float stopDeceleration = currentSpeed >= HighSpeedSlideThreshold
				? HighSpeedSlideDeceleration
				: GroundStopDeceleration;

			_planarVelocity = Vector3.MoveTowards(_planarVelocity, Vector3.zero, stopDeceleration * dt);
		}

		private Vector3 GetEffectiveCurrentDrift()
		{
			return Grounded ? _currentDriftVelocity * GroundedDriftMultiplier : _currentDriftVelocity;
		}

		private void AccelerateTowardDesiredVelocity(Vector3 desiredVelocity, float dt)
		{
			Vector3 current = _planarVelocity;

			if (desiredVelocity.sqrMagnitude <= _threshold)
			{
				_planarVelocity = Vector3.MoveTowards(current, Vector3.zero, BrakeDeceleration * dt);
				return;
			}

			float alignment = current.sqrMagnitude > _threshold
				? Vector3.Dot(current.normalized, desiredVelocity.normalized)
				: 1f;

			float accelRate = ForwardAcceleration;

			Vector3 desiredLocal = transform.InverseTransformDirection(desiredVelocity);
			if (desiredLocal.z < -0.01f)
			{
				accelRate = ReverseAcceleration;
			}
			else if (Mathf.Abs(desiredLocal.x) > Mathf.Abs(desiredLocal.z))
			{
				accelRate = LateralAcceleration;
			}

			if (alignment < -0.15f)
			{
				accelRate *= DirectionChangeDragBoost;
			}

			_planarVelocity = Vector3.MoveTowards(current, desiredVelocity, accelRate * dt);
		}

		private void ApplyWaterDrag(float dt)
		{
			if (_planarVelocity.sqrMagnitude <= _threshold)
			{
				return;
			}

			Vector3 localVelocity = transform.InverseTransformDirection(_planarVelocity);
			localVelocity.x = Mathf.MoveTowards(localVelocity.x, 0f, WaterDragLateral * dt);
			localVelocity.z = Mathf.MoveTowards(localVelocity.z, 0f, WaterDragForward * dt);
			_planarVelocity = transform.TransformDirection(localVelocity);
		}

		private float GetTargetSpeed(float inputMagnitude)
		{
			if (inputMagnitude <= _threshold)
			{
				return 0f;
			}

			float baseSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
			float surgeMultiplier = GetSurgeSpeedMultiplier();
			return baseSpeed * inputMagnitude * surgeMultiplier;
		}

		private void UpdateCurrentDrift(float dt)
		{
			float t = Time.time * CurrentDriftFrequency;
			float x = Mathf.PerlinNoise(t, 37.71f) - 0.5f;
			float z = Mathf.PerlinNoise(91.43f, t) - 0.5f;
			Vector3 targetDrift = new Vector3(x, 0f, z) * (CurrentDriftAmplitude * 2f);

			float blend = 1f - Mathf.Exp(-CurrentDriftResponsiveness * dt);
			_currentDriftVelocity = Vector3.Lerp(_currentDriftVelocity, targetDrift, blend);
		}

		private void UpdateStepCycle(float dt)
		{
			if (!Grounded)
			{
				_distanceSinceStep = 0f;
				return;
			}

			float planarSpeed = new Vector3(_planarVelocity.x, 0f, _planarVelocity.z).magnitude;
			if (planarSpeed < StepMinSpeed)
			{
				_distanceSinceStep = 0f;
				return;
			}

			_distanceSinceStep += planarSpeed * dt;
			if (_distanceSinceStep < StepDistance)
			{
				return;
			}

			_distanceSinceStep -= StepDistance;
			float normalizedSpeed =
				Mathf.InverseLerp(StepMinSpeed, SprintSpeed * SurgeSustainSpeedMultiplier, planarSpeed);
			RaiseStepEvent(GetStepPulse(normalizedSpeed, 0f), planarSpeed, 0f, false);
			
		}

		private void HandleLandingPulse()
		{
			if (Grounded && !_wasGroundedLastFrame)
			{
				float landingSpeed = Mathf.Max(0f, -_verticalVelocity);
				float normalizedLanding = Mathf.Clamp01(landingSpeed * LandingPulseScale);
				float planarSpeed = new Vector3(_planarVelocity.x, 0f, _planarVelocity.z).magnitude;
				RaiseStepEvent(GetStepPulse(0.4f, normalizedLanding), planarSpeed, landingSpeed, true);
				Isjumpping = false;
			}

			_wasGroundedLastFrame = Grounded;
		}

		private float GetStepPulse(float normalizedSpeed, float normalizedLanding)
		{
			float locomotionPulse = Mathf.Lerp(0.35f, 1.0f, Mathf.Clamp01(normalizedSpeed));
			float landingPulse = Mathf.Lerp(0f, 1.25f, Mathf.Clamp01(normalizedLanding));
			return locomotionPulse + landingPulse;
		}

		private void RaiseStepEvent(float pulse, float planarSpeed, float downwardSpeed, bool wasLanding)
		{
			StepImpactData data = new StepImpactData
			{
				pulse = pulse,
				planarSpeed = planarSpeed,
				downwardSpeed = downwardSpeed,
				wasLanding = wasLanding
			};

			StepLanded?.Invoke(data);
		}

		private void UpdateSurgeState(float dt)
		{
			bool hasMoveInput = _input.move.sqrMagnitude > _threshold;
			bool sprintPressed = _input.sprint && hasMoveInput;

			if (sprintPressed && !_wasSprintingLastFrame && _surgeState == SurgeState.None)
			{
				SetSurgeState(SurgeState.Charge);
			}

			_wasSprintingLastFrame = sprintPressed;

			if (_surgeState == SurgeState.None)
			{
				return;
			}

			_surgeTimer += dt;

			switch (_surgeState)
			{
				case SurgeState.Charge:
					if (_surgeTimer >= SurgeChargeDuration)
					{
						SetSurgeState(SurgeState.Ignition);
					}

					break;

				case SurgeState.Ignition:
					if (!_surgeIgnitionImpulseApplied)
					{
						_planarVelocity += transform.forward * SurgeIgnitionImpulse;
						_surgeIgnitionImpulseApplied = true;
					}

					if (_surgeTimer >= SurgeIgnitionDuration)
					{
						SetSurgeState(SurgeState.Sustain);
					}

					break;

				case SurgeState.Sustain:
					if (!sprintPressed || _surgeTimer >= SurgeSustainDuration)
					{
						SetSurgeState(SurgeState.Cooldown);
					}

					break;

				case SurgeState.Cooldown:
					if (_surgeTimer >= SurgeCooldownDuration)
					{
						SetSurgeState(SurgeState.None);
					}

					break;
			}
		}

		private float GetSurgeSpeedMultiplier()
		{
			switch (_surgeState)
			{
				case SurgeState.Charge:
					return 0.9f;
				case SurgeState.Ignition:
					return 1.15f;
				case SurgeState.Sustain:
					return SurgeSustainSpeedMultiplier;
				case SurgeState.Cooldown:
					return 0.95f;
				default:
					return 1f;
			}
		}

		private void SetSurgeState(SurgeState state)
		{
			if (_surgeState == state)
			{
				return;
			}

			_surgeState = state;
			_surgeTimer = 0f;
			_surgeIgnitionImpulseApplied = false;
			SurgeStateChanged?.Invoke(_surgeState);
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
					JumpSound();
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				_input.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(
				new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
				GroundedRadius);
		}

		
		//Wwise Footsteps Logic
		
		//Lytter på Evenet "Steplanded"
		private void OnEnable()
		{
			StepLanded += OnStepLanded;
			
		}
		// Stopper med at Lytte på "Steplanded"
		private void OnDisable()
		{
			StepLanded -= OnStepLanded;
		}
		
		// Method der får data fra StepImpactData structen og bruger waslanding boolen til at finde ud af om vi går eller lander
		private void OnStepLanded(StepImpactData data)
		{
			//Hvis vi lander efter et hop (kommer fra UpdateLandingPulse)
			if (data.wasLanding)
			{
				footstepSystem.PlayFootstep();
				LandingSound.Post(gameObject);
				Debug.Log("Player landed!");
				Isjumpping = false;
			}
			else
			{
				footstepSystem.PlayFootstep();
				// Regular footstep — (kommer fra UpdateStepCycle)
				//Footsteps.Post(gameObject);
			}
		}

		private void JumpSound()

		{

			if (_input.jump && Grounded && !Isjumpping)
			{
				JumpVoice.Post(gameObject);
				Isjumpping = true;
			}
			
		}

		private bool Isjumpping;

		
		
		
	}
	
	
		
	
}