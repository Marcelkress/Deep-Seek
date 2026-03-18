using UnityEngine;

public class MonsterMovement : MonoBehaviour
{
    public Transform target;
    public float maxSpeed = 10f;
    public float steerForce = 5f;
    public float orbitRadius = 15f;

    private Vector3 velocity;

    private void Update()
    {
        if (target == null) return;

        Vector3 toTarget = target.position - transform.position;
        float distance = Mathf.Max(toTarget.magnitude, 0.1f);

        Vector3 seekDirection = toTarget.normalized;
        Vector3 orbitDirection = Vector3.Cross(seekDirection, transform.up).normalized;

        float orbitFactor = Mathf.Clamp01(orbitRadius / distance);
        Vector3 desiredDirection = Vector3.Lerp(seekDirection, orbitDirection, orbitFactor).normalized;
        Vector3 desiredVelocity = desiredDirection * maxSpeed;

        Vector3 steering = Vector3.ClampMagnitude(desiredVelocity - velocity, steerForce);
        velocity = Vector3.ClampMagnitude(velocity + steering * Time.deltaTime, maxSpeed);

        transform.position += velocity * Time.deltaTime;

        if (velocity.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(velocity.normalized);
        }
    }
}