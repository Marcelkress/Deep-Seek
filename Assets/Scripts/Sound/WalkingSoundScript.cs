using UnityEngine;

public class WalkingSoundScript : MonoBehaviour
{
   public AK.Wwise.Event walkingSound;
   //public AK.Wwise.Event runningSound;





   public void WalkingSound()

   {
      walkingSound.Post(gameObject);
      
   }

  /* public void RunningSound()
   {
      runningSound.Post(gameObject);
   }
   */

}
