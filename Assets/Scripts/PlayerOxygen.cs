using StarterAssets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerOxygen : MonoBehaviour
{
    [Header("Generic")]
    public int maxOxygen = 500;
    public int currentOxygen { get; private set; }
    public int oxygenLossPrSStand, oxygenLossPrSWalk, oxygenLossPrSSprint;
    public int currentLoss;
    private float timer;
    private StarterAssetsInputs input;
    public float replenishCoolDown = 30;
    private float replenishCooldownTimer;
        
    [Header("Replenishment amounts")] public int firstAmount = 100;
    public int secondAmount = 150, thirdAmount = 200;
    [Header ("Replenishment time thresholds")]public float firstThreshold = 10;
    public float secondThreshold = 15, thirdThreshold = 20;
    private float replenishTimer;
    private int amountToReplenish;
    public UnityEvent ReplenishStart, ReplenishEnd, StartCoolDownEvent, ResetCooldownEvent;

    private bool walking, sprinting;
    
    void Awake()
    {
        currentOxygen = maxOxygen;
        input = GetComponent<StarterAssetsInputs>();
        currentLoss = oxygenLossPrSWalk;
        replenishCooldownTimer = 30;
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
        
        ReplenishUpdate();
        
        OxygenLossUpdate();
    }

    private void OxygenLossUpdate()
    {
        if (walking && sprinting)
        {
            currentLoss = oxygenLossPrSSprint;
        }
        else if (walking)
        {
            currentLoss = oxygenLossPrSWalk;
        }
        else
        {
            currentLoss = oxygenLossPrSStand;
        }
    }

    private void ReplenishUpdate()
    {
        replenishCooldownTimer += Time.deltaTime;

        if (replenishCooldownTimer < replenishCoolDown)
        {
            return;
        }

        if (pressed)
        {
            replenishTimer += Time.deltaTime;
            //Debug.Log(replenishTimer);

            if (replenishTimer > thirdThreshold)
            {
                amountToReplenish = thirdAmount;
                pressed = false;
                released = true;
            }
            else if (replenishTimer > secondThreshold)
            {
                amountToReplenish = secondAmount;
            }
            else if (replenishTimer > firstThreshold)
            {
                amountToReplenish = firstAmount;
            }
        }
        else if (released)
        {
            if (amountToReplenish != 0)
            {
                replenishCooldownTimer = 0;
                ResetCooldownEvent.Invoke();
            }
            released = false;
            AddOxygen(amountToReplenish);
            amountToReplenish = 0;
            replenishTimer = 0;
            ReplenishEnd.Invoke();
        }
    }

    public void OnMove(InputValue value)
    {
        Vector2 val = value.Get<Vector2>();

        if (val != Vector2.zero)
        {
            walking = true;
        }
        else
        {
            walking = false;
        }
    }
    
    public void OnSprint(InputValue value)
    {
        if (value.isPressed)
        {
            sprinting = true;
        }
        else
        {
            sprinting = false;
        }
    }

    private bool pressed, released;

    public void OnReplenish(InputValue value)
    {
        if (value.isPressed && replenishCooldownTimer > replenishCoolDown)
        {
            pressed = true;
            released = false;
            ReplenishStart.Invoke();
        }
        else
        {
            pressed = false;
            released = true;
        }
        
        Debug.Log(value.isPressed);
    }

    public void AddOxygen(int amount)
    {
        Debug.Log("Replenished " + amount + " oxygen");
        
        currentOxygen += amount;

        if (currentOxygen > maxOxygen)
        {
            currentOxygen = maxOxygen;
        }
    }
}
