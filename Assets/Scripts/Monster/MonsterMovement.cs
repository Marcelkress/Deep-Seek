using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class MonsterMovement : MonoBehaviour
{
    public enum EncounterEvent
    {
        DoNothing, // for MonsterDirector to use
        SwimPastPOV,
        PassOverhead,
        FakeCharge,
        Charge,
        RetreatAndDespawn

    }

    private enum EventPhase
    {
        Event,
        FakeRetreat,
        Retreat
    }

    [Header("Core")]
    [SerializeField] private float maxSpeed = 12f;
    [SerializeField] private float acceleration = 6f;
    [SerializeField] private float maxTurnDegreesPerSecond = 90f;
    [SerializeField] private int monsterOxygenDamage = 100;
    [SerializeField] private float playerOffset = 2f;
    [SerializeField] private float detectionRadius = 10f;

    [Header("Shared Target Offsets")]
    [SerializeField] private float minTargetForwardDistance = 20f;
    [SerializeField] private float maxTargetForwardDistance = 35f;
    [SerializeField] private float minTargetSideDistance = 10f;
    [SerializeField] private float maxTargetSideDistance = 30f;
    [SerializeField] private float targetHeightMin = 4f;
    [SerializeField] private float targetHeightMax = 14f;

    [Header("SwimPast")]
    [SerializeField] private float swimPastDuration = 4f;

    [Range(0f, 1f)]
    [SerializeField] private float swimPastAggressionWeight = 0.2f;

    [Header("PassOverhead")]
    [SerializeField] private float overheadDuration = 3.5f;
    [SerializeField] private float additionalOverheadHeightScaling = 1.5f; // additional height added on top of random height offset to ensure it is noticeably overhead
    [Range(0f, 1f)]
    [SerializeField] private float passOverheadAggressionWeight = 0.2f;

    [Header("FakeCharge")]
    [SerializeField] private float fakeChargeDuration = 1.4f;
    [SerializeField] private float fakeOutDuration = 2f;
    [Range(0f, 1f)]
    [SerializeField] private float fakeChargeAggressionWeight = 0.5f;

    [Header("Charge")]
    [SerializeField] private float chargeDuration = 2f;
    [Range(0f, 1f)]
    [SerializeField] private float chargeAggressionWeight = 0.9f;

    [Header("Phase Completion")]
    [SerializeField] private float arrivalRadius = 3f;

    [Header("Retreat")]
    [SerializeField] private float retreatDistance = 40f;
    [SerializeField] private float despawnDistance = 65f;
    [Range(0f, 1f)]
    [SerializeField] private float retreatAggressionWeight = 0.1f;

    [Header("Avoidance")]
    [SerializeField] private LayerMask avoidanceMask;
    [SerializeField] private float probeRadius = 2f;
    [SerializeField] private float forwardProbeDistance = 15f;
    [SerializeField] private float sideProbeDistance = 10f;
    [SerializeField] private float sideProbeAngle = 30f;
    [SerializeField] private float floorClearance = 8f;
    [SerializeField] private float avoidanceForce = 15f;
    private Vector3 smoothedAvoidance;
    [SerializeField] private float avoidanceSmoothTime = 0.25f;

    [Header("Dust VFX")]
    [SerializeField] private VisualEffect dustVFX;
    [SerializeField] private LayerMask dustGroundLayer;
    [SerializeField] private float rayDistance = 2f;
    [SerializeField] private float dustSpawnDistance = 0.2f; // 

    private Vector3 lastDustPosition; // New: tracks last spawn spot
    private MonsterGameDirector monsterDirector;
    private Transform player;
    private EncounterEvent currentEvent;
    private EventPhase phase;
    private bool isRunningEvent;
    private float phaseTimer;
    private float phaseSafetyDuration;
    private bool isFakingOut;
    private float randomSide;
    private Vector3 currentVelocity;
    private Vector3 currentForward;
    private Vector3 desiredPosition;
    private float eventHeightOffset;
    private Vector3 eventTargetA;
    private Vector3 eventTargetB;

    private void OnEnable()
    {
        currentForward = transform.forward.sqrMagnitude > 0f ? transform.forward.normalized : Vector3.forward;
        currentVelocity = currentForward * (maxSpeed * 0.5f);
        desiredPosition = transform.position + currentForward * 5f;

        monsterDirector = GetComponentInParent<MonsterGameDirector>();
        monsterDirector.monsterActive = true;

        lastDustPosition = transform.position;
    }

    private void Update()
    {
        if (!isRunningEvent || player == null)
        {
            return;
        }

        UpdateCurrentEvent();

        DynamicMovement();

        if (phase == EventPhase.Retreat && Vector3.Distance(transform.position, player.position) >= despawnDistance)
        {
            isRunningEvent = false;
            monsterDirector.monsterActive = false;
            monsterDirector.StartCoroutine(monsterDirector.ScheduleNextAutoTrigger());
            Destroy(gameObject);
        }

        if (phase == EventPhase.FakeRetreat && Vector3.Distance(transform.position, player.position) >= retreatDistance)
        {
            EvaluateNextPhaseOrEnd();
        }

       SpawnDustVFX();
    }

    private void EvaluateNextPhaseOrEnd()
    {
        EncounterEvent[] events = new EncounterEvent[]
        {
            EncounterEvent.SwimPastPOV,
            EncounterEvent.PassOverhead,
            EncounterEvent.FakeCharge,
            EncounterEvent.RetreatAndDespawn,
            EncounterEvent.Charge
        };

        float[] aggressionAnchors = new float[]
        {
            swimPastAggressionWeight,
            passOverheadAggressionWeight,
            fakeChargeAggressionWeight,
            retreatAggressionWeight,
            chargeAggressionWeight
        };

        int selectedIndex = monsterDirector.SelectAggressionWeightedIndex(aggressionAnchors);
        if (selectedIndex >= 0 && selectedIndex < events.Length)
        {
            BeginEvent(events[selectedIndex], player);
            return;
        }

        BeginEvent(EncounterEvent.RetreatAndDespawn, player);
    }
    private void DynamicMovement()
    {
        Vector3 toTarget = desiredPosition - transform.position;
        Vector3 desiredDirection = toTarget.sqrMagnitude < 0.0001f ? currentForward : toTarget.normalized;
        Vector3 rawAvoidance = GetTotalObstacleAvoidance();
        
        smoothedAvoidance = Vector3.Lerp(smoothedAvoidance, rawAvoidance, Time.deltaTime / Mathf.Max(0.001f, avoidanceSmoothTime));
        
        desiredDirection = (desiredDirection + smoothedAvoidance).normalized;
        
        Vector3 desiredVelocity = desiredDirection * maxSpeed;
        currentVelocity = Vector3.MoveTowards(currentVelocity, desiredVelocity, acceleration * Time.deltaTime);
        currentVelocity = Vector3.ClampMagnitude(currentVelocity + smoothedAvoidance * Time.deltaTime, maxSpeed);
        
        
        //  ------noget der gør den ikke svømmer gennem verden--------- //
        Vector3 displacement = currentVelocity * Time.deltaTime;
        float moveDistance = displacement.magnitude;

        if (moveDistance > 0.001f && Physics.SphereCast(transform.position, probeRadius, displacement.normalized, out RaycastHit hit, moveDistance, avoidanceMask))
        {
            displacement = displacement.normalized * Mathf.Max(0f, hit.distance - 0.05f);
            currentVelocity = Vector3.ProjectOnPlane(currentVelocity, hit.normal);
        }

        transform.position += displacement;
        //  ------noget der gør den ikke svømmer gennem verden--------- //

        if (currentVelocity.sqrMagnitude > 0.01f)
        {
        currentForward = currentVelocity.normalized;
        Quaternion targetRotation = Quaternion.LookRotation(currentForward);
        transform.rotation = Quaternion.RotateTowards(
        transform.rotation,
        targetRotation,
        maxTurnDegreesPerSecond * Time.deltaTime
        );
}
    }

    // Called by MonsterGameDirector to start an encounter event
    public void BeginEvent(EncounterEvent eventType, Transform playerRef)
    {
        if (playerRef == null)
        {
            return;
        }

        player = playerRef;
        currentEvent = eventType;
        isRunningEvent = true;
        phaseTimer = 0f;
        phaseSafetyDuration = 0f;
        isFakingOut = false;
        randomSide = Random.value < 0.5f ? -1f : 1f; // left or right 
        eventHeightOffset = Random.Range(targetHeightMin, targetHeightMax);

        ConfigureEventTargets(eventType);

        if (eventType == EncounterEvent.RetreatAndDespawn)
        {
            BeginRetreatPhase();
            return;
        }

        phase = EventPhase.Event;
        desiredPosition = eventTargetA;
        
        switch (eventType)
        {
            case EncounterEvent.SwimPastPOV:
                phaseSafetyDuration = swimPastDuration;
                break;
            case EncounterEvent.PassOverhead:
                phaseSafetyDuration = overheadDuration;
                break;
            case EncounterEvent.FakeCharge:
                phaseSafetyDuration = fakeChargeDuration;
                break;
            case EncounterEvent.Charge:
                phaseSafetyDuration = chargeDuration;
                break;
            default:
                phaseSafetyDuration = 1f;
                break;
        }
    }

    private void UpdateCurrentEvent()
    {
        if (phase == EventPhase.Retreat || phase == EventPhase.FakeRetreat)
        {
            Vector3 retreatDirection = currentVelocity.sqrMagnitude > 0.01f ? currentVelocity.normalized : currentForward;
            retreatDirection.y = 0f;

            if (retreatDirection.sqrMagnitude < 0.0001f)
            {
                retreatDirection = transform.position - player.position;
                retreatDirection.y = 0f;
            }

            retreatDirection = retreatDirection.sqrMagnitude > 0.0001f ? retreatDirection.normalized : Vector3.forward;

            Vector3 retreatTarget = transform.position + (retreatDirection * retreatDistance);
            float minRetreatHeight = player.position.y + targetHeightMin;
            float maxRetreatHeight = player.position.y + targetHeightMax;
            retreatTarget.y = Mathf.Clamp(transform.position.y, minRetreatHeight, maxRetreatHeight);
            desiredPosition = retreatTarget;
            return;
        }

        
        phaseTimer += Time.deltaTime;

        if (HasArrived(desiredPosition))
        {
            if (EncounterEvent.FakeCharge == currentEvent && !isFakingOut)
            {
                isFakingOut = true;
                desiredPosition = eventTargetB;
                phaseTimer = 0f;
                phaseSafetyDuration = fakeOutDuration;
                return;
            }
            if (EncounterEvent.Charge == currentEvent)
            {
                HitPlayer();
            }

            BeginFakePhase();
            return;
        }

        if(EncounterEvent.Charge == currentEvent)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= detectionRadius)
            {
                Vector3 playerPos = new Vector3(player.position.x, player.position.y + playerOffset, player.position.z);
                desiredPosition = playerPos;
                return;
            }
           
        }


        if (phaseTimer >= phaseSafetyDuration) // If the monster got stuck or took too long.
        {
            BeginFakePhase();
        }
    }

    private void ConfigureEventTargets(EncounterEvent eventType)
    {
        Vector3 playerPos = player.position;
        Vector3 playerForwardDir = GetPlayerForwardDir(player);
        Vector3 right = Vector3.Cross(Vector3.up, playerForwardDir).normalized;
        float sideDistance = Random.Range(minTargetSideDistance, maxTargetSideDistance);
        float forwardDistance = Random.Range(minTargetForwardDistance, maxTargetForwardDistance);

        switch (eventType)
        {
            case EncounterEvent.SwimPastPOV:
            {
                eventTargetA = playerPos + (playerForwardDir * forwardDistance) + (right * randomSide * sideDistance) + (Vector3.up * eventHeightOffset);
                eventTargetA = EnsureTargetAboveTerrain(eventTargetA, targetHeightMin);
                break;
            }
            case EncounterEvent.PassOverhead:
            {
                eventTargetA = playerPos + (playerForwardDir * forwardDistance) + (Vector3.up * (eventHeightOffset * additionalOverheadHeightScaling));
                eventTargetA = EnsureTargetAboveTerrain(eventTargetA, targetHeightMin);
                break;
            }
            case EncounterEvent.FakeCharge:
            {
                eventTargetA = playerPos + (playerForwardDir * forwardDistance);
                eventTargetA = EnsureTargetAboveTerrain(eventTargetA, targetHeightMin);
                // because it is 2 stage event
                eventTargetB = playerPos + (playerForwardDir * forwardDistance) + (right * (-randomSide * sideDistance)) + (Vector3.up * eventHeightOffset);
                eventTargetB = EnsureTargetAboveTerrain(eventTargetB, targetHeightMin);
                break;
            }
            case EncounterEvent.Charge:
            {
                eventTargetA = playerPos; // will be updated every frame in UpdateCurrentEvent to ensure it is always charging towards the player
                break;
            }
            default:
                eventTargetA = transform.position + currentForward * 5f;
                eventTargetA = EnsureTargetAboveTerrain(eventTargetA, targetHeightMin);
                break;
        }
    }

    private bool HasArrived(Vector3 targetPosition)
    {
        float radius = Mathf.Max(0.25f, arrivalRadius);
        return (transform.position - targetPosition).sqrMagnitude <= radius * radius;
    }

    private void BeginRetreatPhase()
    {
        phase = EventPhase.Retreat;
        phaseTimer = 0f;
        phaseSafetyDuration = 0f;
    }

    private void BeginFakePhase()
    {
        phase = EventPhase.FakeRetreat;
        phaseTimer = 0f;
        phaseSafetyDuration = 0f;
    }

    float forwardWeight = 1f;
    float sideWeight = 0.7f;
    private Vector3 GetTotalObstacleAvoidance()
    {
        Vector3 forward = currentVelocity.sqrMagnitude > 0.01f ? currentVelocity.normalized : transform.forward;
        Vector3 left = Quaternion.AngleAxis(-sideProbeAngle, Vector3.up) * forward;
        Vector3 right = Quaternion.AngleAxis(sideProbeAngle, Vector3.up) * forward;

        Vector3 total = Vector3.zero;
        total += ObstacleAvoidance(forward, forwardProbeDistance, forwardWeight); 
        total += ObstacleAvoidance(left, sideProbeDistance, sideWeight); 
        total += ObstacleAvoidance(right, sideProbeDistance, sideWeight); 
        total += GetFloorAvoidance();
        return total;
    }

    private Vector3 ObstacleAvoidance(Vector3 direction, float distance, float weight)
    {
        if (Physics.SphereCast(transform.position, probeRadius, direction, out RaycastHit hit, distance, avoidanceMask ))
        {
            float urgency = 1f - (hit.distance / Mathf.Max(0.01f, distance));
            return hit.normal * (avoidanceForce * urgency * weight);
        }

        return Vector3.zero;
    }

    private Vector3 GetFloorAvoidance()
    {
        if (!Physics.SphereCast(transform.position, probeRadius, Vector3.down, out RaycastHit hit, floorClearance * 1.5f, avoidanceMask))
        {
            return Vector3.zero;
        }
        float urgency = 1f - Mathf.Clamp01(hit.distance / Mathf.Max(0.01f, floorClearance));
        return Vector3.up * (avoidanceForce * urgency);
    }

    public static Vector3 GetPlayerForwardDir(Transform refTransform)
    {
        Vector3 forward = refTransform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
        {
            return Vector3.forward;
        }

        return forward.normalized;
    }

    private Vector3 EnsureTargetAboveTerrain(Vector3 target, float minHeight = 2f)
    {
        float rayDistance = monsterDirector.terrainCheckHeight * 2f;  
        if (Physics.Raycast(target + Vector3.up * monsterDirector.terrainCheckHeight, Vector3.down, out RaycastHit hit, rayDistance, avoidanceMask))
        {
            target.y = Mathf.Max(target.y, hit.point.y + minHeight);
        }
        return target;
    }

     private void HitPlayer()
    {
        Debug.Log("Player Hit by Monster!");
       PlayerOxygen playerOxygen = monsterDirector.player.GetComponent<PlayerOxygen>(); 
       playerOxygen.RemoveOxygen(monsterOxygenDamage);
       // Notify the director that the player has been hit
        // Here you would implement what happens when the monster successfully hits the player, e.g. reduce health, trigger effects, etc.

         monsterDirector.currentAgressionWeight /=  2; // så den ikke bliver lige så sur næste gang, da den lige har brugt en masse aggression på at angribe
    }
    
    private void SpawnDustVFX()
    {
        // Check if the monster has moved enough distance
        if (Vector3.Distance(transform.position, lastDustPosition) >= dustSpawnDistance)
        {
            // Raycast origin is elevated from the character's base, and shifted purely on the X/Z plane
            Vector3 rayStart = transform.position + (Vector3.up * 0.5f) + (transform.forward * 0.5f);
    
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayDistance, dustGroundLayer))
            {
                dustVFX.transform.position = hit.point + (hit.normal * 0.1f); // Slightly offset from the ground to prevent clipping
                
                dustVFX.SendEvent("DustVFX");
        
                lastDustPosition = transform.position; 
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || !isRunningEvent)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(desiredPosition, 0.45f);
        Gizmos.DrawLine(transform.position, desiredPosition);
    }


    private void OnDrawGizmos()
    {
        Vector3 forward = Application.isPlaying && currentVelocity.sqrMagnitude > 0.01f ? currentVelocity.normalized : transform.forward;
        Vector3 left = Quaternion.AngleAxis(-sideProbeAngle, Vector3.up) * forward;
        Vector3 right = Quaternion.AngleAxis(sideProbeAngle, Vector3.up) * forward;

        // Draw avoidance raycast lines
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + forward * forwardProbeDistance);
        Gizmos.DrawLine(transform.position, transform.position + left * sideProbeDistance);
        Gizmos.DrawLine(transform.position, transform.position + right * sideProbeDistance);

        // Draw probe radii at the end of the lines
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position + forward * forwardProbeDistance, probeRadius);
        Gizmos.DrawSphere(transform.position + left * sideProbeDistance, probeRadius);
        Gizmos.DrawSphere(transform.position + right * sideProbeDistance, probeRadius);

        // Draw floor clearance (downward probe)
        Gizmos.color = Color.yellow;
        Vector3 groundProbeEnd = transform.position + Vector3.down * (floorClearance * 1.5f);
        Gizmos.DrawLine(transform.position, groundProbeEnd);
        
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawSphere(groundProbeEnd, probeRadius);
    }

}
