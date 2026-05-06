using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class Valve : MonoBehaviour, IInteractable
{
    public ValvePuzzle valvePuzzle;
    public float noiseTime = 0.8f;
    public Light light;
    public float lightFadeTime = 1f;
    public bool turned;

    public UnityEvent InteractEvent;

    private void Start()
    {
        light.intensity = 0;
    }

    public void Interact(GameObject playerObj)
    {
        if (!valvePuzzle.active)
            return;

        InteractEvent.Invoke();
        turned = true;
        valvePuzzle.OpenValve(this);
        playerObj.GetComponent<PlayerNoise>().SetInteractWithTime(noiseTime);
        light.DOColor(Color.green, lightFadeTime);
        GetComponentInChildren<Animator>().SetTrigger("Turn");
    }
}
