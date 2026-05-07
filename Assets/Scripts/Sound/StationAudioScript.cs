using UnityEngine;

public class StationAudioScript : MonoBehaviour
{
   public AK.Wwise.Event failedEvent;
   public AK.Wwise.Event stationActivatedEvent;
   public AK.Wwise.Event handleEvent;
   public AK.Wwise.Event valveEvent;
   public AK.Wwise.Event StationFinishedEvent;




   public void FailedCombination()

   {
      failedEvent.Post(gameObject);
      
   }
   
   
   public void StationActivated()
   
   {
      stationActivatedEvent.Post(gameObject);
   }
   
   public void HandleEvent()
   {
      handleEvent.Post(gameObject);
   }
   
   public void ValveEvent()
   {
      valveEvent.Post(gameObject);
   }

   public void StationFinished()
   {
      StationFinishedEvent.Post(gameObject);
   }

}
