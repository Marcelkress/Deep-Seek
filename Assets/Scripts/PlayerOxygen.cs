using UnityEngine;

public class PlayerOxygen : MonoBehaviour
{
    public int maxOxygen = 500;
    public int currentOxygen;
    public int oxygenLossPrS;
    private float timer;

    void Awake()
    {
        currentOxygen = maxOxygen;
    }
    
    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer > 1)
        {
            currentOxygen -= oxygenLossPrS;
            timer = 0;
        }
    }
}
