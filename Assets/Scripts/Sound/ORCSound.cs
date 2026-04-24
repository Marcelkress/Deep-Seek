using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WwiseEventCaller : MonoBehaviour
{
    [Header("Assign a Wwise Event here")] 
    public AK.Wwise.Event ORCStartEvent;
    public AK.Wwise.Event ORCStopEvent;
    //public AK.Wwise.Event ORCLoopEvent;
    

    [Header("Optional: post on this GameObject instead of this component's object")]
    public GameObject orcstart;


    public void OrcStart()
    {
        GameObject postTarget = orcstart != null ? orcstart : gameObject;
        ORCStartEvent.Post(postTarget);
    }



    public void StopOrcStart()
    {
        ORCStartEvent.Stop(this.gameObject, 0);
        
        GameObject postTarget = orcstart != null ? orcstart : gameObject;

        if (ORCStartEvent == null)
        {
            Debug.LogWarning($"No Wwise event assigned on {name}", this);
            return;
        }

        ORCStopEvent.Post(postTarget);
    }

    /*public void LoopOrcStart()
    {
        GameObject postTarget = orcstart != null ? orcstart : gameObject;
        ORCLoopEvent.Post(postTarget);
    }

    public void LoopOrcStop()
    {
        GameObject postTarget = orcstart != null ? orcstart : gameObject;
        ORCLoopEvent.Stop(postTarget);
    }
    */

    public void OrcStop()
    {
        GameObject postTarget = orcstart != null ? orcstart : gameObject;
        ORCStopEvent.Post(postTarget);
    }

}