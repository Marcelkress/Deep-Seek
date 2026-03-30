 using System.Collections.Generic;
 using UnityEngine;

public class ValvePuzzle : MonoBehaviour
{
    /// <summary>
    /// Keep references to each valve in a dictionary
    /// Turn on computer to turn on the lights of each valve.
    /// When a valve is turned, its value is set to true
    /// </summary>

    public Dictionary<Valve, bool> valves;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        List<Valve> keys = new List<Valve>(valves.Keys);
        
        foreach (var key in keys)
        {
            valves[key] = false;
        }
    }

    public void OpenValve(Valve valve)
    {
        valves[valve] = true;
    }
}
