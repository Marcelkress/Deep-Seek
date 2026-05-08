using UnityEngine;

public class StationEngineScript : MonoBehaviour
{
    [SerializeField] private AK.Wwise.Event stationEngineStartEvent;


    public void stationEngineStart()

    {
        stationEngineStartEvent.Post(gameObject);
    }
    
    
    
    
}
