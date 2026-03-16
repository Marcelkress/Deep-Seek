using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class NoiseUI : MonoBehaviour
{
    private PlayerNoise playerNoise;
    private Slider slider;
    public float changeSpeed = 0.1f;

    public float lowValue = .2f, midValue = 0.5f, highValue = 0.8f;
    
    private Tween noiseTween;
    private PlayerNoise.NoiseLevel lastNoiseLevel = PlayerNoise.NoiseLevel.None;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerNoise = GetComponentInParent<PlayerNoise>();
        slider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        // Only create a new tween if the noise level changed
        if (playerNoise.noiseLevel != lastNoiseLevel)
        {
            noiseTween?.Kill();
            
            float targetValue = GetTargetValue(playerNoise.noiseLevel);
            slider.DOValue(targetValue, changeSpeed).SetEase(Ease.InOutCubic);
            
            lastNoiseLevel = playerNoise.noiseLevel;
        }
    }
    
    private float GetTargetValue(PlayerNoise.NoiseLevel level)
    {
        switch (playerNoise.noiseLevel)
        {
            case PlayerNoise.NoiseLevel.None :
                return 0;
                break;
            case PlayerNoise.NoiseLevel.Low :
                return lowValue;
                break;
            case PlayerNoise.NoiseLevel.Mid :
                return midValue;
                break;
            case PlayerNoise.NoiseLevel.High :
                return highValue;
                break;
            default:
                return 0;
        }
    }
}
