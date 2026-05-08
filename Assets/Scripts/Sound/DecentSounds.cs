using System;
using UnityEngine;

public class DecentSounds : MonoBehaviour
{
    [SerializeField] private AK.Wwise.Event impact;
    [SerializeField] private AK.Wwise.Event doorClose;
    [SerializeField] private AK.Wwise.Event dooropen;
    [SerializeField] private AK.Wwise.Event ambientDoorClose;
    [SerializeField] private AK.Wwise.Event ambientDecent;


    public bool hasplayed;


    private void Start()
    {
        hasplayed = false;
    }


    public void ImpactSound()

    {
        impact.Post(gameObject);

    }


    public void DoorCloseSound()

    {
        if (!hasplayed)
        {
            doorClose.Post(gameObject);    
        }
        
    }

    public void DoorOpenSound()

    {
        dooropen.Post(gameObject);
        
    }

    public void AmbientDoorCloseSound()
    {
        if (!hasplayed)
        {
            ambientDoorClose.Post(gameObject);  
        }
        
        
    }

    public void AmbientDecentSound()
    {
        if (!hasplayed)
        {
            ambientDecent.Post(gameObject);  
            hasplayed = true;
        }
        
        
    }
    



}
