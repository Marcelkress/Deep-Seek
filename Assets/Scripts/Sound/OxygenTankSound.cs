using UnityEngine;

public class OxygenTankSound : MonoBehaviour
{
   [SerializeField] private AK.Wwise.Event oxygenPickupEvent;


   public void OnOxygenPickup()

   {
       
       oxygenPickupEvent.Post(gameObject);
   }
  
}
