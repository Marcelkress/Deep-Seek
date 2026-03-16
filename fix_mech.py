import os

file_path = "/home/JacobVigso/Documents/GitHub/Deep-Seek/Assets/Jacob/MechMovement.cs"

content = """using UnityEngine;
using StarterAssets;
using System;

public class MechMovement : MonoBehaviour
{
    [Header("Movement (Heavy Feel)")]
    [SerializeField] private float walkSpeed = 4.5f;
    [SerializeField] private float sprintSpeed = 9.5f;
    [SerializeField] private float runAcceleration = 3.5f;
    [SerializeField] private float runDeceleration = 6.0f;
    [SerializeField] private float directionChangeDrag = 2.5f; // Resistance when turning
    
    [Header("Surge (Thruster Lurch)")]
    [SerializeField] private float surgeAccelBurst = 10.0f; // High accel burst when starting sprint
    [SerializeField] private float surgeFadeTime = 0.6f;

    [Header("Underwater Resistance")]
    [SerializeField] private float idleDriftSpeed = 0.15f;
    [SerializeField] private float movingDriftSpeed = 0.05f;
    [SerializeField] private float currentFrequency = 0.8f;

    [Header("Steps")]
    [SerializeField] private float stepsPerSecond = 1.1f;
    [SerializeField] private float stepKickForce = 1.8f; // The physical lurch forward on each step
    [SerializeField] private float stepImpactBase = 0.8f;
    [SerializeField] private float stepImpactBySpeed = 0.9f;
    [SerializeField] private float sprintStepImpactBonus = 0.35f;

    [Header("References")]
    [SerializeField] private MechAudioFeedback audioFeedback;

    private StarterAssetsInputs input;
    private Rigidbody rb;
    private float stepTimer;
    private float surge01;
    private bool wasSprinting;

    public event Action onStepLanded;
    public event Action<float> onStepImpact;
    public event Action<float> onSurgeChanged;

    [HideInInspector] public float stepPhase01 => stepTimer;
    [HideInInspector] public bool hasMoveInput;
    [HideInInspector] public float normalizedSpeed => Mathf.Clamp01(currentSpeed / sprintSpeed);
    [HideInInspector] public float surgeAmount01 => surge01;
    [HideInInspector] public float currentSpeed => new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;

    private void Start()
    {
        input = GetComponent<StarterAssetsInputs>();
        rb = GetComponent<Rigidbody>();

        if (audioFeedback == null) audioFeedback = GetComponentInChildren<MechAudioFeedback>(true);

        if (rb != null)
        {
            // Lowered from 10f. Let our custom deceleration script handle the heavy lifting
            rb.linearDamping = 4f; 
            rb.angularDamping = 4f;
            rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    private void FixedUpdate()
    {
        if (input == null || rb == null) return;

        Vector2 moveInput = input.move;
        Vector3 moveDir = transform.right * moveInput.x + transform.forward * moveInput.y;
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        hasMoveInput = moveDir.sqrMagnitude > 0.0001f;
        bool isSprinting = hasMoveInput && input.sprint;

        // --- 1. SURGE LOGIC (Explosive start) ---
        if (isSprinting && !wasSprinting)
        {
            surge01 = 1f; // Instant pop to 1 for thruster kick
        }
        else
        {
            // Bleed out to a resting surge level if continuing to sprint
            float restingSurge = isSprinting ? 0.3f : 0f;
            surge01 = Mathf.MoveTowards(surge01, restingSurge, (1f / surgeFadeTime) * Time.fixedDeltaTime);
        }

        if (isSprinting != wasSprinting) onSurgeChanged?.Invoke(surge01);
        wasSprinting = isSprinting;

        // --- 2. TARGET SPEED ---
        float targetSpeed = isSprinting ? sprintSpeed : (hasMoveInput ? walkSpeed : 0f);
        Vector3 inputVelocity = moveDir * targetSpeed;

        // --- 3. ACCELERATION / BRAKING ---
        Vector3 currentHorizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float currentMag = currentHorizontalVel.magnitude;

        float effectiveAccel = hasMoveInput ? runAcceleration : runDeceleration;

        // Add huge bonus acceleration during the surge burst
        if (surge01 > 0.3f)
        {
            effectiveAccel += surgeAccelBurst * surge01;
        }

        // Direction turn drag: massive drag if pulling opposite to momentum
        if (hasMoveInput && currentMag > 0.5f)
        {
            float directionDot = Vector3.Dot(currentHorizontalVel.normalized, moveDir);
            if (directionDot < 0.5f) // Turning around or sideways
            {
                effectiveAccel *= directionChangeDrag;
            }
        }

        // --- 4. APPLY VELOCITY INTERPOLATION ---
        // Clean single vector interpolation (fixes previous double-smoothing bug)
        Vector3 targetFrameVelocity = Vector3.MoveTowards(currentHorizontalVel, inputVelocity, effectiveAccel * Time.fixedDeltaTime);

        // --- 5. UNDERWATER CURRENT ---
        Vector3 currentDriftDir = (transform.right * Mathf.Sin(Time.time * currentFrequency * 1.5f) + 
                                   transform.forward * Mathf.Sin(Time.time * currentFrequency * 0.9f)).normalized;
        float driftMag = hasMoveInput ? movingDriftSpeed : idleDriftSpeed;
        targetFrameVelocity += currentDriftDir * (driftMag * Time.fixedDeltaTime * 60f);

        // --- 6. STEPS & PHYSICAL FORCES ---
        if (hasMoveInput)
        {
            // Increase step frequency slightly when moving faster
            stepTimer += Time.fixedDeltaTime * stepsPerSecond * Mathf.Lerp(1f, 1.4f, normalizedSpeed);
            if (stepTimer >= 1f)
            {
                stepTimer -= 1f;

                // Actual physical lurch forward on footfall! Sells the "earthquake" step.
                targetFrameVelocity += moveDir * stepKickForce;

                // Fire events
                float footfallImpact = stepImpactBase + (normalizedSpeed * stepImpactBySpeed) + (isSprinting ? sprintStepImpactBonus : 0f) + (surge01 * 0.4f);
                audioFeedback?.PlayStepSound(footfallImpact);
                onStepLanded?.Invoke();
                onStepImpact?.Invoke(footfallImpact);
            }
        }
        else
        {
            stepTimer = 0f;
        }

        // Final Assignment
        rb.linearVelocity = new Vector3(targetFrameVelocity.x, rb.linearVelocity.y, targetFrameVelocity.z);
    }
}
"""

with open(file_path, "w") as f:
    f.write(content)
print("done")
