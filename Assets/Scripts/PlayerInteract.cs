using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteract : MonoBehaviour
{
    public Camera playerCam;
    public float interactionDistance = 2f;
    private bool hit;
    private RaycastHit hitObj;
    public Image uiMarker;
    public float markerFadeValue = 0.5f, fadeDuration = 0.2f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (playerCam == null)
        {
            playerCam = GetComponentInChildren<Camera>();
        }
    }
    
    void Update()
    {
        hit = Physics.Raycast(playerCam.transform.position, playerCam.transform.TransformDirection(Vector3.forward), out RaycastHit newInfo, interactionDistance);

        if (!hit)
        {
            uiMarker.DOFade(markerFadeValue, fadeDuration);
            return;
        }

        hitObj = newInfo;
        
        if (hitObj.transform.GetComponent<IInteractable>() != null)
            uiMarker.DOFade(1, fadeDuration);
    }

    public void OnInteract(InputValue value)
    {
        if (value.isPressed && hitObj.transform != null) 
        {
            if (hitObj.transform.GetComponent<IInteractable>() != null)
            {
                // Interact with something 
                hitObj.transform.GetComponent<IInteractable>().Interact(this.gameObject);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (playerCam == null)
            return;
            
        Gizmos.color = Color.blue;
        Vector3 rayDirection = playerCam.transform.TransformDirection(Vector3.forward);
        Vector3 to = playerCam.transform.position + rayDirection * interactionDistance;
        Gizmos.DrawLine(playerCam.transform.position, to);
    }
}

public interface IInteractable
{
    public void Interact(GameObject playerObj);
}
