using System;
using System.Collections;
using UnityEngine;

public class WwiseEventCaller : MonoBehaviour
{
    [Header("Assign a Wwise Event here")]
    public AK.Wwise.Event MonsterSoundEvent;

    [Header("Optional: post on this GameObject instead of this component's object")]
    public GameObject monsterSound;

    public void Stop()
    {
        GameObject postTarget = monsterSound != null ? monsterSound : gameObject;

        if (MonsterSoundEvent == null)
        {
            Debug.LogWarning($"No Wwise event assigned on {name}", this);
            return;
        }

        MonsterSoundEvent.Stop(postTarget);
    }

    public void monstersound()
    {
        GameObject postTarget = monsterSound != null ? monsterSound : gameObject;
        MonsterSoundEvent.Post(postTarget);
    }

    IEnumerator MonstersoundCouroutine()
    {
        while (true)
        {
            monstersound();
            yield return new WaitForSeconds(30f);

        }


    }


    public void Start()
    {
        StartCoroutine(MonstersoundCouroutine());
    }
}