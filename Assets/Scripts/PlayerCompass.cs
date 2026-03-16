using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerCompass : MonoBehaviour
{
    private Transform playerTransform;
    public Image UINorthPointer;
    private Vector3 north;
    public float spinSpeed;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerTransform = GetComponentInParent<PlayerInput>().transform;
        north = Vector3.forward;
    }

    // Update is called once per frame
    void Update()
    {
        float angle = Vector3.SignedAngle(playerTransform.forward, north, Vector3.up);

        // Use Quaternion.Lerp instead of Vector3.Lerp for smooth rotation
        Quaternion targetRotation = Quaternion.Euler(0, 0, -angle + 90);
        Quaternion currentRotation = UINorthPointer.transform.rotation;
        
        UINorthPointer.transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, spinSpeed);
    }
}
