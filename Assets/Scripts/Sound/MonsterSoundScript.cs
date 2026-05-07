using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class MonsterSoundScript : MonoBehaviour
{
    [Header("Assign a Wwise Event here")] 
    public static List<AK.Wwise.State> ListOfStates = new List<AK.Wwise.State>();
    public AK.Wwise.Event MonsterSoundEvent;
    public AK.Wwise.State OnTriggerEnterState;
    public AK.Wwise.State OnTriggerExitState;

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


    private void OnTriggerEnter(Collider other)

    {
        if (other.tag == "Player")
        {
            ListOfStates.Insert(0, OnTriggerEnterState);
            ListOfStates[0].SetValue();
            
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            ListOfStates.Remove(OnTriggerEnterState);
            if (ListOfStates.Count > 0)
            {
                ListOfStates[0].SetValue();
            }
            else
            {
                OnTriggerExitState.SetValue();
            }

        }
    }

}