using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Light))]
public class LightFade : MonoBehaviour
{
    public float maxIntensity, minIntensity;
    public float fadeTime, waitTime;
    private Light light;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        light = GetComponent<Light>();
        StartCoroutine(FadeLight());
    }

    private IEnumerator FadeLight()
    {
        while (true)
        {
            light.DOIntensity(maxIntensity, fadeTime);
            yield return new WaitForSeconds(fadeTime + waitTime);
            light.DOIntensity(minIntensity, fadeTime);
            yield return new WaitForSeconds(fadeTime + waitTime);
        }
    }
}
