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

    [Header("Replenish Oxygen slider")] public Slider replenishSlider;
    public Slider replenishCooldownSlider;
    public float cooldownSlideSpeed = 0.1f;
    private Tween replenishTween;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainSlider = GetComponent<Slider>();
        playerOxygen = GetComponentInParent<PlayerOxygen>();
        mainSlider.DOValue(playerOxygen.currentOxygen, slideSpeed);
        replenishCooldownSlider.DOValue(replenishCooldownSlider.maxValue, cooldownSlideSpeed);
        mainSlider.maxValue = playerOxygen.maxOxygen;

        //playerOxygen.ReplenishStart.AddListener(StartReplenish);
        //playerOxygen.ReplenishEnd.AddListener(EndReplenish);
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
    }

    public void StartCoolDownSlider()
    {
        replenishCooldownSlider.DOValue(replenishCooldownSlider.maxValue, playerOxygen.replenishCoolDown)
            .SetEase(Ease.Linear);
    }

    public void ResetCooldownSlider()
    {
        replenishCooldownSlider.DOValue(0, cooldownSlideSpeed).OnComplete(StartCoolDownSlider)
            .SetEase(Ease.Linear);
    }

    public void StartReplenish()
    {
        replenishTween?.Kill();
        replenishTween = replenishSlider.DOValue(replenishSlider.maxValue, playerOxygen.thirdThreshold)
            .SetEase(Ease.Linear);
    }

    public void EndReplenish()
    {
        replenishTween?.Kill();
        replenishTween = replenishSlider.DOValue(replenishSlider.minValue, slideSpeed)
            .SetEase(Ease.Linear);
    }
}
