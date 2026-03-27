using System;
using System.Collections;
using StarterAssets;
using Unity.Mathematics;
using UnityEngine;

public class PlayerNoise : MonoBehaviour
{
    private StarterAssetsInputs input;
    private bool walking;
    private bool usingORC;
    private bool interacting;
    public enum NoiseLevel
    {
        None,
        Low,
        Mid,
        High
    }

    public NoiseLevel noiseLevel;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        input = GetComponent<StarterAssetsInputs>();
        noiseLevel = NoiseLevel.None;
    }

    private void Update()
    {
        walking = input.move != Vector2.zero;

        if (usingORC || interacting)
        {
            noiseLevel = NoiseLevel.High;
        }
        else if (input.sprint && walking)
        {
            noiseLevel = NoiseLevel.Mid;
        }
        else if (walking)
        {
            noiseLevel = NoiseLevel.Low;
        }
        else
        {
            noiseLevel = NoiseLevel.None;
        }
    }

    public void SetORCUse(bool val)
    {
        usingORC = val;
    }

    public void SetInteract(bool val)
    {
        interacting = val;
    }

    public void SetInteractWithTime(float time)
    {
        interacting = true;
        StartCoroutine(Wait(time));
    }

    private IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time);
        interacting = false;
    }
}
