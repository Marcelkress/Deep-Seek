using UnityEngine;
using UnityEngine.Events;
using AK.Wwise;




public class O2Tank : MonoBehaviour, IInteractable
{
    public UnityEvent Pickuptank;
    public int maxAmount, minAmount;
    
    public void Interact(GameObject playerObj)
    {
        int addAmount = Random.Range(minAmount, maxAmount);
        
        playerObj.GetComponent<PlayerOxygen>().AddOxygen(addAmount);
        Pickuptank.Invoke();
        
        // Hella nice effects
        
        Destroy(this.gameObject);
    }
}
