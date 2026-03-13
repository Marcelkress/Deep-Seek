using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class OxygenUI : MonoBehaviour
{
    private Slider slider;
    public float updateInterval = 4;
    public float slideSpeed = 1;
    private float timer;
    private PlayerOxygen playerOxygen;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slider = GetComponent<Slider>();
        playerOxygen = GetComponentInParent<PlayerOxygen>();
        slider.DOValue(playerOxygen.currentOxygen, slideSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer > updateInterval)
        {
            slider.DOValue(playerOxygen.currentOxygen, slideSpeed);
            timer = 0;
        }
    }
}
