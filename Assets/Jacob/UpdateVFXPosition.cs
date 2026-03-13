using UnityEngine;
using UnityEngine.VFX;

public class UpdateVFXPosition : MonoBehaviour
{
    public VisualEffect vfx;  // Reference to the VFX Graph component
    public Transform character;  // Reference to the player character's transform

    void Update()
    {
        if (vfx != null && character != null)
        {
            vfx.SetVector3("Character Position", character.position);
        }
    }
}
