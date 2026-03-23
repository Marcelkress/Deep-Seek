using UnityEngine;

public class LightSlowFollow : MonoBehaviour
{
    private Camera camera;
    public float followSpeed;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPos = camera.transform.position;
        
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
        
        Quaternion target = camera.transform.rotation;
        
        transform.rotation = Quaternion.Lerp(transform.rotation, target, followSpeed * Time.deltaTime);
    }
}
