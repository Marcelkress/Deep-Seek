using UnityEngine;

public class JellyFishMovement : MonoBehaviour
{
    // slow movment up and down, but drifting more upwards.
    public float speed = 1f;
    public float driftSpeed = 0.5f;
    public float verticalAmplitude = 0.5f;

    public float verticalFrequency = 1f;


    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        float verticalOffset = Mathf.Sin(Time.time * verticalFrequency) * verticalAmplitude;
        Vector3 drift = new Vector3(0, driftSpeed * Time.deltaTime, 0);
        transform.position = initialPosition + new Vector3(0, verticalOffset, 0)
            + drift * Time.deltaTime;

        
    }

}
