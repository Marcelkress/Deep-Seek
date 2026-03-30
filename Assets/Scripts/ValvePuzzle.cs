 using System.Collections.Generic;
 using UnityEngine;
 using System.Linq;
 using DG.Tweening;

 public class ValvePuzzle : MonoBehaviour
 {
     /// <summary>
     /// Keep references to each valve in a dictionary
     /// Turn on computer to turn on the lights of each valve.
     /// When a valve is turned, its value is set to true
     /// </summary>
     [Header("Valves")] public Valve[] valves;
     public float lightFadeTime = 1f, lightTargetIntensity;
    
    
    [Header("Lights and materials")] public MeshRenderer[] lights;
    public Material lightMat;
    public float emissionLevel;
    
    public bool active;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        active = false;
    }

    public void ActivatePuzzle()
    {
        active = true;
        InitializeLights();
        InitializeValveLights();
    }

    public void InitializeLights()
    {
        for (int i = 0; i < valves.Length; i++)
        {
            Debug.Log("hmm");
            MeshRenderer mesh = lights[i];
            Color col = Color.red;
            lightMat = new Material(lightMat);
            lightMat.EnableKeyword("_EMISSION");
            lightMat.SetColor("_EmissionColor", col * emissionLevel);
            lightMat.color = col;
            mesh.material = lightMat;
        }
    }

    public void InitializeValveLights()
    {
        foreach (var valve in valves)
        {
            valve.light.DOIntensity(lightTargetIntensity, lightFadeTime);
        }
    }

    public void OpenValve(Valve valve)
    {
        // Convert keys to a list to find the "index" of the current valve
        int index = System.Array.IndexOf(valves, valve);

        // Ensure the index is valid and falls within the lights array bounds
        if (index >= 0 && index < lights.Length)
        {
            MeshRenderer mesh = lights[index];
            Color col = Color.green;
            mesh.material.SetColor("_EmissionColor", col * emissionLevel);
            mesh.material.color = col;
        }
    }
}
