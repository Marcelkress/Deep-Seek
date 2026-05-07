using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DrillStationLever : MonoBehaviour, IInteractable
{
    private DrillStation station;
    private Animator anim;
    public int leverID;
    private bool canInteract;
    public UnityEvent PullEvent;
   
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canInteract = true;
        station = GetComponentInParent<DrillStation>();
        anim = GetComponent<Animator>();
    }

    public void Interact(GameObject playerObj)
    {
        if (canInteract && station.active)
        {
            PullEvent.Invoke();
            station.PullLever(leverID);
            anim.SetTrigger("Pull");
            canInteract = false;
        }
    }

    public void SetCanInteract()
    {
        canInteract = true;
    }

    public void SetLight()
    {
        station.SetEvalLight();
    }


   
}
