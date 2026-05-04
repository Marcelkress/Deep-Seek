using System;
using UnityEngine;
using AK.Wwise;

public class ElevatorDecent : MonoBehaviour
{
    [SerializeField] AK.Wwise.Event elevatorAmbientEvent;
    [SerializeField] AK.Wwise.Event elevatordoorEvent;
    

    public bool DoorClosed;


    public void Awake()
    {
       DoorClosed=false;
    }


    public void StartElevatorAmbience()

    {
    
        elevatorAmbientEvent.Post(gameObject);
    
    }

    public void StartElevatorDoor()

    {
        if (!DoorClosed)
        {
            elevatordoorEvent.Post(gameObject);
            DoorClosed = true;
        }
        
    }

    public void stopElevatorAmbience()

    {
        
        elevatorAmbientEvent.Stop(gameObject);
        
    }


}



