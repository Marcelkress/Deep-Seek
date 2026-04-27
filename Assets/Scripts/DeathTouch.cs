using System;
using UnityEngine;

public class DeathTouch : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("should die");
            other.gameObject.GetComponent<PlayerOxygen>().RemoveOxygen(10000);
        }
    }
}
