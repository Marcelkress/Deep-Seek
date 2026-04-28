using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;

public class MonsterGameDirector : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerNoise playerNoise;
    [SerializeField] public Transform player;
    [SerializeField] private MonsterMovement monsterPrefab;
    [SerializeField] private Transform monsterContainer;
    [SerializeField] private float grazePeriodDuration = 10f;

    [Header("Trigger Timing")]
    [SerializeField] private float minTriggerInterval = 4f;
    [SerializeField] private float maxTriggerInterval = 12f;

    [Header("Spawn Settings")]
    [SerializeField] private float monsterSpawnHeightOffset = 5f;
    [SerializeField] private float spawnDistance = 45f;
    [SerializeField] private float eventSpawnHeight = 15f;
    [SerializeField] private float eventSideDistance = 20f;
    [SerializeField] public float terrainCheckHeight = 100f;
    [SerializeField] private LayerMask terrainMask = ~0;

    [Header("Noise Agression Multipliers")]
    [SerializeField, Range(0f, 1f)] public float currentAggressionWeight;
    [SerializeField] private float aggressionLow    = 0.2f;
    [SerializeField] private float aggressionMid    = 0.6f;
    [SerializeField] private float aggressionHigh   = 1.0f;

    [SerializeField] private float aggressionRampRate  = 1.5f; // how fast it climbs
    [SerializeField] private float aggressionDecayRate = 0.8f; // how fast it falls
    public float aggressionSharpness = 5f; // higher = more likely to pick events close to current aggression, lower = more random

    [Header("Auto Spawn Event Weights")]
    [SerializeField, Range(0f, 1f)] private float spawnSwimPastWeight = 0.2f;
    [SerializeField, Range(0f, 1f)] private float spawnPassOverheadWeight = 0.2f;
    [SerializeField, Range(0f, 1f)] private float spawnFakeChargeWeight = 0.5f;
    [SerializeField, Range(0f, 1f)] private float doNothingWeight = 0.1f;

    [Header("Debug Keybinds")]
    [SerializeField] private bool enableDebugKeys = true;
    [SerializeField] private InputAction swimPastKey;
    [SerializeField] private InputAction overheadKey;
    [SerializeField] private InputAction fakeChargeKey;
    [SerializeField] private InputAction retreatKey;

    private MonsterMovement currentMonster;
    private float nextAutoTriggerTime;

    public bool monsterActive = false;

    private void Awake()
    {

        if (playerNoise == null)
        {
            playerNoise = FindAnyObjectByType<PlayerNoise>();
        }

        if (player == null && playerNoise != null)
        {
            player = playerNoise.transform;
        }

        StartCoroutine(AgressionLevel());

        

        StartCoroutine(ScheduleNextAutoTrigger());
    }

    private IEnumerator wait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    private void OnEnable()
    {
        swimPastKey?.Enable();
        overheadKey?.Enable();
        fakeChargeKey?.Enable();
        retreatKey?.Enable();
    }

    private void OnDisable()
    {
        swimPastKey?.Disable();
        overheadKey?.Disable();
        fakeChargeKey?.Disable();
        retreatKey?.Disable();
    }

    private void Update()
    {

        if (enableDebugKeys && monsterActive == false)
        {   
            if (swimPastKey != null && swimPastKey.WasPressedThisFrame()) ForceSwimPastPOV();
            else if (overheadKey != null && overheadKey.WasPressedThisFrame()) ForcePassOverhead();
            else if (fakeChargeKey != null && fakeChargeKey.WasPressedThisFrame()) ForceFakeCharge();
            else if (retreatKey != null && retreatKey.WasPressedThisFrame()) ForceRetreatAndDespawn();
        }
    }

    private void EvaluateMonsterSpawningWeights()
    {
        if (player == null || monsterActive) return;

        MonsterMovement.EncounterEvent[] events = new MonsterMovement.EncounterEvent[]
        {
            MonsterMovement.EncounterEvent.SwimPastPOV,
            MonsterMovement.EncounterEvent.PassOverhead,
            MonsterMovement.EncounterEvent.FakeCharge,
            MonsterMovement.EncounterEvent.DoNothing
        };

        float[] aggressionAnchors = { spawnSwimPastWeight, spawnPassOverheadWeight, spawnFakeChargeWeight, doNothingWeight };
        int selectedIndex = SelectAggressionWeightedIndex(aggressionAnchors);
        if (selectedIndex >= 0 && selectedIndex < events.Length)
        {
            StartEvent(events[selectedIndex]);
            return;
        }

        StartEvent(MonsterMovement.EncounterEvent.FakeCharge);
    }

    public int SelectAggressionWeightedIndex(float[] aggressionAnchors)
    {
        if (aggressionAnchors == null || aggressionAnchors.Length == 0)
        {
            return -1;
        }

        float[] weightedScores = new float[aggressionAnchors.Length];
        float totalWeight = 0f;

        for (int i = 0; i < aggressionAnchors.Length; i++)
        {
            float distanceFromCurrentAggressionWeight = Mathf.Abs(aggressionAnchors[i] - currentAggressionWeight);

            // Bell curve: events closer to current aggression score higher
            // aggressionSharpness controls how steeply score falls off with distance
            float weightedScore = Mathf.Exp(-aggressionSharpness * distanceFromCurrentAggressionWeight * distanceFromCurrentAggressionWeight);
            weightedScores[i] = weightedScore;
            totalWeight += weightedScore;
        }

        if (totalWeight <= 0f)
        {
            return -1;
        }

        float randomRoll = Random.Range(0f, totalWeight);
        float runningTotal = 0f;

        for (int i = 0; i < weightedScores.Length; i++)
        {
            runningTotal += weightedScores[i];
            if (randomRoll <= runningTotal)
            {
                return i;
            }
        }

        return weightedScores.Length - 1;
    }

    public void ForceSwimPastPOV() => StartEvent(MonsterMovement.EncounterEvent.SwimPastPOV);

    public void ForcePassOverhead() => StartEvent(MonsterMovement.EncounterEvent.PassOverhead);

    public void ForceFakeCharge() => StartEvent(MonsterMovement.EncounterEvent.FakeCharge);

    public void ForceRetreatAndDespawn() => StartEvent(MonsterMovement.EncounterEvent.RetreatAndDespawn);

    private void StartEvent(MonsterMovement.EncounterEvent eventType)
    {
        if (eventType == MonsterMovement.EncounterEvent.DoNothing)
        {
            StartCoroutine(ScheduleNextAutoTrigger());
            return;
        }
        if (player == null)
        {
            return;
        }

        if (!EnsureMonsterIsSpawned())
        {
            return;
        }

        Vector3 spawnPosition = GetSpawnPosition(eventType);
        currentMonster.transform.position = FitToTerrain(spawnPosition);

        Vector3 toPlayer = player.position - currentMonster.transform.position;
        if (toPlayer.sqrMagnitude > 0.001f)
        {
            currentMonster.transform.rotation = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
        }

        currentMonster.BeginEvent(eventType, player);
    }

    private bool EnsureMonsterIsSpawned()
    {
        if (currentMonster != null)
        {
            return true;
        }

        if (monsterPrefab == null)
        {
            return false;
        }

        Transform parent = monsterContainer;
        currentMonster = Instantiate(monsterPrefab, parent);
        return true;
    }

    private Vector3 GetSpawnPosition(MonsterMovement.EncounterEvent eventType)
    {
        Vector3 forward = MonsterMovement.GetPlayerForwardDir(player);
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        float side = Random.value < 0.5f ? -1f : 1f;

        if (eventType == MonsterMovement.EncounterEvent.PassOverhead)
        {
            return player.position - (forward * spawnDistance) + (Vector3.up * eventSpawnHeight);
        }

        if (eventType == MonsterMovement.EncounterEvent.FakeCharge)
        {
            return player.position + (forward * spawnDistance) + (right * side * eventSideDistance);
        }

        if (eventType == MonsterMovement.EncounterEvent.RetreatAndDespawn && currentMonster != null && currentMonster.gameObject.activeSelf)
        {
            return currentMonster.transform.position;
        }

        return player.position + (forward * spawnDistance) + (right * side * eventSideDistance);
    }

    private Vector3 FitToTerrain(Vector3 desired)
    {
        // raycast from strictly above desired position to bump the creature out of floor geometry
        Vector3 rayOrigin = desired + Vector3.up * terrainCheckHeight;
        float rayDistance = terrainCheckHeight * 2f;
        
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayDistance, terrainMask))
        {
            desired.y = hit.point.y + monsterSpawnHeightOffset;
        }

        return desired;
    }

    private bool startGrazePeriodPlayed = false;
    public IEnumerator ScheduleNextAutoTrigger()
    {
        if (!startGrazePeriodPlayed)
        {
            startGrazePeriodPlayed = true;
            yield return new WaitForSeconds(grazePeriodDuration);
        }
        nextAutoTriggerTime = Time.time + Random.Range(minTriggerInterval, maxTriggerInterval);
        yield return new WaitForSeconds(nextAutoTriggerTime - Time.time);
        EvaluateMonsterSpawningWeights();
    }

    public PlayerNoise.NoiseLevel PlayerNoiseLevel()
    {
        return playerNoise.noiseLevel;
    }

     private IEnumerator AgressionLevel()
    {
        while (true)
        {
            float targetMax = PlayerNoiseLevel() switch
            {
                PlayerNoise.NoiseLevel.None => 0f,
                PlayerNoise.NoiseLevel.Low => aggressionLow,
                PlayerNoise.NoiseLevel.Mid => aggressionMid,
                PlayerNoise.NoiseLevel.High => aggressionHigh,
                _ => 0f
            };

            targetMax = Mathf.Clamp01(targetMax);

            if (currentAggressionWeight < targetMax)
            {
                // Ramp up toward the target ceiling
                currentAggressionWeight += aggressionRampRate * Time.deltaTime;
                currentAggressionWeight  = Mathf.Min(currentAggressionWeight, targetMax);
            }
            else if (currentAggressionWeight > targetMax)
            {
                // Decay down toward the target ceiling (or zero)
                currentAggressionWeight -= aggressionDecayRate * Time.deltaTime;
                currentAggressionWeight  = Mathf.Max(currentAggressionWeight, targetMax);
            }
            yield return null;
        }
    }

}