using System.Collections;
using Cinemachine;
using StarterAssets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.InputSystem;
using DG.Tweening;

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

    private CinemachineVirtualCamera virtualCamera;
        
    [Header("Replenishment amounts")] public int firstAmount = 100;
    public int secondAmount = 150, thirdAmount = 200;
    [Header ("Replenishment time thresholds")]public float firstThreshold = 10;
    public float secondThreshold = 15, thirdThreshold = 20;
    private float replenishTimer;
    private int amountToReplenish;
    public UnityEvent ReplenishStart, ReplenishEnd, StartCoolDownEvent, ResetCooldownEvent;

    public UnityEvent LowOxygenWarningEvent;

    [Header("Low oxygen Warning")]
    [SerializeField, Range(0f, 100f)] private float lowOxygenPercent = 10f; // 10 = 10%
    [SerializeField] private Light lowOxygenLight; // blinking light to indicate low oxygen
    [SerializeField] private float lightFadeDuration = 0.5f; // duration for fading the light in and out
    [SerializeField] private float lowOxygenBlinkInterval = 0.5f; // interval for blinking light
    public bool IsLowOxygen => currentOxygen <= LowOxygenThreshold;
    public int LowOxygenThreshold => Mathf.CeilToInt(maxOxygen * (lowOxygenPercent / 100f));

    private float initialLightIntensity;
    private bool walking, sprinting;
    
    void Awake()
    {
        currentOxygen = maxOxygen;
        input = GetComponent<StarterAssetsInputs>();
        currentLoss = oxygenLossPrSWalk;
        replenishCooldownTimer = 30;
        virtualCamera = GetComponent<CinemachineVirtualCamera>();

        initialLightIntensity = lowOxygenLight.intensity;
        lowOxygenLight.intensity = 0; // Start with the light off
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

        if (!warningActive)
        {
            StartCoroutine(LowOxygenWarningCoroutine());
        }

        if (currentOxygen <= 0)
        {
            Die();
        }
    }

    private bool warningActive = false;
    private IEnumerator LowOxygenWarningCoroutine()
    {
        if (!IsLowOxygen)
        {
            warningActive = false;
            lowOxygenLight.DOIntensity(0, lightFadeDuration); // Smooth fade out if it shouldn't be blinking
            yield break;
        }

        warningActive = true;

        while (IsLowOxygen)
        {
            LowOxygenWarningEvent.Invoke();
            
            // Smoothly fade to full intensity over the whole interval
            lowOxygenLight.DOIntensity(initialLightIntensity, lowOxygenBlinkInterval).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(lowOxygenBlinkInterval); 
            
            // Smoothly fade back down to 0 over the whole interval
            lowOxygenLight.DOIntensity(0, lowOxygenBlinkInterval).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(lowOxygenBlinkInterval);
        }

        // Clean up once the loop ends (oxygen was replenished)
        lowOxygenLight.DOIntensity(0, lightFadeDuration);
        warningActive = false;
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

    public void RemoveOxygen(int amount)
    {
        currentOxygen -= amount;
        
        GetComponent<CinemachineImpulseSource>().GenerateImpulse();
    }

    private void Die()
    {
        Debug.Log("Player has died");
        // Implement death logic here (e.g., respawn, game over screen, etc.)
    }
}
