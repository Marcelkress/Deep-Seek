using UnityEngine;

public class GameMusic : MonoBehaviour
{
    public AK.Wwise.Event music;


    public void StartMusic()

    {
        music.Post(gameObject);
        
    }
    
    
}
