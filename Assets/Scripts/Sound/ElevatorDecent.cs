using UnityEngine;
using AK.Wwise;

public class ElevatorDecent : MonoBehaviour
{
    [SerializeField] AK.Wwise.Event elevatorAmbientEvent;
    [SerializeField] AK.Wwise.Event elevatordoorEvent;
    
    
    
    
    public void StartElevatorAmbience()

    {
    
        elevatorAmbientEvent.Post(gameObject);
    
    }

    public void StartElevatorDoor()

    {
        elevatordoorEvent.Post(gameObject);
    }

    public void stopElevatorAmbience()

    {
        
        elevatorAmbientEvent.Stop(gameObject);
        
    }


}



