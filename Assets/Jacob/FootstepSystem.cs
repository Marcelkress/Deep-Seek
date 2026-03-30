using StarterAssets;
using UnityEngine;
using UnityEngine.VFX;

public class FootstepSystem : MonoBehaviour
{
    [SerializeField] private VisualEffect footstepVFX;
    [SerializeField] private LayerMask groundLayer;

    public void PlayFootstep()
    {
        if (footstepVFX == null) return;


        // Raycast origin is elevated from the character's base, and shifted purely on the X/Z plane
        Vector3 rayStart = transform.position + (Vector3.up * 0.5f);

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 2.0f, groundLayer))
        {
            footstepVFX.transform.position = hit.point + (hit.normal * 0.1f); // Slightly offset from the ground to prevent clipping
           //footstepVFX.transform.up = hit.normal;
            footstepVFX.SendEvent("DustVFX");
        }
    }

}