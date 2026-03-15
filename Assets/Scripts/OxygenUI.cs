using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class OxygenUI : MonoBehaviour
{
    [Header("Main Oxygen slider")]
    private Slider mainSlider;
    public float updateInterval = 4;
    public float slideSpeed = 1;
    private float timer;
    private PlayerOxygen playerOxygen;
    private bool started;

    [Header("Replenish Oxygen slider")] public Slider replenishSlider;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        started = false;
        mainSlider = GetComponent<Slider>();
        playerOxygen = GetComponentInParent<PlayerOxygen>();
        mainSlider.DOValue(playerOxygen.currentOxygen, slideSpeed);

        replenishSlider.maxValue = playerOxygen.replenishAmount;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer > updateInterval)
        {
            mainSlider.DOValue(playerOxygen.currentOxygen, slideSpeed);
            timer = 0;
        }

        if (playerOxygen.replenish && !started) 
        {
            replenishSlider.DOValue(playerOxygen.replenishAmount, playerOxygen.replenishTimeThreshold);
            started = true;
        }
        else if (!playerOxygen.replenish && started)
        {
            replenishSlider.DOValue(0, .5f);
            started = false;
        }
    }
}
