using UnityEngine;

public class KeyboardSound : MonoBehaviour
{
    public AK.Wwise.Event keyboardClickEvent;




    public void KeyboardClick()

    {
        keyboardClickEvent.Post(gameObject);
    }
    
    
    
}
