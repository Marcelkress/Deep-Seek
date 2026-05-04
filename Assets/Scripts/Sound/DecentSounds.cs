using UnityEngine;

public class DecentSounds : MonoBehaviour
{
    [SerializeField] private AK.Wwise.Event impact;
    [SerializeField] private AK.Wwise.Event doorClose;
    [SerializeField] private AK.Wwise.Event dooropen;
    [SerializeField] private AK.Wwise.Event ambientDoorClose;
    [SerializeField] private AK.Wwise.Event ambientDecent;



   

    public void ImpactSound()

    {
        impact.Post(gameObject);

    }


    public void DoorCloseSound()

    {
        doorClose.Post(gameObject);
    }

    public void DoorOpenSound()

    {
        dooropen.Post(gameObject);
        
    }

    public void AmbientDoorCloseSound()
    {
        ambientDoorClose.Post(gameObject);
    }

    public void AmbientDecentSound()
    {
        ambientDecent.Post(gameObject);
    }
    



}
