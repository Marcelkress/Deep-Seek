using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class FlockController : MonoBehaviour
{
    public Vector3 bound;
    public float speed = 100.0f;
    public float targetReachedRadius = 10.0f;

    public GameObject fishPrefab;
    public int flockCount = 5;
    public bool wanderAroundTarget;
    public Transform target;

    public bool showBoundBox;

    private Vector3 initialPosition;
    private Vector3 nextMovementPoint;
    private Vector3 relativePos;

    // Use this for initialization
    private void Awake()
    {
        for (int i = 0; i < flockCount; i++)
        {
            InstantiateAsync(fishPrefab, transform, transform.position, Quaternion.identity);
        }
    }

    private void Start()
    {
        initialPosition = transform.position;
        CalculateNextMovementPoint();
    }

    // Update is called once per frame
    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        transform.rotation =
            Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(nextMovementPoint - transform.position),
                1.0f * Time.deltaTime);

        if (Vector3.Distance(nextMovementPoint, transform.position) <= targetReachedRadius)
            CalculateNextMovementPoint();
    }

    private void OnDrawGizmos()
    {
        if(showBoundBox)
            Gizmos.DrawWireCube(target.position, bound);
    }

    private void CalculateNextMovementPoint()
    {
        // Choose a random point within the bounds around initialPosition.
        // Previously the code generated absolute coordinates and then added initialPosition
        // again which caused the target to be far away. Use the generated coordinates
        // directly as the world-space target.

        relativePos = wanderAroundTarget ? target.position : initialPosition;

        var posX = Random.Range(relativePos.x - bound.x, relativePos.x + bound.x);

        var posY = Random.Range(relativePos.y - bound.y,
            relativePos.y + bound.y);
        var posZ = Random.Range(relativePos.z - bound.z,
            relativePos.z + bound.z);
        nextMovementPoint = new Vector3(posX,
            posY, posZ);
    }
}