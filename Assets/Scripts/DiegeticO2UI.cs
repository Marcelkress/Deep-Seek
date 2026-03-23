using DG.Tweening.Plugins.Options;
using UnityEngine;
using DG.Tweening;

public class DiegeticO2UI : MonoBehaviour
{
    public Transform indicator;
    public float maxOxygenRot,  minOxygenRot;
    public float updateTimeInterval = 4f;
    private float timer;
    private PlayerOxygen playerOxygen;
    private float totalRot, playerMaxOxygen;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerOxygen = GetComponentInParent<PlayerOxygen>();
        playerMaxOxygen = playerOxygen.maxOxygen;


        timer = 4;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > updateTimeInterval)
        {
            // Calculate normalized oxygen (0 to 1)
            float oxygenNormalized = playerOxygen.currentOxygen / playerMaxOxygen;
        
            // Map to your rotation range (245 at max, -69 at min)
            float targetRotation = Mathf.Lerp(minOxygenRot, maxOxygenRot, oxygenNormalized);

            indicator.DOLocalRotate(new Vector3(indicator.localEulerAngles.x, indicator.localEulerAngles.y,
                targetRotation), 1f);

            timer = 0;
        }
    }

}
