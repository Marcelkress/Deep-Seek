using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerOxygen : MonoBehaviour
{
    [Header("Generic")]
    public int maxOxygen = 500;
    public int currentOxygen { get; private set; }
    public int oxygenLossPrSWalk, oxygenLossPrSSprint;
    private int currentLoss;
    private float timer;
    private StarterAssetsInputs input;
    public bool replenish { get; private set; }

    [Header("Replenishment")] public int replenishAmount;
    public float replenishTimeThreshold;
    private float replenishTimer;
    
    void Awake()
    {
        replenish = false;
        currentOxygen = maxOxygen;
        input = GetComponent<StarterAssetsInputs>();
        currentLoss = oxygenLossPrSWalk;
    }
    
    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer > 1) // 1 second
        {
            currentOxygen -= currentLoss;
            timer = 0;
        }

        if (replenish)
        {
            replenishTimer += Time.deltaTime;

            if (replenishTimer > replenishTimeThreshold)
            {
                replenishTimer = 0;
                AddOxygen(replenishAmount);
                replenish = false;
            }
        }
        else
        {
            replenishTimer = 0;
        }
    }
    
    public void OnSprint(InputValue value)
    {
        if (value.isPressed)
        {
            currentLoss = oxygenLossPrSSprint;
        }
        else
        {
            currentLoss = oxygenLossPrSWalk;
        }
    }

    public void OnReplenish(InputValue value)
    {
        replenish = value.isPressed;
    }

    public void AddOxygen(int amount)
    {
        Debug.Log("Added " + amount + " oxygen");
        
        currentOxygen += amount;

        if (currentOxygen > maxOxygen)
        {
            currentOxygen = maxOxygen;
        }
    }
}
