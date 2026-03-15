using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerOxygen : MonoBehaviour
{
    public int maxOxygen = 500;
    public int currentOxygen { get; private set; }
    public int oxygenLossPrSWalk, oxygenLossPrSSprint;
    private int currentLoss;
    private float timer;
    private StarterAssetsInputs input;

    void Awake()
    {
        currentOxygen = maxOxygen;
        input = GetComponent<StarterAssetsInputs>();
        currentLoss = oxygenLossPrSWalk;
    }
    
    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer > 1)
        {
            currentOxygen -= currentLoss;
            timer = 0;
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

    public void AddOxygen(int amount)
    {
        currentOxygen += amount;
    }
}
