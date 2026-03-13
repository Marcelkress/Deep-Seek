using System.Collections;
using UnityEngine;

public class Flock : MonoBehaviour
{
    public float minSpeed = 20.0f;
    public float turnSpeed = 20.0f;
    public float randomFreq = 20.0f;
    public float randomForce = 20.0f;

    //alignment variables
    [Header("Alligment")] public float toOriginForce = 50.0f;

    public float toOriginRange = 100.0f;
    public float gravity = 2.0f;

    //seperation variables
    [Header("Seperation")] public float avoidanceRadius = 50.0f;

    public float avoidanceForce = 20.0f;

    //cohesion variables
    [Header("Cohesion")] public float followVelocity = 4.0f;

    public float followRadius = 40.0f;
    private Vector3 normalizedVelocity;
    private Transform[] objects;

    //these variables control the movement of the boid
    [Header("Movement of boid")] private Transform origin;

    private Vector3 originPush;
    private Flock[] otherFlocks;
    private float randomFreqInterval;
    private Vector3 randomPush;
    private Transform transformComponent;
    private Vector3 velocity;

    private void Start()
    {
        randomFreqInterval = 1.0f / randomFreq;

        // Assign the parent as origin
        origin = transform.parent;

        // Flock transform
        transformComponent = transform;

        // Temporary components
        Component[] tempFlocks = null;

        // Get all the unity flock components from the parent transform in the group
        if (transform.parent)
            tempFlocks = transform.parent
                .GetComponentsInChildren<Flock>();

        // Assign and store all the flock objects in this group
        objects = new Transform[tempFlocks.Length];
        otherFlocks = new Flock[tempFlocks.Length];

        for (var i = 0; i < tempFlocks.Length; i++)
        {
            objects[i] = tempFlocks[i].transform;
            otherFlocks[i] = (Flock)tempFlocks[i];
        }

        // Null Parent as the flock leader will be UnityFlockController object
        transform.parent = null;

        // Calculate random push depends on the random frequency provided
        StartCoroutine(UpdateRandom());
    }

    private void FixedUpdate()
    {
        //Internal variables
        var speed = velocity.magnitude;
        var avgVelocity = Vector3.zero;
        var avgPosition = Vector3.zero;
        var count = 0;
        var myPosition = transformComponent.position;
        Vector3 forceV;
        Vector3 toAvg;

        for (var i = 0; i < objects.Length; i++)
        {
            var boidTransform = objects[i];

            if (boidTransform != transformComponent)
            {
                var otherPosition = boidTransform.position;

                // Average position to calculate cohesion
                avgPosition += otherPosition;
                count++;

                //Directional vector from other flock to this flock
                forceV = myPosition - otherPosition;

                //Magnitude of that directional vector(Length)
                var directionMagnitude = forceV.magnitude;
                var forceMagnitude = 0.0f;

                if (directionMagnitude < followRadius)
                {
                    if (directionMagnitude < avoidanceRadius)
                        forceMagnitude = 1.0f - directionMagnitude / avoidanceRadius;

                    if (directionMagnitude > 0)
                        avgVelocity += forceV / directionMagnitude * forceMagnitude * avoidanceForce;
                }

                forceMagnitude = directionMagnitude / followRadius;

                var tempOtherBoid = otherFlocks[i];
                avgVelocity += followVelocity * forceMagnitude * tempOtherBoid.normalizedVelocity;
            }
        }

        if (count > 0)
        {
            //Calculate the average flock velocity(Alignment)
            avgVelocity /= count;

            //Calculate Center value of the flock(Cohesion)
            toAvg = avgPosition / count -
                    myPosition;
        }
        else
        {
            toAvg = Vector3.zero;
        }

        //Directional Vector to the leader
        forceV = origin.position - myPosition;

        var leaderDirectionMagnitude = forceV.magnitude;
        var leaderForceMagnitude = leaderDirectionMagnitude / toOriginRange;

        //Calculate the velocity of the flock to the leader
        if (leaderDirectionMagnitude > 0)
            originPush = leaderForceMagnitude *
                         toOriginForce *
                         (forceV /
                          leaderDirectionMagnitude);

        if (speed < minSpeed && speed > 0)
            velocity = velocity / speed * minSpeed;

        var wantedVel = velocity;

        //Calculate final velocity
        wantedVel -= wantedVel * Time.deltaTime;
        wantedVel += randomPush * Time.deltaTime;
        wantedVel += originPush * Time.deltaTime;
        wantedVel += avgVelocity * Time.deltaTime;
        wantedVel += gravity * Time.deltaTime *
                     toAvg.normalized;

        velocity = Vector3.RotateTowards(velocity,
            wantedVel, turnSpeed * Time.deltaTime,
            100.00f);

        transformComponent.rotation =
            Quaternion.LookRotation(velocity);

        //Move the flock based on the calculated velocity
        transformComponent.Translate(velocity *
                                     Time.deltaTime, Space.World);
        normalizedVelocity = velocity.normalized;
    }

    private IEnumerator UpdateRandom()
    {
        while (true)
        {
            randomPush = Random.insideUnitSphere * randomForce;

            yield return new WaitForSeconds(
                randomFreqInterval + Random.Range(
                    -randomFreqInterval / 2.0f,
                    randomFreqInterval / 2.0f));
        }
    }
}