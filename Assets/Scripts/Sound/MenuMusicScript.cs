using UnityEngine;

public class MenuMusicScript : MonoBehaviour
{
    
    public AK.Wwise.Event startmenuMusic;
    public AK.Wwise.Event stopmenuMusic;
    public AK.Wwise.Event underwatersound;



    public void MenuMusicPlay()

    {
        startmenuMusic.Post(gameObject);
        
    }

    public void MenuMusicStop()
    {
        stopmenuMusic.Post(gameObject);
    }


    public void GetUnderWater()

    {
        underwatersound.Post(gameObject);
    }

}
