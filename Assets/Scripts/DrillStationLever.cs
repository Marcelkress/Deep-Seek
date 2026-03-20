using System.Collections;
using UnityEngine;

public class DrillStationLever : MonoBehaviour, IInteractable
{
    private DrillStation station;
    private Animator anim;
    public int leverID;
    public float colorResetWait = 2f;
    private Material mat;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        station = GetComponentInParent<DrillStation>();
        anim = GetComponent<Animator>();
        mat = new Material(GetComponent<MeshRenderer>().material);
    }

    public void Interact(GameObject playerObj)
    {
        station.PullLever(leverID);
        anim.SetTrigger("Pull");
        
        StartCoroutine(ResetColor());
    }

    private IEnumerator ResetColor()
    {
        yield return new WaitForSeconds(colorResetWait);
        
        GetComponent<MeshRenderer>().material = mat;
        GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", mat.color);
    }
}
