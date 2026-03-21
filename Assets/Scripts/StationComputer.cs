using System;
using UnityEngine;

public class StationComputer : MonoBehaviour, IInteractable
{
    private DrillStation station;

    private void Start()
    {
        station = GetComponentInParent<DrillStation>();
    }

    public void Interact(GameObject playerObj)
    {
        if (!station.active)
        {
            station.ActivatePuzzle();
        }
    }
}
