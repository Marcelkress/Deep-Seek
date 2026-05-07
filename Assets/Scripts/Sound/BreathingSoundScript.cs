using StarterAssets;
using UnityEngine;
using System.Collections;

public class BreathingSoundScript : MonoBehaviour
{
    public AK.Wwise.Event breathingSoundWalk;
    public AK.Wwise.Event breathingSoundRun;
    
    private FirstPersonController firstPersonController;
    private StarterAssetsInputs input;
    private bool isInitialized = false;
    
    private enum BreathingState { None, Walking, Running }
    private BreathingState currentBreathingState = BreathingState.None;

    void Start()
    {
        // Persist this script across scene changes
        DontDestroyOnLoad(gameObject);
        
       
        // Find player when new scene loads
        StartCoroutine(FindPlayerWhenReady());
    }

    private IEnumerator FindPlayerWhenReady()
    {
        // Wait for player scene to load
        yield return new WaitForSeconds(0.5f);
        
        firstPersonController = FindObjectOfType<FirstPersonController>();
        
        if (firstPersonController != null)
        {
            input = firstPersonController.GetComponent<StarterAssetsInputs>();
            isInitialized = true;
            
            // Ensure walk breathing is still active and in the right state
            if (currentBreathingState == BreathingState.Walking)
            {
                breathingSoundWalk.Post(gameObject); // Re-post to ensure it's playing
                Debug.Log("Re-posting walk breathing in player scene");
            }
            
            Debug.Log("BreathingScript found FirstPersonController");
        }
        else
        {
            Debug.LogWarning("FirstPersonController not found yet, will keep searching...");
            StartCoroutine(FindPlayerWhenReady()); // Keep looking
        }
    }

    void FixedUpdate()
    {
        // Only control breathing when player is loaded
        if (isInitialized && firstPersonController != null)
        {
            SprintChecker();
        }
    }

    private void StopAllBreathing()
    {
        if (currentBreathingState == BreathingState.Walking)
        {
            breathingSoundWalk.Stop(gameObject);
            Debug.Log("Stopping walk breathing");
        }
        else if (currentBreathingState == BreathingState.Running)
        {
            breathingSoundRun.Stop(gameObject);
            Debug.Log("Stopping run breathing");
        }
        
        currentBreathingState = BreathingState.None;
    }

    public void StartBreathingSoundWalk()
    {
        if (currentBreathingState != BreathingState.Walking)
        {
            StopAllBreathing();
            breathingSoundWalk.Post(gameObject);
            currentBreathingState = BreathingState.Walking;
            Debug.Log("Starting walk breathing");
        }
    }

    public void StartBreathingSoundRun()
    {
        if (currentBreathingState != BreathingState.Running)
        {
            if (currentBreathingState == BreathingState.Walking)
            {
                breathingSoundWalk.Stop(gameObject);
                Debug.Log("Stopping walk breathing");
            }
            
            currentBreathingState = BreathingState.None;
            breathingSoundRun.Post(gameObject);
            currentBreathingState = BreathingState.Running;
            Debug.Log("Starting run breathing");
        }
    }

    public void SprintChecker()
    {
        if (input != null && input.sprint && firstPersonController.isMoving)
        {
            StartBreathingSoundRun();
        }
        else if (firstPersonController.isMoving)
        {
            StartBreathingSoundWalk();
        }
        // REMOVED: Stop breathing when idle - this was killing the walk sound
        // Now it keeps breathing even when standing still
    }
}