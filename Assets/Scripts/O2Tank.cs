using UnityEngine;

public class O2Tank : MonoBehaviour, IInteractable
{
    public int maxAmount, minAmount;
    
    public void Interact(GameObject playerObj)
    {
        int addAmount = Random.Range(minAmount, maxAmount);
        
        playerObj.GetComponent<PlayerOxygen>().AddOxygen(addAmount);
        
        // Hella nice effects
        
        Destroy(this.gameObject);
    }
}
