using UnityEngine;

public class ValveComputer : MonoBehaviour, IInteractable
{
    private ValvePuzzle station;

    private void Start()
    {
        station = GetComponentInParent<ValvePuzzle>();
    }

    public void Interact(GameObject playerObj)
    {
        if (!station.active)
        {
            station.ActivatePuzzle();
        }
    }
}
